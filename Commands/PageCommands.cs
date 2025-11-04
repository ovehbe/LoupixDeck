using LoupixDeck.Commands.Base;
using LoupixDeck.Controllers;

namespace LoupixDeck.Commands;

[Command("System.NextPage","Next Touch Page", "Pages")]
public class PreviousTouchPageCommand(LoupedeckLiveSController loupedeck) : IExecutableCommand
{
    public Task Execute(string[] parameters)
    {
        if (parameters.Length != 0)
        {
            Console.WriteLine("Invalid Parameter count");
            return Task.CompletedTask;
        }

        loupedeck.PageManager.NextTouchPage();
        return Task.CompletedTask;
    }
}

[Command("System.PreviousPage","Previous Touch Page", "Pages")]
public class NextTouchPageCommand(LoupedeckLiveSController loupedeck) : IExecutableCommand
{
    public Task Execute(string[] parameters)
    {
        if (parameters.Length != 0)
        {
            Console.WriteLine("Invalid Parameter count");
            return Task.CompletedTask;
        }

        loupedeck.PageManager.PreviousTouchPage();
        return Task.CompletedTask;
    }
}

[Command("System.NextRotaryPage","Next Rotary Page", "Pages")]
public class NextRotaryPageCommand(LoupedeckLiveSController loupedeck) : IExecutableCommand
{
    public Task Execute(string[] parameters)
    {
        if (parameters.Length != 0)
        {
            Console.WriteLine("Invalid Parameter count");
            return Task.CompletedTask;
        }

        loupedeck.PageManager.NextRotaryPage();
        return Task.CompletedTask;
    }
}

[Command("System.PreviousRotaryPage","Previous Rotary Page", "Pages")]
public class PreviousRotaryPageCommand(LoupedeckLiveSController loupedeck) : IExecutableCommand
{
    public Task Execute(string[] parameters)
    {
        if (parameters.Length != 0)
        {
            Console.WriteLine("Invalid Parameter count");
            return Task.CompletedTask;
        }

        loupedeck.PageManager.PreviousRotaryPage();
        return Task.CompletedTask;
    }
}

[Command("System.GotoPage", "Go to Touch Page by Index", "Pages", ParameterTemplate = "(pageIndex)")]
public class GotoPageCommand(IDeviceController controller) : IExecutableCommand
{
    public async Task Execute(string[] parameters)
    {
        if (parameters.Length != 1)
        {
            Console.WriteLine("Invalid parameter count. Usage: System.GotoPage(pageIndex)");
            return;
        }

        if (!int.TryParse(parameters[0], out int pageIndex))
        {
            Console.WriteLine($"Invalid page index: {parameters[0]}");
            return;
        }

        // Convert to 0-based index (user provides 1-based)
        var targetIndex = pageIndex - 1;
        
        if (targetIndex < 0 || targetIndex >= controller.PageManager.TouchButtonPages.Count)
        {
            Console.WriteLine($"Page index {pageIndex} out of range (1-{controller.PageManager.TouchButtonPages.Count})");
            return;
        }

        await controller.PageManager.ApplyTouchPage(targetIndex);
        Console.WriteLine($"Switched to touch page {pageIndex}");
    }
}

[Command("System.GotoRotaryPage", "Go to Rotary Page by Index", "Pages", ParameterTemplate = "(pageIndex)")]
public class GotoRotaryPageCommand(IDeviceController controller) : IExecutableCommand
{
    public Task Execute(string[] parameters)
    {
        if (parameters.Length != 1)
        {
            Console.WriteLine("Invalid parameter count. Usage: System.GotoRotaryPage(pageIndex)");
            return Task.CompletedTask;
        }

        if (!int.TryParse(parameters[0], out int pageIndex))
        {
            Console.WriteLine($"Invalid page index: {parameters[0]}");
            return Task.CompletedTask;
        }

        // Convert to 0-based index (user provides 1-based)
        var targetIndex = pageIndex - 1;
        
        if (targetIndex < 0 || targetIndex >= controller.PageManager.RotaryButtonPages.Count)
        {
            Console.WriteLine($"Page index {pageIndex} out of range (1-{controller.PageManager.RotaryButtonPages.Count})");
            return Task.CompletedTask;
        }

        controller.PageManager.ApplyRotaryPage(targetIndex);
        Console.WriteLine($"Switched to rotary page {pageIndex}");
        return Task.CompletedTask;
    }
}