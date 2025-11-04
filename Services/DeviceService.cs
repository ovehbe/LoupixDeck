using LoupixDeck.Models;
using LoupixDeck.Registry;
using LoupedeckDeviceBase = LoupixDeck.LoupedeckDevice.Device.LoupedeckDevice;

namespace LoupixDeck.Services;

public interface IDeviceService
{
    LoupedeckDeviceBase Device { get; }
    void StartDevice(string devicePort, int deviceBaudrate, string vid = null, string pid = null);
    Task ShowTemporaryTextButton(int index, string text, int displayDurationMilliseconds);
}

public class LoupedeckDeviceService : IDeviceService
{
    private readonly IElgatoController _elgatoController;
    private readonly LoupedeckConfig _config;
    private readonly AutoResetEvent _deviceCreatedEvent = new(false);

    public LoupedeckDeviceBase Device { get; private set; }

    public LoupedeckDeviceService(IObsController obsController,
        IElgatoController elgatoController,
        ElgatoDevices elgatoDevices,
        LoupedeckConfig config)
    {
        _elgatoController = elgatoController;
        _config = config;

        obsController.Connect();

        _elgatoController.KeyLightFound += (_, light) =>
        {
            var checkDevice = elgatoDevices.KeyLights.FirstOrDefault(kl => kl.DisplayName == light.DisplayName);
            if (checkDevice != null)
            {
                elgatoDevices.RemoveKeyLight(checkDevice);
            }

            _elgatoController.InitDeviceAsync(light).GetAwaiter().GetResult();
            elgatoDevices.AddKeyLight(light);
        };
        _ = _elgatoController.ProbeForElgatoDevices();
    }

    public void StartDevice(string devicePort, int deviceBaudrate, string vid = null, string pid = null)
    {
        var deviceThread = new Thread(() =>
        {
            // If VID/PID provided, try to match to known device type
            if (!string.IsNullOrEmpty(vid) && !string.IsNullOrEmpty(pid))
            {
                var deviceInfo = DeviceRegistry.GetDeviceByVidPid(vid, pid);
                if (deviceInfo != null)
                {
                    // Create the appropriate device type
                    Device = (LoupedeckDeviceBase)Activator.CreateInstance(
                        deviceInfo.DeviceType,
                        null, devicePort, deviceBaudrate, true, LoupixDeck.LoupedeckDevice.Constants.DefaultReconnectInterval);
                    _deviceCreatedEvent.Set();
                    return;
                }
            }
            
            // Default to LoupedeckLiveSDevice for backward compatibility
            Device = new LoupixDeck.LoupedeckDevice.Device.LoupedeckLiveSDevice(null, devicePort, deviceBaudrate);
            _deviceCreatedEvent.Set();
        })
        {
            IsBackground = true
        };
        deviceThread.Start();
        _deviceCreatedEvent.WaitOne();
    }

    private int _currentCallId;

    public async Task ShowTemporaryTextButton(int index, string text, int displayDurationMilliseconds)
    {
        var callId = Interlocked.Increment(ref _currentCallId); // Atomically increment the call ID
        const int interval = 100; // Update interval in milliseconds
        var elapsed = 0; // Tracks the elapsed time

        while (elapsed < displayDurationMilliseconds)
        {
            if (callId != _currentCallId)
            {
                // Exit if a newer call has been made
                return;
            }

            await Device.DrawTextButton(index, text); // Update the text button
            await Task.Delay(interval); // Wait for the specified interval
            elapsed += interval; // Increment the elapsed time
        }

        // Only the last call executes this action
        if (callId == _currentCallId)
        {
            await Device.DrawTouchButton(
                _config.CurrentTouchButtonPage.TouchButtons[index], 
                _config,
                false,
                _config.DeviceColumns); // Reset the button
        }
    }
}