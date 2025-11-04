using LoupixDeck.Commands.Base;
using LoupixDeck.Controllers;
using LoupixDeck.Services;
using LoupixDeck.Utils;

namespace LoupixDeck.Commands;

[Command("System.DeviceOff", "Device OFF (Apply OFF Config)", "Device Control")]
public class DeviceOffCommand(IDeviceController controller) : IExecutableCommand
{
    public async Task Execute(string[] parameters)
    {
        Console.WriteLine("Manual Device OFF command triggered");
        await controller.ClearDeviceState();
        Console.WriteLine("Device turned OFF (OFF config applied)");
    }
}

[Command("System.DeviceOn", "Device ON (Reload Config)", "Device Control")]
public class DeviceOnCommand(IDeviceController controller) : IExecutableCommand
{
    public async Task Execute(string[] parameters)
    {
        Console.WriteLine("Manual Device ON command triggered");
        
        // Restore device state from current loaded config
        await controller.RestoreDeviceState();
        
        Console.WriteLine("Device turned ON");
    }
}

[Command("System.ToggleWindow", "Toggle Window Show/Hide", "System")]
public class ToggleWindowCommand() : IExecutableCommand
{
    public Task Execute(string[] parameters)
    {
        var window = WindowHelper.GetMainWindow();
        if (window == null)
        {
            Console.WriteLine("Window not found");
            return Task.CompletedTask;
        }
        
        if (window.IsVisible)
        {
            Console.WriteLine("Hiding window");
            window.Hide();
        }
        else
        {
            Console.WriteLine("Showing window");
            window.Show();
            window.WindowState = Avalonia.Controls.WindowState.Normal;
            window.Activate();
        }
        
        return Task.CompletedTask;
    }
}

