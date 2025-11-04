using System.ComponentModel;
using LoupixDeck.LoupedeckDevice;
using LoupixDeck.Models;
using LoupixDeck.Models.Extensions;
using LoupixDeck.Services;
using LoupixDeck.Utils;
using SkiaSharp;

namespace LoupixDeck.Controllers;

/// <summary>
/// Controller for the Razer Stream Controller device.
/// This device has:
/// - 6 rotary encoders (3 on left, 3 on right)
/// - 8 physical buttons below the screen (0-7)
/// - 4x3 touch grid in the center (12 buttons)
/// </summary>
public class RazerStreamControllerController(
    IDeviceService deviceService,
    ICommandService commandService,
    IPageManager pageManager,
    IConfigService configService,
    LoupedeckConfig config) : IDeviceController
{
    private readonly string _configPath = FileDialogHelper.GetConfigPath("config_razer.json");
    private readonly string _offConfigPath = FileDialogHelper.GetConfigPath("config_razer_off.json");
    private readonly string _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "RazerDefaultConfig.json");
    private readonly string _offTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "RazerOffConfig.json");

    public IPageManager PageManager => pageManager;

    public LoupedeckConfig Config => config;

    public async Task Initialize(string port = null, int baudrate = 0)
    {
        // Initialize config files on first run
        InitializeConfigFiles();

        // LOAD the actual saved config (don't use the injected empty one)
        var savedConfig = configService.LoadConfig<LoupedeckConfig>(_configPath);
        if (savedConfig != null)
        {
            Console.WriteLine("Loading saved config...");
            // Copy all properties from saved config to our config object
            config.DevicePort = savedConfig.DevicePort ?? port;
            config.DeviceBaudrate = savedConfig.DeviceBaudrate > 0 ? savedConfig.DeviceBaudrate : (baudrate > 0 ? baudrate : 921600);
            config.DeviceVid = savedConfig.DeviceVid;
            config.DevicePid = savedConfig.DevicePid;
            config.DeviceColumns = savedConfig.DeviceColumns;
            config.DeviceRows = savedConfig.DeviceRows;
            config.DeviceTouchButtonCount = savedConfig.DeviceTouchButtonCount;
            config.DeviceRotaryCount = savedConfig.DeviceRotaryCount;
            config.Brightness = savedConfig.Brightness;
            config.SimpleButtons = savedConfig.SimpleButtons;
            config.RotaryButtonPages = savedConfig.RotaryButtonPages;
            config.TouchButtonPages = savedConfig.TouchButtonPages;
            config.Wallpaper = savedConfig.Wallpaper;
            config.WallpaperOpacity = savedConfig.WallpaperOpacity;
            config.TouchFeedbackEnabled = savedConfig.TouchFeedbackEnabled;
            config.TouchFeedbackColor = savedConfig.TouchFeedbackColor;
            config.TouchFeedbackOpacity = savedConfig.TouchFeedbackOpacity;
        }
        else
        {
            Console.WriteLine("No saved config - using defaults");
            if (port != null)
                Config.DevicePort = port;
            if (baudrate > 0)
                Config.DeviceBaudrate = baudrate;
                
            // Set device-specific layout parameters for Razer Stream Controller
            Config.DeviceColumns = 4;
            Config.DeviceRows = 3;
            Config.DeviceTouchButtonCount = 14;
            Config.DeviceRotaryCount = 6;
        }

        // Start the device using the configuration
        deviceService.StartDevice(config.DevicePort, config.DeviceBaudrate, config.DeviceVid, config.DevicePid);

        // Apply OFF config first, then actual config (clean startup)
        await ApplyOffConfig();
        await Task.Delay(300);
        Console.WriteLine("Applying actual config...");

        pageManager.OnTouchPageChanged += OnTouchPageChanged;

        // Only initialize buttons if not loaded from config
        if (config.SimpleButtons == null || config.SimpleButtons.Length == 0)
        {
            Console.WriteLine("Initializing default buttons...");
            config.SimpleButtons =
            [
                await CreateSimpleButton(Constants.ButtonType.BUTTON0, Avalonia.Media.Colors.Blue, "System.PreviousPage"),
                await CreateSimpleButton(Constants.ButtonType.BUTTON1, Avalonia.Media.Colors.Green, ""),
                await CreateSimpleButton(Constants.ButtonType.BUTTON2, Avalonia.Media.Colors.Green, ""),
                await CreateSimpleButton(Constants.ButtonType.BUTTON3, Avalonia.Media.Colors.Blue, "System.NextPage"),
                await CreateSimpleButton(Constants.ButtonType.BUTTON4, Avalonia.Media.Colors.Red, ""),
                await CreateSimpleButton(Constants.ButtonType.BUTTON5, Avalonia.Media.Colors.Red, ""),
                await CreateSimpleButton(Constants.ButtonType.BUTTON6, Avalonia.Media.Colors.Red, ""),
                await CreateSimpleButton(Constants.ButtonType.BUTTON7, Avalonia.Media.Colors.Red, "")
            ];
        }
        else
        {
            Console.WriteLine($"Loaded {config.SimpleButtons.Length} buttons from saved config");
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

            // Render all touch buttons (including side displays)
            foreach (var touchButton in config.CurrentTouchButtonPage.TouchButtons)
            {
                if (touchButton.Index == 12 || touchButton.Index == 13)
                {
                    // Render side display buttons (left/right narrow displays) - 60×270
                    if (deviceService.Device is LoupixDeck.LoupedeckDevice.Device.RazerStreamControllerDevice razerDevice)
                    {
                        var bitmap = BitmapHelper.RenderTouchButtonContent(touchButton, config, 60, 270, 1);
                        if (bitmap != null)
                        {
                            touchButton.RenderedImage = bitmap;
                            await razerDevice.DrawSideDisplayButton(touchButton.Index, bitmap);
                        }
                    }
                }
                else if (touchButton.Index < 12)
                {
                    // Render center grid buttons (0-11)
                    await deviceService.Device.DrawTouchButton(touchButton, config, true, config.DeviceColumns);
                }
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

    private void OnSimpleButtonPress(object sender, ButtonEventArgs e)
    {
        if (e.EventType != Constants.ButtonEventType.BUTTON_DOWN)
            return;

        var button = config.SimpleButtons.FirstOrDefault(b => b.Id == e.ButtonId);
        if (button != null)
        {
            if (!string.IsNullOrEmpty(button.Command))
            {
                commandService.ExecuteCommand(button.Command).GetAwaiter().GetResult();
            }
        }
        else
        {
            // Handle rotary button presses (all 6 knobs)
            string command = null;
            switch (e.ButtonId)
            {
                case Constants.ButtonType.KNOB_TL:
                    command = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[0].Command;
                    break;
                case Constants.ButtonType.KNOB_CL:
                    command = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[1].Command;
                    break;
                case Constants.ButtonType.KNOB_BL:
                    command = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[2].Command;
                    break;
                case Constants.ButtonType.KNOB_TR:
                    command = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[3].Command;
                    break;
                case Constants.ButtonType.KNOB_CR:
                    command = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[4].Command;
                    break;
                case Constants.ButtonType.KNOB_BR:
                    command = config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[5].Command;
                    break;
            }
            
            if (!string.IsNullOrEmpty(command))
            {
                commandService.ExecuteCommand(command).GetAwaiter().GetResult();
            }
        }
    }

    private void OnTouchButtonPress(object sender, TouchEventArgs e)
    {
        if (e.EventType != Constants.TouchEventType.TOUCH_START)
            return;

        foreach (var touch in e.Touches)
        {
            var button = config.CurrentTouchButtonPage.TouchButtons.FindByIndex(touch.Target.Key);
            if (button == null) continue;

            // Visual feedback: Flash the button
            _ = ShowTouchFeedback(button);
            
            if (!string.IsNullOrEmpty(button.Command))
            {
                commandService.ExecuteCommand(button.Command).GetAwaiter().GetResult();
            }
            
            deviceService.Device.Vibrate();
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
            
            // Get dimensions based on button type
            var width = (button.Index == 12 || button.Index == 13) ? 60 : 90;
            var height = (button.Index == 12 || button.Index == 13) ? 270 : 90;
            
            // Create flash bitmap with overlay
            var flashBitmap = new SKBitmap(width, height);
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
                canvas.DrawRect(0, 0, width, height, paint);
            }
            
            // Render flash to DEVICE (not just GUI)
            if (button.Index == 12 || button.Index == 13)
            {
                // Side displays
                if (deviceService.Device is LoupixDeck.LoupedeckDevice.Device.RazerStreamControllerDevice razerDevice)
                {
                    await razerDevice.DrawSideDisplayButton(button.Index, flashBitmap);
                }
            }
            else
            {
                // Center grid buttons
                await deviceService.Device.DrawKey(button.Index, flashBitmap);
            }
            
            // Wait
            await Task.Delay(100);
            
            // Restore original to DEVICE
            if (originalImage != null)
            {
                if (button.Index == 12 || button.Index == 13)
                {
                    if (deviceService.Device is LoupixDeck.LoupedeckDevice.Device.RazerStreamControllerDevice razerDevice)
                    {
                        await razerDevice.DrawSideDisplayButton(button.Index, originalImage);
                    }
                }
                else
                {
                    await deviceService.Device.DrawKey(button.Index, originalImage);
                }
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
        // Handle rotation for all 6 knobs
        string command = e.ButtonId switch
        {
            Constants.ButtonType.KNOB_TL => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[0].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[0].RotaryRightCommand,
            Constants.ButtonType.KNOB_CL => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[1].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[1].RotaryRightCommand,
            Constants.ButtonType.KNOB_BL => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[2].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[2].RotaryRightCommand,
            Constants.ButtonType.KNOB_TR => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[3].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[3].RotaryRightCommand,
            Constants.ButtonType.KNOB_CR => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[4].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[4].RotaryRightCommand,
            Constants.ButtonType.KNOB_BR => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[5].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[5].RotaryRightCommand,
            _ => null
        };

        if (!string.IsNullOrEmpty(command))
        {
            commandService.ExecuteCommand(command).GetAwaiter().GetResult();
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
        }

        if (newIndex >= 0 && newIndex < config.TouchButtonPages.Count && config.TouchButtonPages[newIndex] != null)
        {
            foreach (var touchButton in config.TouchButtonPages[newIndex].TouchButtons)
            {
                touchButton.ItemChanged += TouchItemChanged;
            }
        }
    }

    private async void TouchItemChanged(object sender, EventArgs e)
    {
        if (sender is not TouchButton item) return;

        var button = config.CurrentTouchButtonPage.TouchButtons.FirstOrDefault(b => b.Index == item.Index);

        if (button == null) return;

        // Handle side display buttons specially
        if (button.Index == 12 || button.Index == 13)
        {
            if (deviceService.Device is LoupixDeck.LoupedeckDevice.Device.RazerStreamControllerDevice razerDevice)
            {
                var bitmap = BitmapHelper.RenderTouchButtonContent(button, config, 60, 270, 1);
                if (bitmap != null)
                {
                    button.RenderedImage = bitmap;
                    await razerDevice.DrawSideDisplayButton(button.Index, bitmap);
                }
            }
        }
        else if (button.Index < 12)
        {
            // Center grid buttons
            await deviceService.Device.DrawTouchButton(button, config, true, config.DeviceColumns);
        }
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
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"Created config directory: {directory}");
            }
            
            configService.SaveConfig(config, _configPath);
            Console.WriteLine($"Config saved to: {_configPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR saving config: {ex.Message}");
        }
    }

    private CancellationTokenSource _propertyChangedCts;
    
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

                case nameof(LoupedeckConfig.Wallpaper):
                case nameof(LoupedeckConfig.WallpaperOpacity):
                    await Task.Delay(100, token); // Debounce
                    foreach (var touchButton in config.CurrentTouchButtonPage.TouchButtons)
                    {
                        if (touchButton.Index == 12 || touchButton.Index == 13)
                        {
                            // Side display buttons (60×270)
                            if (deviceService.Device is LoupixDeck.LoupedeckDevice.Device.RazerStreamControllerDevice razerDevice)
                            {
                                var bitmap = BitmapHelper.RenderTouchButtonContent(touchButton, config, 60, 270, 1);
                                if (bitmap != null)
                                {
                                    touchButton.RenderedImage = bitmap;
                                    await razerDevice.DrawSideDisplayButton(touchButton.Index, bitmap);
                                }
                            }
                        }
                        else if (touchButton.Index < 12)
                        {
                            // Center grid buttons
                            await deviceService.Device.DrawTouchButton(touchButton, config, true, config.DeviceColumns);
                        }
                        await Task.Delay(0, token);
                    }
                    break;
            }
        }
        catch (TaskCanceledException)
        {
            // ignore canceled Tasks
        }
    }

    private void InitializeConfigFiles()
    {
        // Create OFF config if it doesn't exist (never overwrite)
        if (!File.Exists(_offConfigPath))
        {
            try
            {
                if (File.Exists(_offTemplatePath))
                {
                    File.Copy(_offTemplatePath, _offConfigPath);
                    Console.WriteLine($"Created OFF config from template");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create OFF config: {ex.Message}");
            }
        }

        // Create main config if it doesn't exist
        if (!File.Exists(_configPath))
        {
            try
            {
                if (File.Exists(_templatePath))
                {
                    File.Copy(_templatePath, _configPath);
                    Console.WriteLine($"Created main config from template");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create main config: {ex.Message}");
            }
        }
    }

    private async Task ApplyOffConfig()
    {
        try
        {
            Console.WriteLine("Applying OFF config (clearing device)...");
            var offConfig = configService.LoadConfig<LoupedeckConfig>(_offConfigPath);
            if (offConfig == null || offConfig.TouchButtonPages == null || offConfig.TouchButtonPages.Count == 0)
            {
                Console.WriteLine("OFF config not loaded properly");
                return;
            }
            
            // Set brightness to 0
            await deviceService.Device.SetBrightness(0);
            
            // Set all button LEDs to black
            foreach (var button in offConfig.SimpleButtons ?? [])
            {
                await deviceService.Device.SetButtonColor(button.Id, Avalonia.Media.Colors.Black);
            }
            
            // Render all BLACK touch buttons to display (clear the screen graphics)
            var touchButtons = offConfig.TouchButtonPages[0].TouchButtons;
            foreach (var touchButton in touchButtons)
            {
                if (touchButton.Index == 12 || touchButton.Index == 13)
                {
                    // Narrow displays
                    if (deviceService.Device is LoupixDeck.LoupedeckDevice.Device.RazerStreamControllerDevice razerDevice)
                    {
                        var bitmap = BitmapHelper.RenderTouchButtonContent(touchButton, offConfig, 60, 270, 1);
                        if (bitmap != null)
                        {
                            await razerDevice.DrawSideDisplayButton(touchButton.Index, bitmap);
                            bitmap.Dispose();
                        }
                    }
                }
                else if (touchButton.Index < 12)
                {
                    // Center grid
                    var bitmap = BitmapHelper.RenderTouchButtonContent(touchButton, offConfig, 90, 90, 4);
                    if (bitmap != null)
                    {
                        await deviceService.Device.DrawKey(touchButton.Index, bitmap);
                        bitmap.Dispose();
                    }
                }
            }
            
            Console.WriteLine("OFF config applied (device cleared - all black)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying OFF config: {ex.Message}");
        }
    }

    public async Task ClearDeviceState()
    {
        // Simply apply the OFF config
        await ApplyOffConfig();
    }

    public async Task RestoreDeviceState()
    {
        Console.WriteLine("Restoring device state from current config...");
        
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
                if (touchButton.Index == 12 || touchButton.Index == 13)
                {
                    // Render side displays
                    if (deviceService.Device is LoupixDeck.LoupedeckDevice.Device.RazerStreamControllerDevice razerDevice)
                    {
                        var bitmap = BitmapHelper.RenderTouchButtonContent(touchButton, config, 60, 270, 1);
                        if (bitmap != null)
                        {
                            touchButton.RenderedImage = bitmap;
                            await razerDevice.DrawSideDisplayButton(touchButton.Index, bitmap);
                        }
                    }
                }
                else if (touchButton.Index < 12)
                {
                    // Render center grid buttons
                    await deviceService.Device.DrawTouchButton(touchButton, config, true, config.DeviceColumns);
                }
            }
            Console.WriteLine("All touch buttons rendered");
        }
        
        Console.WriteLine("Device state fully restored!");
    }
}

