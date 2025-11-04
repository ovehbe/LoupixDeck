using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LoupixDeck.Controllers;
using LoupixDeck.Models;
using LoupixDeck.Services;
using LoupixDeck.Utils;
using LoupixDeck.ViewModels.Base;
using AsyncRelayCommand = CommunityToolkit.Mvvm.Input.AsyncRelayCommand;
using RelayCommand = LoupixDeck.Utils.RelayCommand;

namespace LoupixDeck.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    public ICommand RotaryButtonCommand { get; }
    public ICommand SimpleButtonCommand { get; }
    public ICommand TouchButtonCommand { get; }

    public ICommand AddRotaryPageCommand { get; }
    public ICommand DeleteRotaryPageCommand { get; }
    public ICommand RotaryPageButtonCommand { get; }


    public ICommand AddTouchPageCommand { get; }
    public ICommand DeleteTouchPageCommand { get; }
    public ICommand TouchPageButtonCommand { get; }

    public ICommand SettingsMenuCommand { get; }
    public ICommand AboutMenuCommand { get; }
    public ICommand QuitApplicationCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand MinimizeToTrayCommand { get; }

    public IDeviceController LoupedeckController { get; }

    public MainWindowViewModel(IDeviceController loupedeck,
        IDialogService dialogService,
        ISysCommandService sysCommandService,
        ISystemPowerService powerService)
    {
        LoupedeckController = loupedeck;

        sysCommandService.Initialize();
        
        // Suspend/resume monitoring disabled (manual commands available instead)
        // powerService.Suspending += OnSystemSuspending;
        // powerService.Resuming += OnSystemResuming;
        // powerService.StartMonitoring();

        _dialogService = dialogService;

        RotaryButtonCommand = new AsyncRelayCommand<RotaryButton>(RotaryButton_Click);
        SimpleButtonCommand = new AsyncRelayCommand<SimpleButton>(SimpleButton_Click);
        TouchButtonCommand = new AsyncRelayCommand<TouchButton>(TouchButton_Click);

        AddRotaryPageCommand = new Utils.RelayCommand(AddRotaryPageButton_Click);
        DeleteRotaryPageCommand = new Utils.RelayCommand(DeleteRotaryPageButton_Click);
        RotaryPageButtonCommand = new Utils.RelayCommand<int>(RotaryPageButton_Click);

        AddTouchPageCommand = new Utils.RelayCommand(AddTouchPageButton_Click);
        DeleteTouchPageCommand = new Utils.RelayCommand(DeleteTouchPageButton_Click);
        TouchPageButtonCommand = new Utils.RelayCommand<int>(TouchPageButton_Click);

        SettingsMenuCommand = new AsyncRelayCommand(SettingsMenuButton_Click);
        AboutMenuCommand = new AsyncRelayCommand(AboutMenuButton_Click);
        QuitApplicationCommand = new Utils.RelayCommand(QuitApplication);
        ShowWindowCommand = new Utils.RelayCommand(ShowWindow);
        MinimizeToTrayCommand = new Utils.RelayCommand(MinimizeToTray);
    }

    private void AddRotaryPageButton_Click()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoupedeckController.PageManager.AddRotaryButtonPage();
            LoupedeckController.SaveConfig();
        });
    }

    private void DeleteRotaryPageButton_Click()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoupedeckController.PageManager.DeleteRotaryButtonPage();
            LoupedeckController.SaveConfig();
        });
    }

    private void RotaryPageButton_Click(int page)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoupedeckController.PageManager.ApplyRotaryPage(page - 1);
        });
    }

    private void AddTouchPageButton_Click()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoupedeckController.PageManager.AddTouchButtonPage();
            LoupedeckController.SaveConfig();
        });
    }

    private void DeleteTouchPageButton_Click()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoupedeckController.PageManager.DeleteTouchButtonPage();
            LoupedeckController.SaveConfig();
        });
    }

    private void TouchPageButton_Click(int page)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoupedeckController.PageManager.ApplyTouchPage(page - 1);
        });
    }

    private async Task RotaryButton_Click(RotaryButton button)
    {
        await _dialogService.ShowDialogAsync<RotaryButtonSettingsViewModel, DialogResult>(vm => vm.Initialize(button)
        );

        LoupedeckController.SaveConfig();
    }

    private async Task SimpleButton_Click(SimpleButton button)
    {
        await _dialogService.ShowDialogAsync<SimpleButtonSettingsViewModel, DialogResult>(vm => vm.Initialize(button)
        );

        LoupedeckController.SaveConfig();
    }

    private async Task TouchButton_Click(TouchButton button)
    {
        await _dialogService.ShowDialogAsync<TouchButtonSettingsViewModel, DialogResult>(vm => vm.Initialize(button)
        );

        LoupedeckController.SaveConfig();
    }

    private async Task SettingsMenuButton_Click()
    {
        await _dialogService.ShowDialogAsync<SettingsViewModel, DialogResult>();
        LoupedeckController.SaveConfig();
    }
    
    private async Task AboutMenuButton_Click()
    {
        await _dialogService.ShowDialogAsync<AboutViewModel, DialogResult>();
        LoupedeckController.SaveConfig();
    }

    private void ShowWindow()
    {
        var window = WindowHelper.GetMainWindow();
        if (window != null)
        {
            window.Show();
            window.WindowState = Avalonia.Controls.WindowState.Normal;
            window.Activate();
        }
    }

    private void MinimizeToTray()
    {
        var window = WindowHelper.GetMainWindow();
        window?.Hide();
    }

    private void OnSystemSuspending(object sender, EventArgs e)
    {
        Console.WriteLine("System is suspending - clearing device state...");
        LoupedeckController.ClearDeviceState().GetAwaiter().GetResult();
    }

    private void OnSystemResuming(object sender, EventArgs e)
    {
        Console.WriteLine("System is resuming - restoring device state...");
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(1000); // Wait for device to wake up
            
            // Trigger a full config reload by refreshing all elements
            await LoupedeckController.PageManager.ApplyTouchPage(LoupedeckController.Config.CurrentTouchPageIndex);
            LoupedeckController.PageManager.ApplyRotaryPage(LoupedeckController.Config.CurrentRotaryPageIndex);
            await LoupedeckController.Config.SetBrightness(LoupedeckController.Config.Brightness / 100.0);
        });
    }

    public void QuitApplication()
    {
        try
        {
            Console.WriteLine("Quitting application - clearing device...");
            
            // Clear device state before quitting (apply OFF config)
            LoupedeckController.ClearDeviceState().GetAwaiter().GetResult();
            
            Console.WriteLine("Device cleared - waiting for commands to complete...");
            
            // Give MORE time for all device commands to complete (especially display rendering)
            Thread.Sleep(1500);
            
            Console.WriteLine("Exiting now.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during quit: {ex.Message}");
        }
        
        Environment.Exit(0);
    }
}