using Avalonia.Media;
using LoupixDeck.Commands.Base;
using LoupixDeck.Controllers;
using LoupixDeck.Models.Extensions;
using SkiaSharp;

namespace LoupixDeck.Commands;

[Command("System.UpdateButton", "Update Touch Button Properties", "Button Control", 
    ParameterTemplate = "(index,text=...,textColor=...,backColor=...,image=...)")]
public class UpdateButtonCommand(IDeviceController controller) : IExecutableCommand
{
    public async Task Execute(string[] parameters)
    {
        if (parameters.Length < 2)
        {
            Console.WriteLine("Usage: System.UpdateButton(index,text=...,textColor=...,backColor=...,image=...)");
            Console.WriteLine("Example: System.UpdateButton(0,text=Hello,textColor=Red,backColor=Blue)");
            return;
        }

        // First parameter is the button index
        if (!int.TryParse(parameters[0], out int buttonIndex))
        {
            Console.WriteLine($"Invalid button index: {parameters[0]}");
            return;
        }

        // Convert to 0-based if needed (assuming user provides 0-based)
        if (buttonIndex < 0 || buttonIndex >= controller.Config.DeviceTouchButtonCount)
        {
            Console.WriteLine($"Button index {buttonIndex} out of range (0-{controller.Config.DeviceTouchButtonCount - 1})");
            return;
        }

        var button = controller.Config.CurrentTouchButtonPage.TouchButtons.FindByIndex(buttonIndex);
        if (button == null)
        {
            Console.WriteLine($"Button {buttonIndex} not found on current page");
            return;
        }

        bool updated = false;

        // Parse remaining parameters as key=value pairs
        for (int i = 1; i < parameters.Length; i++)
        {
            var param = parameters[i].Trim();
            var parts = param.Split('=', 2);
            
            if (parts.Length != 2)
            {
                Console.WriteLine($"Invalid parameter format: {param}. Expected key=value");
                continue;
            }

            var key = parts[0].Trim().ToLower();
            var value = parts[1].Trim();

            switch (key)
            {
                case "text":
                    button.Text = value;
                    updated = true;
                    Console.WriteLine($"Updated text: {value}");
                    break;

                case "textcolor":
                    if (TryParseColor(value, out var textColor))
                    {
                        button.TextColor = textColor;
                        updated = true;
                        Console.WriteLine($"Updated text color: {value}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid text color: {value}");
                    }
                    break;

                case "backcolor":
                case "backgroundcolor":
                    if (TryParseColor(value, out var backColor))
                    {
                        button.BackColor = backColor;
                        updated = true;
                        Console.WriteLine($"Updated background color: {value}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid background color: {value}");
                    }
                    break;

                case "image":
                    if (File.Exists(value))
                    {
                        try
                        {
                            button.Image = SKBitmap.Decode(value);
                            updated = true;
                            Console.WriteLine($"Updated image: {value}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading image: {ex.Message}");
                        }
                    }
                    else if (string.IsNullOrEmpty(value) || value.ToLower() == "null" || value.ToLower() == "clear")
                    {
                        button.Image = null;
                        updated = true;
                        Console.WriteLine("Cleared image");
                    }
                    else
                    {
                        Console.WriteLine($"Image file not found: {value}");
                    }
                    break;

                default:
                    Console.WriteLine($"Unknown property: {key}");
                    break;
            }
        }

        if (updated)
        {
            // Save the configuration
            controller.SaveConfig();
            Console.WriteLine($"Button {buttonIndex} updated and config saved");
        }
        else
        {
            Console.WriteLine("No properties were updated");
        }
        
        await Task.CompletedTask;
    }

    private bool TryParseColor(string colorString, out Color color)
    {
        color = Colors.Black;

        try
        {
            // Try parsing as hex color (#RRGGBB or #AARRGGBB)
            if (colorString.StartsWith("#"))
            {
                color = Color.Parse(colorString);
                return true;
            }

            // Try parsing as named color
            var property = typeof(Colors).GetProperty(colorString, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Static | 
                System.Reflection.BindingFlags.IgnoreCase);
            
            if (property != null)
            {
                color = (Color)property.GetValue(null);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}

