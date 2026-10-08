using System.Diagnostics;

namespace ZapretDesktop.Core;

public class ProcessManager
{
    private Process? _winwsProcess;
    private readonly string _binPath;

    public bool IsRunning => _winwsProcess is { HasExited: false };

    public event Action<string>? LogReceived;
    public event Action<int>? ProcessExited;

    public ProcessManager(string? customBinPath = null)
    {
        _binPath = customBinPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
    }

    public bool Start(string arguments)
    {
        if (IsRunning)
        {
            Stop();
        }

        string exePath = Path.Combine(_binPath, "winws.exe");

        if (!File.Exists(exePath))
        {
            LogReceived?.Invoke($"[ERROR] Executable not found: {exePath}");
            return false;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            WorkingDirectory = _binPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            _winwsProcess = new Process { StartInfo = startInfo };

            _winwsProcess.OutputDataReceived += (s, e) => { if (e.Data != null) LogReceived?.Invoke(e.Data); };
            _winwsProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) LogReceived?.Invoke($"[ERR] {e.Data}"); };

            _winwsProcess.EnableRaisingEvents = true;
            _winwsProcess.Exited += (s, e) =>
            {
                int exitCode = _winwsProcess?.ExitCode ?? -1;
                LogReceived?.Invoke($"[INFO] Process winws.exe exited with code {exitCode}");
                ProcessExited?.Invoke(exitCode);
            };

            bool started = _winwsProcess.Start();

            if (started)
            {
                _winwsProcess.BeginOutputReadLine();
                _winwsProcess.BeginErrorReadLine();
                LogReceived?.Invoke("[INFO] winws.exe started successfully.");
            }

            return started;
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"[EXCEPTION] Failed to start winws: {ex.Message}");
            return false;
        }
    }

    public void Stop()
    {
        if (_winwsProcess == null || _winwsProcess.HasExited) return;

        try
        {
            _winwsProcess.Kill(entireProcessTree: true);
            _winwsProcess.WaitForExit(2000);
            LogReceived?.Invoke("[INFO] winws.exe process stopped.");
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"[ERROR] Failed to stop process: {ex.Message}");
        }
        finally
        {
            _winwsProcess?.Dispose();
            _winwsProcess = null;
        }
    }
}