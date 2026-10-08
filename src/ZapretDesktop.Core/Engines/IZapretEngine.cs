namespace ZapretDesktop.Core.Engines;

public interface IZapretEngine
{
    bool IsInstalled { get; }
    bool IsRunning { get; }
    Task EnsureInstalledAsync(IProgress<double>? progress = null);
    void Start(string arguments);
    void Stop();
}