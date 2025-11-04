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

[Command("System.DeviceToggle", "Device Toggle ON/OFF", "Device Control")]
public class DeviceToggleCommand(IDeviceController controller) : IExecutableCommand
{
    public async Task Execute(string[] parameters)
    {
        Console.WriteLine("Device Toggle command triggered");
        
        if (controller.IsDeviceOff())
        {
            Console.WriteLine("Device is OFF, turning ON...");
            await controller.RestoreDeviceState();
            Console.WriteLine("Device turned ON");
        }
        else
        {
            Console.WriteLine("Device is ON, turning OFF...");
            await controller.ClearDeviceState();
            Console.WriteLine("Device turned OFF");
        }
    }
}

[Command("System.DeviceWakeup", "Device Wakeup (Reconnect & ON)", "Device Control")]
public class DeviceWakeupCommand(IDeviceController controller) : IExecutableCommand
{
    public async Task Execute(string[] parameters)
    {
        Console.WriteLine("Device Wakeup command triggered");
        
        try
        {
            // Reconnect the device
            await controller.Reconnect();
            
            // Wait a moment for connection to stabilize
            await Task.Delay(500);
            
            // Turn device ON
            await controller.RestoreDeviceState();
            
            Console.WriteLine("Device wakeup complete - reconnected and turned ON");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Device wakeup failed: {ex.Message}");
            throw;
        }
    }
}

