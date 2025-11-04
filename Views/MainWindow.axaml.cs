using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform;
using LoupixDeck.Utils;
using LoupixDeck.ViewModels;

namespace LoupixDeck.Views;

public partial class MainWindow : Window
{
    private static MainWindow Instance { get; set; }
    
    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public static void ToggleVisibility()
    {
        if (Instance == null) return;
        
        if (Instance.IsVisible)
        {
            Console.WriteLine("Toggle: Hiding window");
            Instance.Hide();
        }
        else
        {
            Console.WriteLine("Toggle: Showing window");
            Instance.Show();
            Instance.WindowState = WindowState.Normal;
            Instance.Activate();
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        
        Instance = this;

        this.Closing += OnWindowClosing;
        
        Console.WriteLine("LoupixDeck started. Run the app again to toggle window visibility.");
        Console.WriteLine($"Toggle command: {AppDomain.CurrentDomain.BaseDirectory}LoupixDeck");
    }

    private void OnWindowClosing(object sender, WindowClosingEventArgs e)
    {
        // X button quits the app normally
        ViewModel?.QuitApplication();
    }
}
