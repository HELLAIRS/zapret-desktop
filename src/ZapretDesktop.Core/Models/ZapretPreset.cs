namespace ZapretDesktop.Core.Models;

public class ZapretPreset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;

    public static List<ZapretPreset> GetDefaultPresets()
    {
        return new List<ZapretPreset>
        {
            new ZapretPreset
            {
                Name = "Оптимальный (YouTube + Discord + Telegram)",
                Description = "Обход блокировок по доменам. Прямой трафик Telegram не перехватывается и не ломается.",
                Arguments = "--wf-l3=ipv4,ipv6 --wf-tcp=80,443,50000-65535 --wf-udp=443,50000-65535 " +
                            "--filter-l7=http,tls --hostlist=\"{LISTS}\\list-general.txt\" --hostlist=\"{LISTS}\\list-general-user.txt\" " +
                            "--dpi-desync=fake,split2 --dpi-desync-cutoff=n4 --dpi-desync-repeats=6 --dpi-desync-fooling=md5sig"
            },
            new ZapretPreset
            {
                Name = "Альтернативный (Fake + Disorder2)",
                Description = "Для провайдеров с жестким DPI на YouTube и Discord (без вмешательства в Telegram)",
                Arguments = "--wf-l3=ipv4,ipv6 --wf-tcp=80,443,50000-65535 --wf-udp=443,50000-65535 " +
                            "--filter-l7=http,tls --hostlist=\"{LISTS}\\list-general.txt\" --hostlist=\"{LISTS}\\list-general-user.txt\" " +
                            "--dpi-desync=fake,disorder2 --dpi-desync-cutoff=n4 --dpi-desync-repeats=6 --dpi-desync-fooling=md5sig"
            },
            new ZapretPreset
            {
                Name = "Telegram IP Desync (Только если IP заблокированы)",
                Description = "Использовать ТОЛЬКО если провайдер полностью блокирует IP-адреса Telegram (DC1-DC5)",
                Arguments = "--wf-l3=ipv4 --wf-tcp=80,443,5222,5223 " +
                            "--ipset=\"{LISTS}\\ipset-telegram.txt\" " +
                            "--dpi-desync=split2 --dpi-desync-cutoff=n1"
            }
        };
    }
}