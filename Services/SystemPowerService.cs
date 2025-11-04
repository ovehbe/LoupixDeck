using System.Diagnostics;

namespace LoupixDeck.Services;

public interface ISystemPowerService
{
    event EventHandler Suspending;
    event EventHandler Resuming;
    void StartMonitoring();
}

public class SystemPowerService : ISystemPowerService
{
    public event EventHandler Suspending;
    public event EventHandler Resuming;
    
    private Process _monitorProcess;
    private bool _isMonitoring;

    public void StartMonitoring()
    {
        if (_isMonitoring) return;
        _isMonitoring = true;

        // Monitor systemd sleep events on Linux
        Task.Run(() => MonitorSystemdEvents());
    }

    private void MonitorSystemdEvents()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dbus-monitor",
                Arguments = "--system \"type='signal',interface='org.freedesktop.login1.Manager',member='PrepareForSleep'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _monitorProcess = Process.Start(psi);
            if (_monitorProcess == null) return;

            while (!_monitorProcess.StandardOutput.EndOfStream)
            {
                var line = _monitorProcess.StandardOutput.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                // PrepareForSleep(true) = going to sleep
                // PrepareForSleep(false) = waking up
                if (line.Contains("boolean true"))
                {
                    Console.WriteLine("System suspending...");
                    Suspending?.Invoke(this, EventArgs.Empty);
                }
                else if (line.Contains("boolean false"))
                {
                    Console.WriteLine("System resuming...");
                    Resuming?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Power monitoring not available: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _monitorProcess?.Kill();
        _monitorProcess?.Dispose();
    }
}

