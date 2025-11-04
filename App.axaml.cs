using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LoupixDeck.Views;
using LoupixDeck.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using LoupixDeck.Utils;
using System.Runtime.Versioning;

namespace LoupixDeck;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

#if WINDOWS
    [SupportedOSPlatform("windows")]
#endif
    public override async void OnFrameworkInitializationCompleted()
    {
        DisableAvaloniaDataAnnotationValidation();

        var configPath = FileDialogHelper.GetConfigPath("config.json");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (!File.Exists(configPath))
            {
                var initWindow = new InitSetup
                {
                    DataContext = new InitSetupViewModel()
                };
                
                initWindow.Closed += async (_, _) =>
                {
                    if (initWindow.DataContext is InitSetupViewModel { ConnectionWorking: true } vm)
                    {
                        await InitializeMainWindow(vm.SelectedDevice.Path, vm.SelectedBaudRate, vm.SelectedDevice.Vid, vm.SelectedDevice.Pid, desktop);
                    }
                    else
                    {
                        desktop.Shutdown();
                    }
                };

                initWindow.Show();
            }
            else
            {
                await InitializeMainWindow(null, 0, null, null, desktop);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeMainWindow(string port, int baudRate, string vid, string pid, IClassicDesktopStyleApplicationLifetime desktop)
    {
        var splashScreen = new SplashScreen();
        desktop.MainWindow = splashScreen;
        splashScreen.Show();

        try
        {
            var viewModel = await CreateMainWindowViewModel(port, baudRate, vid, pid);
            OnViewModelCreated(viewModel, splashScreen, desktop);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
            desktop.Shutdown();
        }
    }

    private void OnViewModelCreated(MainWindowViewModel viewModel, SplashScreen splashScreen,
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        // UI-Thread verwenden, um Änderungen an der UI vorzunehmen
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            splashScreen.Close();
        });
    }

    private async Task<MainWindowViewModel> CreateMainWindowViewModel(string port = null, int baudrate = 0, string vid = null, string pid = null)
    {
        var collection = new ServiceCollection();
        collection.AddCommonServices();

        var services = collection.BuildServiceProvider();

        services.PostInit();

        // Determine which controller to use based on VID/PID
        Controllers.IDeviceController controller;
        if (!string.IsNullOrEmpty(vid) && !string.IsNullOrEmpty(pid))
        {
            var deviceInfo = Registry.DeviceRegistry.GetDeviceByVidPid(vid, pid);
            if (deviceInfo?.Name == "Razer Stream Controller")
            {
                controller = services.GetRequiredService<Controllers.RazerStreamControllerController>();
            }
            else
            {
                controller = services.GetRequiredService<Controllers.LoupedeckLiveSController>();
            }
        }
        else
        {
            // Default to Loupedeck Live S for backward compatibility
            controller = services.GetRequiredService<Controllers.LoupedeckLiveSController>();
        }

        // Create MainWindowViewModel with the selected controller
        var dialogService = services.GetRequiredService<Services.IDialogService>();
        var sysCommandService = services.GetRequiredService<Services.ISysCommandService>();
        var powerService = services.GetRequiredService<Services.ISystemPowerService>();
        var mainViewModel = new MainWindowViewModel(controller, dialogService, sysCommandService, powerService);

        // Initialize the controller (which will start the device with VID/PID)
        // We need to update the config with VID/PID first so the controller can pass it to deviceService
        if (!string.IsNullOrEmpty(vid) && !string.IsNullOrEmpty(pid))
        {
            controller.Config.DeviceVid = vid;
            controller.Config.DevicePid = pid;
        }
        
        await controller.Initialize(port, baudrate);

        return mainViewModel;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}