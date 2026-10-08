using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ZapretDesktop.Core.Engines;

public class WindowsZapretEngine : IZapretEngine
{
    private readonly string _workDir;
    private Process? _process;

    public WindowsZapretEngine()
    {
        _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zapret");
        Directory.CreateDirectory(_workDir);
    }

    public bool IsInstalled => File.Exists(Path.Combine(_workDir, "winws.exe"));
    public bool IsRunning => _process is { HasExited: false } && Process.GetProcessesByName("winws").Length > 0;

    public async Task EnsureInstalledAsync(IProgress<double>? progress = null)
    {
        if (IsInstalled) return;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ZapretDesktop", "1.0"));

        var apiUrl = "https://api.github.com/repos/bol-van/zapret/releases/latest";
        var responseJson = await client.GetStringAsync(apiUrl);

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;
        var assets = root.GetProperty("assets");

        string? downloadUrl = null;
        foreach (var asset in assets.EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString();
            if (name != null && name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                break;
            }
        }

        if (string.IsNullOrEmpty(downloadUrl))
            throw new Exception("Не удалось найти ZIP-архив в релизе zapret.");

        var zipPath = Path.Combine(_workDir, "temp_zapret.zip");
        var extractDir = Path.Combine(_workDir, "temp_extract");

        try
        {
            var zipBytes = await client.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(zipPath, zipBytes);

            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);

            ZipFile.ExtractToDirectory(zipPath, extractDir);

            var winwsFile = Directory.GetFiles(extractDir, "winws.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (winwsFile == null)
                throw new FileNotFoundException("Файл winws.exe не найден в архиве.");

            var winwsDir = Path.GetDirectoryName(winwsFile)!;

            foreach (var file in Directory.GetFiles(winwsDir))
            {
                File.Copy(file, Path.Combine(_workDir, Path.GetFileName(file)), overwrite: true);
            }

            foreach (var sysFile in Directory.GetFiles(extractDir, "*WinDivert*", SearchOption.AllDirectories))
            {
                var dest = Path.Combine(_workDir, Path.GetFileName(sysFile));
                if (!File.Exists(dest))
                {
                    File.Copy(sysFile, dest, overwrite: true);
                }
            }
        }
        finally
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
        }
    }

    public void Start(string arguments)
    {
        Stop();

        var exePath = Path.Combine(_workDir, "winws.exe");
        if (!File.Exists(exePath))
            throw new FileNotFoundException("Файл winws.exe не найден.");

        var baseListsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lists"));
        Directory.CreateDirectory(baseListsDir);

        EnsureGeneralListExists(Path.Combine(baseListsDir, "list-general.txt"));
        EnsureUserListExists(Path.Combine(baseListsDir, "list-general-user.txt"));
        EnsureTelegramIpSetExists(Path.Combine(baseListsDir, "ipset-telegram.txt"));

        var formattedArguments = arguments.Replace("{LISTS}", baseListsDir);

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = formattedArguments,
            WorkingDirectory = _workDir,
            UseShellExecute = true,
            Verb = "runas",
            CreateNoWindow = true
        };

        _process = Process.Start(psi);
    }

    public void Stop()
    {
        if (_process is { HasExited: false })
        {
            _process.Kill();
            _process.Dispose();
            _process = null;
        }

        foreach (var proc in Process.GetProcessesByName("winws"))
        {
            try { proc.Kill(); } catch { }
        }
    }

    private static void EnsureUserListExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "# Пользовательские домены (один на строку)\n");
        }
    }

    private static void EnsureGeneralListExists(string filePath)
    {
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            var defaultDomains = new[]
            {
                "# DNS-over-HTTPS (DoH)",
                "dns.google", "dns.quad9.net", "dns.nextdns.io", "doh.opendns.com",
                "doh.cleanbrowsing.org", "freedns.controld.com", "wikimedia-dns.org", "cloudflare-dns.com",
                "",
                "# Cloudflare & CDN",
                "cloudflare-ech.com", "encryptedsni.com", "cloudflareaccess.com", "cloudflareapps.com",
                "cloudflarebolt.com", "cloudflareclient.com", "cloudflareinsights.com", "cloudflareok.com",
                "cloudflarepartners.com", "cloudflareportal.com", "cloudflarepreview.com", "cloudflareresolve.com",
                "cloudflaressl.com", "cloudflarestatus.com", "cloudflarestorage.com", "cloudflarestream.com",
                "cloudflaretest.com", "cloudfront.net",
                "",
                "# Discord",
                "dis.gd", "discord-attachments-uploads-prd.storage.googleapis.com", "discord.app",
                "discord.co", "discord.com", "discord.design", "discord.dev", "discord.gift",
                "discord.gifts", "discord.gg", "discord.media", "discord.new", "discord.store",
                "discord.status", "discord-activities.com", "discordactivities.com", "discordapp.com",
                "discordapp.net", "discordcdn.com", "discordmerch.com", "discordpartygames.com",
                "discordsays.com", "discordsez.com", "discordstatus.com", "zendesk.com",
                "",
                "# Twitch / Emotes / Media",
                "frankerfacez.com", "ffzap.com", "betterttv.net", "7tv.app", "7tv.io",
                "localizeapi.com", "klipy.com", "live-video.net",
                "",
                "# YouTube",
                "youtube.com", "googlevideo.com", "ytimg.com", "ggpht.com",
                "youtubei.googleapis.com", "jnn-pa.googleapis.com", "nhacmp3youtube.com", "yt.be",
                "",
                "# Telegram",
                "telegram.org", "t.me", "telegram.me", "telegra.ph", "tdlib.org", "tg.dev",
                "",
                "# SoundCloud & NSFW",
                "soundcloud.com", "sndcdn.com", "pornhub.com", "phncdn.com", "pornhubpremium.com",
                "",
                "# Steam",
                "steampowered.com", "steamcommunity.com", "steamstatic.com", "steamcontent.com",
                "steamcdn-a.akamaihd.net", "steamuserimages-a.akamaihd.net"
            };
            File.WriteAllLines(filePath, defaultDomains);
        }
    }

    private static void EnsureTelegramIpSetExists(string filePath)
    {
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            var telegramRanges = new[]
            {
                "# IPv4 Telegram DC1-DC5",
                "91.108.4.0/22",
                "91.108.8.0/22",
                "91.108.12.0/22",
                "91.108.16.0/22",
                "91.108.20.0/22",
                "91.108.56.0/22",
                "149.154.160.0/20",
                "185.76.151.0/24",
                "",
                "# IPv6 Telegram",
                "2001:67c:4e8::/48",
                "2001:b28:f23d::/48",
                "2a0a:f280::/32"
            };
            File.WriteAllLines(filePath, telegramRanges);
        }
    }
}