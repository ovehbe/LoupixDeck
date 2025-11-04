using System.Net;
using System.Net.Sockets;
using Avalonia;

namespace LoupixDeck;

sealed class Program
{
#if !WINDOWS
    private const string SocketPath = "/tmp/loupixdeck_app.sock";
    private static Socket _listenerSocket;
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
#if !WINDOWS
        {
            if (File.Exists(SocketPath))
            {
                // App is already running - send toggle command
                try
                {
                    using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    client.Connect(new UnixDomainSocketEndPoint(SocketPath));
                    
                    // Send toggle command
                    var message = System.Text.Encoding.UTF8.GetBytes("TOGGLE");
                    client.Send(message);
                    
                    Console.WriteLine("Toggle command sent to running instance.");
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
    private static void ListenForCommands()
    {
        try
        {
            while (true)
            {
                var clientSocket = _listenerSocket.Accept();
                var buffer = new byte[1024];
                var bytesRead = clientSocket.Receive(buffer);
                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                if (message == "TOGGLE")
                {
                    Console.WriteLine("Received TOGGLE command");
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        Views.MainWindow.ToggleVisibility();
                    });
                }
                
                clientSocket.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Command listener error: {ex.Message}");
        }
    }
#endif

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}