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
                Name = "General + YouTube + Discord",
                Description = "Общий обход для сайтов, YouTube и Discord",
                Arguments = "--wf-l3=ipv4,ipv6 --wf-tcp=80,443 --hostlist=\"../lists/list-general.txt\" --hostlist=\"../lists/list-general-user.txt\" --dpi-desync=fake,split2 --dpi-desync-fooling=md5sig"
            },
            new ZapretPreset
            {
                Name = "Telegram Bypass",
                Description = "Обход голосовых серверов и медиа Telegram",
                Arguments = "--wf-l3=ipv4,ipv6 --wf-tcp=80,443,5222,5223,5228 --hostlist=\"../lists/list-general.txt\" --dpi-desync=fake,disorder2 --dpi-desync-repeats=4"
            },
            new ZapretPreset
            {
                Name = "Максимальный (Всё включено)",
                Description = "Комбинированный пресет под все сервисы и пользовательские списки",
                Arguments = "--wf-l3=ipv4,ipv6 --wf-tcp=80,443,5222-5228 --hostlist=\"../lists/list-general.txt\" --hostlist=\"../lists/list-general-user.txt\" --dpi-desync=fake,split2 --dpi-desync-repeats=6 --dpi-desync-fooling=md5sig"
            }
        };
    }
}