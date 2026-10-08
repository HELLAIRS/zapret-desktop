namespace ZapretDesktop.Core.Models;

public class ZapretConfig
{
    public string SelectedPresetId { get; set; } = "default";
    
    // Протоколы TCP / UDP
    public bool EnableTcp { get; set; } = true;
    public bool EnableUdp { get; set; } = true;
    public string TcpPorts { get; set; } = "80,443";
    public string UdpPorts { get; set; } = "443,50000-65535";

    // Списки и Сервисы
    public bool EnableTelegramBypass { get; set; } = true;
    public List<string> SelectedLists { get; set; } = ["list-general.txt"];

    // Настройки для опытных пользователей
    public string DesyncStrategy { get; set; } = "fake,split2";
    public int FoolingTtl { get; set; } = 8;
    public bool EnableWssize { get; set; } = false;
    public string CustomRawArgs { get; set; } = string.Empty;

    public string BuildCommandLine(string listsDir)
    {
        var sb = new List<string>();

        if (EnableTcp)
        {
            sb.Add($"--wf-tcp={TcpPorts}");
        }

        if (EnableUdp)
        {
            sb.Add($"--wf-udp={UdpPorts}");
        }

        if (!string.IsNullOrWhiteSpace(DesyncStrategy))
        {
            sb.Add($"--dpi-desync={DesyncStrategy}");
        }

        if (FoolingTtl > 0)
        {
            sb.Add($"--dpi-desync-ttl={FoolingTtl}");
        }

        foreach (var list in SelectedLists)
        {
            string path = Path.Combine(listsDir, list);
            if (File.Exists(path))
            {
                sb.Add($"--hostlist=\"{path}\"");
            }
        }

        if (EnableTelegramBypass)
        {
            string tgList = Path.Combine(listsDir, "telegram.txt");
            if (File.Exists(tgList))
            {
                sb.Add($"--hostlist=\"{tgList}\"");
            }
        }

        if (!string.IsNullOrWhiteSpace(CustomRawArgs))
        {
            sb.Add(CustomRawArgs);
        }

        return string.Join(" ", sb);
    }
}