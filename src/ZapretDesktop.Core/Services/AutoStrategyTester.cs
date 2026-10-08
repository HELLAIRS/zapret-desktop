using ZapretDesktop.Core.Models;

namespace ZapretDesktop.Core.Services;

public class AutoStrategyTester
{
    private readonly ProcessManager _processManager;
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(4) };

    public event Action<string>? TestProgressLog;

    public AutoStrategyTester(ProcessManager processManager)
    {
        _processManager = processManager;
    }

    public async Task<Preset?> TestStrategiesAsync(IEnumerable<Preset> presets, string testUrl = "https://www.youtube.com", CancellationToken ct = default)
    {
        foreach (var preset in presets)
        {
            if (ct.IsCancellationRequested) break;

            TestProgressLog?.Invoke($"[AUTO-TEST] Проверка пресета: {preset.Name}...");

            _processManager.Stop();
            await Task.Delay(500, ct);

            if (!_processManager.Start(preset.Arguments))
            {
                TestProgressLog?.Invoke($"[AUTO-TEST] Ошибка запуска winws для {preset.Name}");
                continue;
            }

            await Task.Delay(1500, ct);

            bool success = await CheckUrlAccessAsync(testUrl, ct);

            if (success)
            {
                TestProgressLog?.Invoke($"[SUCCESS] Рабочая стратегия найдена: {preset.Name}");
                _processManager.Stop();
                return preset;
            }

            TestProgressLog?.Invoke($"[FAIL] Пресет {preset.Name} не дал доступа.");
        }

        _processManager.Stop();
        return null;
    }

    private async Task<bool> CheckUrlAccessAsync(string url, CancellationToken ct)
    {
        try
        {
            var response = await HttpClient.GetAsync(url, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}