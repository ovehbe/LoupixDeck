using System.ComponentModel;
using LoupixDeck.LoupedeckDevice;
using LoupixDeck.Models;
using LoupixDeck.Models.Extensions;
using LoupixDeck.Services;
using LoupixDeck.Utils;
using SkiaSharp;

namespace LoupixDeck.Controllers;

/// <summary>
/// This controller orchestrates the collaboration of the services:
/// - It loads or saves the configuration,
/// - starts the device,
/// - registers the device events and
/// - forwards the UI events to the corresponding services.
/// </summary>
public class LoupedeckLiveSController(
    IDeviceService deviceService,
    ICommandService commandService,
    IPageManager pageManager,
    IConfigService configService,
    LoupedeckConfig config) : IDeviceController
{
    private readonly string _configPath = FileDialogHelper.GetConfigPath("config.json");
    
    private bool _isDeviceOff = false;
    private int? _activeTouchButtonIndex = null; // Track which button is currently being touched

    public IPageManager PageManager => pageManager;

    public LoupedeckConfig Config => config;

    public async Task Initialize(string port = null, int baudrate = 0)
    {
        if (port != null)
            Config.DevicePort = port;

        if (baudrate > 0)
            Config.DeviceBaudrate = baudrate;

        // Set device-specific layout parameters for Loupedeck Live S
        Config.DeviceColumns = 5;
        Config.DeviceRows = 3;
        Config.DeviceTouchButtonCount = 15; // 5x3
        Config.DeviceRotaryCount = 2;
        
        // Detect and update port AFTER config is set up (handles port changes after suspend/disconnect)
        DetectAndUpdatePort();

        // Start the device using the configuration
        deviceService.StartDevice(config.DevicePort, config.DeviceBaudrate, config.DeviceVid, config.DevicePid);

        // Clear device state on startup (turn off everything before applying config)
        await ClearDeviceState();
        await Task.Delay(200);

        pageManager.OnTouchPageChanged += OnTouchPageChanged;
        
        // Subscribe to current page property changes for wallpaper
        if (config.CurrentTouchButtonPage != null)
        {
            config.CurrentTouchButtonPage.PropertyChanged += PagePropertyChanged;
        }

        // Only initialize buttons if not loaded from config
        if (config.SimpleButtons == null || config.SimpleButtons.Length == 0)
        {
            config.SimpleButtons =
            [
                await CreateSimpleButton(Constants.ButtonType.BUTTON0, Avalonia.Media.Colors.Blue, "System.PreviousPage"),
                await CreateSimpleButton(Constants.ButtonType.BUTTON1, Avalonia.Media.Colors.Blue, "System.PreviousRotaryPage"),
                await CreateSimpleButton(Constants.ButtonType.BUTTON2, Avalonia.Media.Colors.Blue, "System.NextRotaryPage"),
                await CreateSimpleButton(Constants.ButtonType.BUTTON3, Avalonia.Media.Colors.Blue, "System.NextPage")
            ];
        }
        else
        {
            // Process loaded buttons (render images and attach events)
            foreach (var button in config.SimpleButtons)
            {
                // Render the button image for GUI
                Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                {
                    button.RenderedImage = BitmapHelper.RenderSimpleButtonImage(button, 90, 90);
                });
                
                // Attach event handler for live updates
                button.ItemChanged += SimpleButtonChanged;
                
                // Apply color to device
                await deviceService.Device.SetButtonColor(button.Id, button.ButtonColor);
            }
        }

        // Only create default pages if config is completely empty (first run)
        var isFirstRun = config.RotaryButtonPages == null || config.RotaryButtonPages.Count == 0;
        
        if (isFirstRun)
        {
            pageManager.AddRotaryButtonPage(true);
        }
        else
        {
            // Existing config: Just set current page index
            if (config.CurrentRotaryPageIndex < 0 || config.CurrentRotaryPageIndex >= config.RotaryButtonPages.Count)
                config.CurrentRotaryPageIndex = 0;
            pageManager.ApplyRotaryPage(config.CurrentRotaryPageIndex, true);
        }

        if (isFirstRun)
        {
            await pageManager.AddTouchButtonPage(true);
        }
        else
        {
            // Existing config: Just set current page index
            if (config.CurrentTouchPageIndex < 0 || config.CurrentTouchPageIndex >= config.TouchButtonPages.Count)
                config.CurrentTouchPageIndex = 0;
            await pageManager.ApplyTouchPage(config.CurrentTouchPageIndex, true);

            // With an existing config, we need to apply the item changed event to the current Touch Button Page
            foreach (var touchButton in config.CurrentTouchButtonPage.TouchButtons)
            {
                touchButton.ItemChanged += TouchItemChanged;
            }

            foreach (var touchButton in config.CurrentTouchButtonPage.TouchButtons)
            {
                await deviceService.Device.DrawTouchButton(touchButton, config, true, config.DeviceColumns);
            }
        }

        config.CurrentRotaryButtonPage.Selected = true;
        config.CurrentTouchButtonPage.Selected = true;

        config.PropertyChanged += ConfigOnPropertyChanged;

        await deviceService.Device.SetBrightness(config.Brightness / 100.0);

        InitButtonEvents();

        // Only save config if this is the first run (to create the file)
        if (isFirstRun)
        {
            SaveConfig();
        }

        await Task.CompletedTask;
    }

    private void InitButtonEvents()
    {
        var device = deviceService.Device;
        device.OnButton += OnSimpleButtonPress;
        device.OnTouch += OnTouchButtonPress;
        device.OnRotate += OnRotate;
    }

    /// <summary>
    /// Wraps a command with prefix and/or suffix if enabled
    /// </summary>
    private string WrapCommand(string originalCommand, bool prefixEnabled, string prefixCommand, bool suffixEnabled, string suffixCommand)
    {
        if (string.IsNullOrEmpty(originalCommand))
            return originalCommand;

        var result = originalCommand;
        
        if (prefixEnabled && !string.IsNullOrEmpty(prefixCommand))
        {
            result = $"{prefixCommand} && {result}";
        }
        
        if (suffixEnabled && !string.IsNullOrEmpty(suffixCommand))
        {
            result = $"{result} && {suffixCommand}";
        }
        
        return result;
    }

    private void OnSimpleButtonPress(object sender, ButtonEventArgs e)
    {
        if (e.EventType != Constants.ButtonEventType.BUTTON_DOWN)
            return;

        var button = config.SimpleButtons.FirstOrDefault(b => b.Id == e.ButtonId);
        if (button != null)
        {
            // Don't execute commands when device is OFF, unless EnableWhenOff is true
            if (_isDeviceOff && !button.EnableWhenOff)
                return;
                
            if (!string.IsNullOrEmpty(button.Command))
            {
                var wrappedCommand = WrapCommand(
                    button.Command,
                    config.CurrentRotaryButtonPage.SimpleButtonPrefixEnabled,
                    config.CurrentRotaryButtonPage.SimpleButtonPrefixCommand,
                    config.CurrentRotaryButtonPage.SimpleButtonSuffixEnabled,
                    config.CurrentRotaryButtonPage.SimpleButtonSuffixCommand
                );
                commandService.ExecuteCommand(wrappedCommand).GetAwaiter().GetResult();
            }
        }
        else
        {
            // Handle rotary button presses (for Loupedeck Live S with 2 knobs)
            RotaryButton rotaryButton = null;
            switch (e.ButtonId)
            {
                case Constants.ButtonType.KNOB_TL:
                    rotaryButton = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[0];
                    break;
                case Constants.ButtonType.KNOB_CL:
                    rotaryButton = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[1];
                    break;
            }
            
            if (rotaryButton != null)
            {
                // Don't execute commands when device is OFF, unless EnableWhenOff is true
                if (_isDeviceOff && !rotaryButton.EnableWhenOff)
                    return;
                    
                if (!string.IsNullOrEmpty(rotaryButton.Command))
                {
                    var wrappedCommand = WrapCommand(
                        rotaryButton.Command,
                        config.CurrentRotaryButtonPage.KnobPressPrefixEnabled,
                        config.CurrentRotaryButtonPage.KnobPressPrefixCommand,
                        config.CurrentRotaryButtonPage.KnobPressSuffixEnabled,
                        config.CurrentRotaryButtonPage.KnobPressSuffixCommand
                    );
                    commandService.ExecuteCommand(wrappedCommand).GetAwaiter().GetResult();
                }
            }
        }
    }

    private void OnTouchButtonPress(object sender, TouchEventArgs e)
    {
        // Don't execute commands or vibrate when device is OFF
        if (_isDeviceOff)
            return;

        // Handle TOUCH_END - reset the active touch tracking
        if (e.EventType == Constants.TouchEventType.TOUCH_END)
        {
            // Always cancel any pending firmware-level vibration on release
            deviceService.Device?.Vibrate(LoupedeckDevice.Constants.VibrationPattern.Off);
            _activeTouchButtonIndex = null;
            return;
        }
        
        // Only process TOUCH_START events for button actions
        if (e.EventType != Constants.TouchEventType.TOUCH_START)
            return;
        
        // Only process the changed touch (the button that was just pressed)
        if (e.ChangedTouch == null)
            return;
            
        var touch = e.ChangedTouch;
        var buttonIndex = touch.Target.Key;
        
        // Prevent multiple triggers while sliding - only process the first button touch
        if (_activeTouchButtonIndex.HasValue && _activeTouchButtonIndex.Value == buttonIndex)
            return;
        
        // If sliding to a different button, ignore it completely (no multi-press on slide)
        if (_activeTouchButtonIndex.HasValue && _activeTouchButtonIndex.Value != buttonIndex)
            return;
        
        // Mark this button as the active one
        _activeTouchButtonIndex = buttonIndex;
        
        var button = config.CurrentTouchButtonPage.TouchButtons.FindByIndex(buttonIndex);
        if (button == null)
            return;

        // Visual feedback: Flash the button
        _ = ShowTouchFeedback(button);
        
        if (!string.IsNullOrEmpty(button.Command))
        {
            var wrappedCommand = WrapCommand(
                button.Command,
                config.CurrentTouchButtonPage.TouchButtonPrefixEnabled,
                config.CurrentTouchButtonPage.TouchButtonPrefixCommand,
                config.CurrentTouchButtonPage.TouchButtonSuffixEnabled,
                config.CurrentTouchButtonPage.TouchButtonSuffixCommand
            );
            commandService.ExecuteCommand(wrappedCommand).GetAwaiter().GetResult();
        }
        
        // Vibrate only if enabled for this button
        if (button.VibrationEnabled)
        {
            deviceService.Device.Vibrate(button.VibrationPattern);
        }
    }

    private async Task ShowTouchFeedback(TouchButton button)
    {
        if (!config.TouchFeedbackEnabled)
            return;
            
        try
        {
            // Create flash overlay by rendering button with semi-transparent overlay
            var originalImage = button.RenderedImage;
            
            // Create flash bitmap with overlay
            var flashBitmap = new SKBitmap(90, 90);
            using (var canvas = new SKCanvas(flashBitmap))
            {
                // Draw original content
                if (originalImage != null)
                {
                    canvas.DrawBitmap(originalImage, 0, 0);
                }
                
                // Draw semi-transparent overlay
                using var paint = new SKPaint
                {
                    Color = config.TouchFeedbackColor.ToSKColor().WithAlpha((byte)(255 * config.TouchFeedbackOpacity))
                };
                canvas.DrawRect(0, 0, 90, 90, paint);
            }
            
            // Render flash to DEVICE
            await deviceService.Device.DrawKey(button.Index, flashBitmap);
            
            // Wait
            await Task.Delay(100);
            
            // Restore original to DEVICE
            if (originalImage != null)
            {
                await deviceService.Device.DrawKey(button.Index, originalImage);
            }
            
            flashBitmap.Dispose();
        }
        catch
        {
            // Ignore errors in feedback animation
        }
    }

    private void OnRotate(object sender, RotateEventArgs e)
    {
        // Get the rotary button first to check EnableWhenOff
        RotaryButton rotaryButton = e.ButtonId switch
        {
            Constants.ButtonType.KNOB_TL => config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[0],
            Constants.ButtonType.KNOB_CL => config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[1],
            _ => null
        };

        if (rotaryButton == null)
            return;

        // Don't execute commands when device is OFF, unless EnableWhenOff is true
        if (_isDeviceOff && !rotaryButton.EnableWhenOff)
            return;
        
        // Get the appropriate command based on rotation direction
        string command = e.Delta < 0 ? rotaryButton.RotaryLeftCommand : rotaryButton.RotaryRightCommand;

        if (!string.IsNullOrEmpty(command))
        {
            // Wrap with appropriate prefix/suffix based on rotation direction
            var wrappedCommand = e.Delta < 0
                ? WrapCommand(command, config.CurrentRotaryButtonPage.KnobLeftPrefixEnabled, config.CurrentRotaryButtonPage.KnobLeftPrefixCommand,
                    config.CurrentRotaryButtonPage.KnobLeftSuffixEnabled, config.CurrentRotaryButtonPage.KnobLeftSuffixCommand)
                : WrapCommand(command, config.CurrentRotaryButtonPage.KnobRightPrefixEnabled, config.CurrentRotaryButtonPage.KnobRightPrefixCommand,
                    config.CurrentRotaryButtonPage.KnobRightSuffixEnabled, config.CurrentRotaryButtonPage.KnobRightSuffixCommand);
            
            commandService.ExecuteCommand(wrappedCommand).GetAwaiter().GetResult();
        }
    }

    private void OnTouchPageChanged(int oldIndex, int newIndex)
    {
        if (oldIndex >= 0 && oldIndex < config.TouchButtonPages.Count && config.TouchButtonPages[oldIndex] != null)
        {
            foreach (var touchButton in config.TouchButtonPages[oldIndex].TouchButtons)
            {
                touchButton.ItemChanged -= TouchItemChanged;
            }
            // Unsubscribe from page property changes
            config.TouchButtonPages[oldIndex].PropertyChanged -= PagePropertyChanged;
        }

        if (newIndex >= 0 && newIndex < config.TouchButtonPages.Count && config.TouchButtonPages[newIndex] != null)
        {
            foreach (var touchButton in config.TouchButtonPages[newIndex].TouchButtons)
            {
                touchButton.ItemChanged += TouchItemChanged;
            }
            // Subscribe to page property changes for wallpaper updates
            config.TouchButtonPages[newIndex].PropertyChanged += PagePropertyChanged;
        }
    }

    private async void PagePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TouchButtonPage.Wallpaper) || e.PropertyName == nameof(TouchButtonPage.WallpaperOpacity))
        {
            _propertyChangedCts?.Cancel();
            _propertyChangedCts = new CancellationTokenSource();
            var token = _propertyChangedCts.Token;

            try
            {
                await Task.Delay(100, token); // Debounce
                foreach (var touchButton in config.CurrentTouchButtonPage.TouchButtons)
                {
                    await deviceService.Device.DrawTouchButton(touchButton, config, true, config.DeviceColumns);
                    await Task.Delay(0, token);
                }
            }
            catch (TaskCanceledException)
            {
                // ignore canceled Tasks
            }
        }
    }

    private async void TouchItemChanged(object sender, EventArgs e)
    {
        if (sender is not TouchButton item) return;

        var button = config.CurrentTouchButtonPage.TouchButtons.FirstOrDefault(b => b.Index == item.Index);

        if (button == null) return;

        await deviceService.Device.DrawTouchButton(button, config, true, config.DeviceColumns);
    }

    private async Task<SimpleButton> CreateSimpleButton(Constants.ButtonType id, Avalonia.Media.Color color,
        string command)
    {
        var button = config.SimpleButtons.FindById(id) ?? new SimpleButton
        {
            Id = id,
            Command = command,
            ButtonColor = color
        };

        Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
        {
            button.RenderedImage = BitmapHelper.RenderSimpleButtonImage(button, 90, 90);
        });

        button.ItemChanged += SimpleButtonChanged;

        await deviceService.Device.SetButtonColor(id, button.ButtonColor);

        return button;
    }

    private async void SimpleButtonChanged(object sender, EventArgs e)
    {
        if (sender is not SimpleButton button) return;

        button.RenderedImage = BitmapHelper.RenderSimpleButtonImage(button, 90, 90);
        await deviceService.Device.SetButtonColor(button.Id, button.ButtonColor);
    }

    public void SaveConfig()
    {
        configService.SaveConfig(config, _configPath);
    }

    private CancellationTokenSource _propertyChangedCts;
    
    private void UpdatePortInConfigFile(string configPath, string newPort)
    {
        try
        {
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Config file not found: {configPath}");
                return;
            }
            
            // Read the config file as text
            var jsonText = File.ReadAllText(configPath);
            
            // Parse as JSON
            var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonText);
            var root = jsonDoc.RootElement;
            
            // Create a dictionary from the JSON
            var configDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonText);
            
            if (configDict != null)
            {
                // Update ONLY the DevicePort field
                if (configDict.ContainsKey("DevicePort"))
                {
                    configDict["DevicePort"] = newPort;
                }
                else
                {
                    configDict.Add("DevicePort", newPort);
                }
                
                // Write back with pretty formatting
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var updatedJson = System.Text.Json.JsonSerializer.Serialize(configDict, options);
                File.WriteAllText(configPath, updatedJson);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating port in config file {configPath}: {ex.Message}");
        }
    }
    
    private void DetectAndUpdatePort()
    {
        try
        {
            // Only attempt detection if we have VID/PID to search for
            if (string.IsNullOrEmpty(config.DeviceVid) || string.IsNullOrEmpty(config.DevicePid))
            {
                Console.WriteLine("No VID/PID in config, skipping port detection");
                return;
            }
            
            Console.WriteLine($"Detecting device port for VID={config.DeviceVid}, PID={config.DevicePid}...");
            
            var devices = SerialDeviceHelper.ListSerialUsbDevices();
            var matchingDevice = devices.FirstOrDefault(d => 
                d.Vid?.ToLower() == config.DeviceVid.ToLower() && 
                d.Pid?.ToLower() == config.DevicePid.ToLower());
            
            if (matchingDevice != null && !string.IsNullOrEmpty(matchingDevice.DevNode))
            {
                if (matchingDevice.DevNode != config.DevicePort)
                {
                    Console.WriteLine($"Port changed: {config.DevicePort} -> {matchingDevice.DevNode}");
                    
                    // Update port in memory
                    var oldPort = config.DevicePort;
                    config.DevicePort = matchingDevice.DevNode;
                    
                    // Update ONLY the port field in the config file (don't overwrite other settings)
                    UpdatePortInConfigFile(_configPath, matchingDevice.DevNode);
                    
                    Console.WriteLine($"Updated port in config file: {oldPort} -> {matchingDevice.DevNode}");
                }
                else
                {
                    Console.WriteLine($"Port unchanged: {config.DevicePort}");
                }
            }
            else
            {
                Console.WriteLine($"Device not found - using configured port: {config.DevicePort}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error detecting port: {ex.Message}");
            // Continue with existing port on error
        }
    }
    
    private async void ConfigOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _propertyChangedCts?.Cancel();
        _propertyChangedCts = new CancellationTokenSource();
        var token = _propertyChangedCts.Token;

        try
        {
            switch (e.PropertyName)
            {
                case nameof(LoupedeckConfig.Brightness):
                    await Task.Delay(100, token); // Debounce
                    await deviceService.Device.SetBrightness(config.Brightness / 100.0);
                    break;

                // Wallpaper is now handled per-page via PagePropertyChanged event
            }
        }
        catch (TaskCanceledException)
        {
            // ignore canceled Tasks
        }
    }

    public async Task ClearDeviceState()
    {
        try
        {
            Console.WriteLine("Clearing device state...");
            
            // Set device to OFF mode (disables all button commands)
            _isDeviceOff = true;
            
            // Turn off display brightness
            await deviceService.Device.SetBrightness(0);
            
            // Turn off all button LEDs (set to black)
            if (config.SimpleButtons != null)
            {
                foreach (var button in config.SimpleButtons)
                {
                    await deviceService.Device.SetButtonColor(button.Id, Avalonia.Media.Colors.Black);
                }
            }
            
            Console.WriteLine("Device state cleared");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing device state: {ex.Message}");
        }
    }

    public async Task RestoreDeviceState()
    {
        Console.WriteLine("Restoring device state from current config...");
        
        // Set device to ON mode (enables all button commands)
        _isDeviceOff = false;
        
        // Restore brightness
        await deviceService.Device.SetBrightness(config.Brightness / 100.0);
        Console.WriteLine($"Brightness restored to {config.Brightness}");
        
        // Restore button LED colors
        if (config.SimpleButtons != null)
        {
            foreach (var button in config.SimpleButtons)
            {
                await deviceService.Device.SetButtonColor(button.Id, button.ButtonColor);
            }
            Console.WriteLine($"Restored {config.SimpleButtons.Length} button LEDs");
        }
        
        // EXPLICITLY re-render all touch buttons (same as initial load)
        if (config.TouchButtonPages != null && config.TouchButtonPages.Count > 0)
        {
            var touchButtons = config.CurrentTouchButtonPage.TouchButtons;
            Console.WriteLine($"Rendering {touchButtons.Count} touch buttons...");
            
            foreach (var touchButton in touchButtons)
            {
                await deviceService.Device.DrawTouchButton(touchButton, config, true, config.DeviceColumns);
            }
            Console.WriteLine("All touch buttons rendered");
        }
        
        Console.WriteLine("Device state fully restored!");
    }
    
    public bool IsDeviceOff()
    {
        return _isDeviceOff;
    }
    
    public async Task Reconnect()
    {
        Console.WriteLine("Reconnecting device...");
        
        try
        {
            // Detect and update port before reconnecting (handles port changes)
            DetectAndUpdatePort();
            
            // Restart the device connection
            deviceService.StartDevice(config.DevicePort, config.DeviceBaudrate);
            
            // Wait a moment for the connection to establish
            await Task.Delay(500);
            
            // Re-register event handlers
            var device = deviceService.Device;
            device.OnButton += OnSimpleButtonPress;
            device.OnTouch += OnTouchButtonPress;
            device.OnRotate += OnRotate;
            
            Console.WriteLine("Device reconnected successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reconnecting device: {ex.Message}");
            throw;
        }
    }
}
