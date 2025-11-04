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

        // Start the device using the configuration
        deviceService.StartDevice(config.DevicePort, config.DeviceBaudrate, config.DeviceVid, config.DevicePid);

        // Clear device state on startup (turn off everything before applying config)
        await ClearDeviceState();
        await Task.Delay(200);

        pageManager.OnTouchPageChanged += OnTouchPageChanged;

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

    private void OnSimpleButtonPress(object sender, ButtonEventArgs e)
    {
        if (e.EventType != Constants.ButtonEventType.BUTTON_DOWN)
            return;

        var button = config.SimpleButtons.FirstOrDefault(b => b.Id == e.ButtonId);
        if (button != null)
        {
            commandService.ExecuteCommand(button.Command).GetAwaiter().GetResult();
        }
        else
        {
            switch (e.ButtonId)
            {
                case Constants.ButtonType.KNOB_TL:
                    commandService.ExecuteCommand(config.RotaryButtonPages[config.CurrentRotaryPageIndex]
                        .RotaryButtons[0].Command).GetAwaiter().GetResult();
                    break;
                case Constants.ButtonType.KNOB_CL:
                    commandService.ExecuteCommand(config.RotaryButtonPages[config.CurrentRotaryPageIndex]
                        .RotaryButtons[1].Command).GetAwaiter().GetResult();
                    break;
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
            
            commandService.ExecuteCommand(button.Command).GetAwaiter().GetResult();
            
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
        string command = e.ButtonId switch
        {
            Constants.ButtonType.KNOB_TL => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[0].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[0].RotaryRightCommand,
            Constants.ButtonType.KNOB_CL => e.Delta < 0
                ? config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[1].RotaryLeftCommand
                : config.RotaryButtonPages[config.CurrentRotaryPageIndex].RotaryButtons[1].RotaryRightCommand,
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
                        await deviceService.Device.DrawTouchButton(touchButton, config, true, config.DeviceColumns);
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

    public async Task ClearDeviceState()
    {
        try
        {
            Console.WriteLine("Clearing device state...");
            
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
}