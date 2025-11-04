using SkiaSharp;

namespace LoupixDeck.LoupedeckDevice.Device;

/// <summary>
/// Razer Stream Controller device implementation.
/// This device is essentially a rebranded Loupedeck Live with a different layout:
/// - 3 knobs on the left side
/// - 3 knobs on the right side  
/// - 4x3 touch grid in the center (12 buttons)
/// - 8 physical buttons below the screen
/// </summary>
public class RazerStreamControllerDevice : LoupedeckDevice
{
    public RazerStreamControllerDevice(string host = null, string path = null, int baudrate = 0, bool autoConnect = true, int reconnectInterval = Constants.DefaultReconnectInterval)
        : base(host, path, baudrate, autoConnect, reconnectInterval)
    {
        // 8 physical buttons below the screen (0-7)
        Buttons = [0, 1, 2, 3, 4, 5, 6, 7];
        
        // 4 columns x 3 rows = 12 touch buttons
        Columns = 4;
        Rows = 3;
        
        // Center screen visible area: 60px offset from left (left screen), 420px end (before right screen)
        // Total display is 480x270, divided into: left(60) + center(360) + right(60)
        VisibleX = [60, 420];
        VisibleY = [0, 270];
        
        Type = "Razer Stream Controller";
        ProductId = "0d06";
        
        // Three separate display areas:
        // - left: 60x270 (behind left 3 knobs)
        // - center: 360x270 (main touch area for 4x3 grid)
        // - right: 60x270 (behind right 3 knobs)
        Displays = new Dictionary<string, DisplayInfo>
        {
            ["left"] = new() { Id = "\0L"u8.ToArray(), Width = 60, Height = 270 },
            ["center"] = new() { Id = "\0M"u8.ToArray(), Width = 360, Height = 270 },
            ["right"] = new() { Id = "\0R"u8.ToArray(), Width = 60, Height = 270 }
        };
    }

    protected override TouchTarget GetTarget(int x, int y)
    {
        if (VisibleX == null || VisibleY == null)
        {
            throw new InvalidOperationException("VisibleX or VisibleY cannot be null.");
        }

        // Determine which screen was touched
        string screen;
        int key;

        if (x < 60)
        {
            // Left narrow display (60x270): Single tall button
            screen = "left";
            key = 12; // Index 12
            return new TouchTarget { Screen = screen, Key = key };
        }
        else if (x >= 420)
        {
            // Right narrow display (60x270): Single tall button
            screen = "right";
            key = 13; // Index 13
            return new TouchTarget { Screen = screen, Key = key };
        }
        else
        {
            // Center display (360x270): 4x3 grid
            screen = "center";
            
            // Clamp to visible area
            x = Math.Max(x, VisibleX[0]);
            x = Math.Min(x, VisibleX[1]);
            y = Math.Max(y, VisibleY[0]);
            y = Math.Min(y, VisibleY[1]);
            
            // Adjust x to be relative to the center screen (remove left screen offset)
            x -= VisibleX[0];
            
            // Calculate which button was pressed (4x3 grid, 90x90 buttons)
            var column = x / 90;
            var row = y / 90;
            key = row * Columns + column; // Indices 0-11
            
            return new TouchTarget { Screen = "center", Key = key };
        }
    }

    /// <summary>
    /// Override to handle rendering for side displays (indices 12-13)
    /// Indices 0-11: Center grid (4×3)
    /// Index 12: Left display (60×270 full height)
    /// Index 13: Right display (60×270 full height)
    /// </summary>
    public async Task DrawSideDisplayButton(int index, SKBitmap bitmap)
    {
        if (index < 12 || index > 13)
            throw new Exception($"Index {index} is not a side display button (valid: 12-13)");

        try
        {
            // The Razer has ONE 480×270 display, not three separate ones
            // Left display: X=0, Width=60
            // Center: X=60, Width=360
            // Right display: X=420, Width=60
            
            if (index == 12)
            {
                // Left narrow display: Draw at X=0 on the unified display
                await DrawCanvas("center", 60, 270, bitmap, 0, 0);
            }
            else if (index == 13)
            {
                // Right narrow display: Draw at X=420 on the unified display
                await DrawCanvas("center", 60, 270, bitmap, 420, 0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR drawing side display {index}: {ex.Message}");
        }
    }
}

