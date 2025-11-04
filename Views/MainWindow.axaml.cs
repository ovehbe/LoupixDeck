using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform;
using LoupixDeck.Utils;
using LoupixDeck.ViewModels;

namespace LoupixDeck.Views;

public partial class MainWindow : Window
{
    private static TrayIcon _trayIcon;
    private bool _isMinimizedToTray;

    // Static Commands
    private ICommand ShowCommand { get; }
    private ICommand QuitCommand { get; }

    private static MainWindow Instance { get; set; }
    
    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        
        Instance = this;

        ShowCommand = new RelayCommand(() => Instance?.ShowFromTray());
        QuitCommand = new RelayCommand(() => Instance?.QuitApplication());

        // Tray icon disabled for now (causes crash on some systems)
        // CreateTrayIcon();

        this.Closing += OnWindowClosing;
        
        // Don't auto-minimize since tray is disabled
        // if (_trayIcon != null)
        // {
        //     Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        //     {
        //         MinimizeToTray();
        //     }, Avalonia.Threading.DispatcherPriority.ApplicationIdle);
        // }
    }

    private void CreateTrayIcon()
    {
        try
        {
            if (_trayIcon == null)
            {
                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://LoupixDeck/Assets/logo.ico"))),
                    ToolTipText = "LoupixDeck",
                    IsVisible = true,
                    Menu = new NativeMenu()
                };

                var showMenuItem = new NativeMenuItem("Show") { Command = ShowCommand };
                var quitMenuItem = new NativeMenuItem("Quit") { Command = QuitCommand };

                _trayIcon.Menu?.Items.Add(showMenuItem);
                _trayIcon.Menu?.Items.Add(new NativeMenuItemSeparator());
                _trayIcon.Menu?.Items.Add(quitMenuItem);

                _trayIcon.Clicked += (sender, e) => ShowFromTray();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tray icon not available (this is normal on some systems): {ex.Message}");
            _trayIcon = null;
        }
    }

    private void OnWindowClosing(object sender, WindowClosingEventArgs e)
    {
        // Only minimize to tray if tray icon is available
        if (!_isMinimizedToTray && _trayIcon != null)
        {
            e.Cancel = true;
            MinimizeToTray();
        }
    }

    private void MinimizeToTray()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_isMinimizedToTray) return;

            _isMinimizedToTray = true;

            if (_trayIcon != null)
            {
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            CreateTrayIcon();

            Hide();
        });
    }

    private void ShowFromTray()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (!_isMinimizedToTray) return;

            _isMinimizedToTray = false;
            Show();
            Activate();

            if (_trayIcon != null)
            {
                _trayIcon.IsVisible = false;
            }
        });
    }

    private void QuitApplication()
    {
        _isMinimizedToTray = true; // Allow window to actually close
        _trayIcon?.Dispose();
        _trayIcon = null;
        
        // Clear device state
        ViewModel?.QuitApplication();
    }
}