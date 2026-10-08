using System.Text.Json;
using ZapretDesktop.Core.Models;

namespace ZapretDesktop.Core.Services;

public class PresetManager
{
    private readonly string _presetsFilePath;

    public PresetManager(string? customPath = null)
    {
        string dir = customPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
        Directory.CreateDirectory(dir);
        _presetsFilePath = Path.Combine(dir, "presets.json");
    }

    public List<Preset> GetDefaultPresets()
    {
        return new List<Preset>
        {
            new()
            {
                Id = "discord-youtube-general",
                Name = "Discord + YouTube (General)",
                Description = "Универсальный стратег под Discord (Voice + Media) и 4K YouTube",
                Category = "Universal",
                Arguments = "--wf-tcp=80,443 --wf-udp=443,50000-65535 --dpi-desync=fake,split2 --dpi-desync-repeats=6 --dpi-desync-fooling=md5sig",
                IsBuiltIn = true
            },
            new()
            {
                Id = "youtube-alt",
                Name = "YouTube Alt (Fake TLS)",
                Description = "Обход замедления YouTube через подмену TLS ClientHello",
                Category = "YouTube",
                Arguments = "--wf-tcp=80,443 --wf-udp=443 --dpi-desync=fake --dpi-desync-fake-tls=\"bin/tls_clienthello_www_google_com.bin\"",
                IsBuiltIn = true
            },
            new()
            {
                Id = "telegram-speedup",
                Name = "Telegram Speedup & Voice",
                Description = "Ускорение медиапотоков и голосовых вызовов Telegram",
                Category = "Telegram",
                Arguments = "--wf-tcp=80,443 --wf-udp=443,1400,5222 --dpi-desync=fake,disorder2",
                IsBuiltIn = true
            }
        };
    }

    public async Task<List<Preset>> LoadPresetsAsync()
    {
        if (!File.Exists(_presetsFilePath))
        {
            var defaults = GetDefaultPresets();
            await SavePresetsAsync(defaults);
            return defaults;
        }

        try
        {
            string json = await File.ReadAllTextAsync(_presetsFilePath);
            var presets = JsonSerializer.Deserialize<List<Preset>>(json);
            return presets ?? GetDefaultPresets();
        }
        catch
        {
            return GetDefaultPresets();
        }
    }

    public async Task SavePresetsAsync(List<Preset> presets)
    {
        string json = JsonSerializer.Serialize(presets, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_presetsFilePath, json);
    }
}