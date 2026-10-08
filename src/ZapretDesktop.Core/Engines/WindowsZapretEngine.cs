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
    public bool IsRunning => _process is { HasExited: false };

    public async Task EnsureInstalledAsync(IProgress<double>? progress = null)
    {
        if (IsInstalled) return;

        using var client = new HttpClient();
        // GitHub API требует заголовок User-Agent
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ZapretDesktop", "1.0"));

        // 1. Запрашиваем информацию о последнем релизе
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
        {
            throw new Exception("Не удалось найти ZIP-архив в последнем релизе zapret.");
        }

        var zipPath = Path.Combine(_workDir, "temp_zapret.zip");
        var extractDir = Path.Combine(_workDir, "temp_extract");

        try
        {
            // 2. Скачиваем архив
            var zipBytes = await client.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(zipPath, zipBytes);

            // 3. Распаковываем
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);

            ZipFile.ExtractToDirectory(zipPath, extractDir);

            // 4. Ищем winws.exe и копируем бинарники/DLL в рабочую папку
            var winwsFile = Directory.GetFiles(extractDir, "winws.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (winwsFile == null)
            {
                throw new FileNotFoundException("Файл winws.exe не найден внутри скачанного архива.");
            }

            var winwsDir = Path.GetDirectoryName(winwsFile)!;
            foreach (var file in Directory.GetFiles(winwsDir))
            {
                var dest = Path.Combine(_workDir, Path.GetFileName(file));
                File.Copy(file, dest, overwrite: true);
            }
        }
        finally
        {
            // Очищаем временные файлы
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

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
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
}