using System.Net;
using System.Net.Sockets;
using Avalonia;

namespace LoupixDeck;

sealed class Program
{
#if !WINDOWS
    private const string SocketPath = "/tmp/loupixdeck_app.sock";
    private static Socket _listenerSocket;
    public static IServiceProvider AppServices { get; set; }
#else
    private const string MutexName = "LoupixDeck_Mutex";
    private static bool _mutexOwned;
    private static Mutex _instanceMutex;
#endif

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Handle CLI commands if arguments provided
        if (args.Length > 0)
        {
            HandleCliCommand(args);
            return;
        }

#if !WINDOWS
        {
            if (File.Exists(SocketPath))
            {
                // App is already running - just notify
                try
                {
                    using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    client.Connect(new UnixDomainSocketEndPoint(SocketPath));
                    Console.WriteLine("Already running.");
                    return;
                }
                catch (SocketException)
                {
                    File.Delete(SocketPath);
                }
            }

            _listenerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            _listenerSocket.Bind(new UnixDomainSocketEndPoint(SocketPath));
            _listenerSocket.Listen(1);

            // Listen for toggle commands in background
            Task.Run(() => ListenForCommands());

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                _listenerSocket.Close();
                File.Delete(SocketPath);
            };
        }
#else
        _instanceMutex = new Mutex(true, MutexName, out _mutexOwned);

        if (!_mutexOwned)
        {
            Console.WriteLine("Already running.");
            return;
        }
#endif

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

#if !WINDOWS
    private static void HandleCliCommand(string[] args)
    {
        if (!File.Exists(SocketPath))
        {
            Console.WriteLine("LoupixDeck is not running. Start it first with: dotnet run");
            Environment.Exit(1);
            return;
        }

        try
        {
            using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            client.Connect(new UnixDomainSocketEndPoint(SocketPath));
            
            var command = args[0].ToLower();
            var message = System.Text.Encoding.UTF8.GetBytes(command);
            client.Send(message);
            
            var buffer = new byte[1024];
            var received = client.Receive(buffer);
            var response = System.Text.Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void ListenForCommands()
    {
        try
        {
            while (true)
            {
                var clientSocket = _listenerSocket.Accept();
                Task.Run(() => HandleClientCommand(clientSocket));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Command listener error: {ex.Message}");
        }
    }

    private static void HandleClientCommand(Socket client)
    {
        try
        {
            var buffer = new byte[1024];
            var bytesRead = client.Receive(buffer);
            var command = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim().ToLower();
            
            Console.WriteLine($"Received CLI command: {command}");
            
            string response;
            
            // Handle updateButton command with parameters
            if (command.StartsWith("updatebutton "))
            {
                var args = command.Substring(13).Trim(); // Remove "updatebutton "
                // Replace spaces with commas for parameter format: "6 text=hello" -> "6,text=hello"
                args = args.Replace(" ", ",");
                response = ExecuteDeviceCommand($"System.UpdateButton({args})");
            }
            // Handle page commands (page1, page2, rotarypage1, etc.)
            else if (command.StartsWith("page") && int.TryParse(command.Substring(4), out int touchPageNum))
            {
                response = ExecuteDeviceCommand($"System.GotoPage({touchPageNum})");
            }
            else if (command.StartsWith("rotarypage") && int.TryParse(command.Substring(10), out int rotaryPageNum))
            {
                response = ExecuteDeviceCommand($"System.GotoRotaryPage({rotaryPageNum})");
            }
            else
            {
                response = command switch
                {
                    "off" => ExecuteDeviceCommand("System.DeviceOff"),
                    "on" => ExecuteDeviceCommand("System.DeviceOn"),
                    "on-off" => ExecuteDeviceCommand("System.DeviceToggle"),
                    "wakeup" => ExecuteDeviceCommand("System.DeviceWakeup"),
                    "nextpage" => ExecuteDeviceCommand("System.NextPage"),
                    "previouspage" => ExecuteDeviceCommand("System.PreviousPage"),
                    "nextrotarypage" => ExecuteDeviceCommand("System.NextRotaryPage"),
                    "previousrotarypage" => ExecuteDeviceCommand("System.PreviousRotaryPage"),
                    "toggle" or "show" or "hide" => ExecuteDeviceCommand("System.ToggleWindow"),
                    "quit" => ExecuteQuit(),
                    _ => "Unknown command. Available: on, off, on-off, wakeup, page<N>, rotaryPage<N>, nextPage, previousPage, nextRotaryPage, previousRotaryPage, updateButton, toggle, show, hide, quit"
                };
            }
            
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
            client.Send(responseBytes);
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling command: {ex.Message}");
        }
    }

    private static string ExecuteDeviceCommand(string commandName)
    {
        try
        {
            var commandService = AppServices?.GetService(typeof(Services.ICommandService)) as Services.ICommandService;
            
            if (commandService != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    await commandService.ExecuteCommand(commandName);
                });
                return $"OK: {commandName} executed";
            }
            return "ERROR: Command service not available";
        }
        catch (Exception ex)
        {
            return $"ERROR: {ex.Message}";
        }
    }

    private static string ExecuteQuit()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var mainWindow = Utils.WindowHelper.GetMainWindow();
            if (mainWindow?.DataContext is ViewModels.MainWindowViewModel vm)
            {
                vm.QuitApplication();
            }
        });
        return "OK: Quitting...";
    }
#endif

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}