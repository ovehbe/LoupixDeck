# LoupixDeck

[![.NET Release](https://github.com/RadiatorTwo/LoupixDeck/actions/workflows/release.yml/badge.svg)](https://github.com/RadiatorTwo/LoupixDeck/actions/workflows/release.yml)
[![Platform](https://img.shields.io/badge/platform-linux-blue)](https://github.com/RadiatorTwo/LoupixDeck)
[![Platform](https://img.shields.io/badge/platform-windows-blue)](https://github.com/RadiatorTwo/LoupixDeck)

# ⚠️ THIS FORK IS ONLY TESTED WITH RAZER STREAM CONTROLLER (LOUPEDECK LIVE)

**LoupixDeck** is a Linux and Windows application for controlling the **Loupedeck Live S** and **Razer Stream Controller**.  
It provides a highly customizable interface to assign commands, control external tools, and build dynamic layouts using touchscreen and rotary inputs.

**NEW**: Full support for Razer Stream Controller with 6 rotary encoders, 8 LED buttons, 4×3 touch grid, and narrow side displays!

---

## ⚠️ Disclaimer

 ⚠️ **This software is in an early, experimental stage.**

 ⚠️ **Many features are under active development or not fully implemented.**

---

## ✨ Features

- **Custom touchscreen button rendering**:
  - Images, background colors, text
  - **Per-page wallpapers**: Set different wallpapers for each page with independent opacity controls
  - **Wallpaper management**: Easy select and remove buttons in settings
  - Rotation support for narrow side displays (0°, 90°, 180°, 270°)
- **Full input support**:
  - Touch input, rotary encoder turns/clicks, and physical button presses
  - **Per-button vibration control**: Enable/disable and choose from 28 haptic patterns for each touch button
  - **Smart vibration defaults**: Center buttons use minimal vibration, narrow buttons use short haptic feedback
  - **Visual touch feedback** with configurable color and opacity
  - **Touch sliding prevention**: No accidental multi-triggers when finger slides across buttons
- **Dynamic page system**:
  - Independent pages for touch and rotary buttons
  - **Silent page switching**: No visual page indicator (clean switching)
- **Flexible command actions**:
  - Execute shell commands on button press
  - Send keyboard macros
  - **Per-Page Global Commands**: Add prefix/suffix commands to all buttons on a specific page
    - Select which page to configure via dropdown
    - Touch buttons: Global pre/post actions for all touch buttons on current page
    - Simple buttons: Global pre/post actions for physical LED buttons on current page
    - Knobs: Separate prefix/suffix for left rotation, right rotation, and press on current page
    - Perfect for page-specific logging, mode switching, or state management
- **Device configuration**:
  - Adjust display brightness
  - Set individual RGB colors for each physical button
  - Configurable touch feedback (enable/disable, color, opacity)
  - **Enable When OFF**: Keep specific physical buttons/knobs functional even when device is OFF
- **Persistent configuration**:
  - Save and restore full device layout and settings
  - Template-based initial configuration
  - Device-specific config files (no conflicts between devices)
- **Command selection menu**:
  - Assign commands directly with parameters
- **External tool integration**:
  - Control **OBS Studio** via WebSocket (e.g., toggle virtual camera)
  - Manage **Elgato Key Lights** (power, brightness, color temperature)
- **System interaction**:
  - Show system notifications via **D-Bus**
- **Power management**:
  - Device clears on app exit (brightness=0, LEDs off)
  - Device clears on system suspend and restores on resume
  - Clean startup sequence (clear → apply config)

---

## 🖥️ Supported Devices

### ✅ Loupedeck Live S (VID: 2ec2, PID: 0006)
- **Display**: Single 480×270 touchscreen
- **Touch Grid**: 5×3 = 15 buttons (90×90 each)
- **Rotary Encoders**: 2 knobs (left side)
- **Physical Buttons**: 4 RGB LED buttons
- **Config File**: `config.json`

### ✅ Razer Stream Controller (VID: 1532, PID: 0d06) **NEW!**
- **Display**: Unified 480×270 touchscreen with three zones:
  - Left narrow: 60×270 (1 tall button with rotation support)
  - Center grid: 360×270 (4×3 = 12 buttons, 90×90 each)
  - Right narrow: 60×270 (1 tall button with rotation support)
- **Total Touch Buttons**: 14 (12 center + 2 narrow)
- **Rotary Encoders**: 6 knobs (3 left + 3 right)
- **Physical Buttons**: 8 RGB LED buttons
- **Config File**: `config_razer.json`
- **Special Features**:
  - Rotation control for narrow displays (perfect for vertical text/images)
  - Template-based initial configuration
  - Automatic device clear on exit/suspend

> The Razer Stream Controller uses the same protocol as Loupedeck Live.
> Support for additional Loupedeck models may be considered in the future.

## 📸 Screenshots

![Screenshot_20-Mai_22-30-55_LoupixDeck](https://github.com/user-attachments/assets/1a16ae9c-e765-435d-9a43-26ea521e78bd)

![Screenshot_20-Mai_22-31-52_LoupixDeck](https://github.com/user-attachments/assets/dea8d42d-fc2f-4132-b80e-d4ddf3a463dc)

<img width="814" height="548" alt="Screenshot from 2025-11-04 09 51 30" src="https://github.com/user-attachments/assets/05144e1f-2c1c-4f09-af24-630c3a1cfbb6" />

## 🛠️ Installation

### Prerequisites
- **.NET 9.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))

### Quick Install (Linux) - Recommended

The easiest way to install is using the automated build script:

```bash
# Clone or download the repository
git clone https://github.com/ovehbe/LoupixDeck.git
cd LoupixDeck

# Run the automated build script
./build_release.sh
```

This script will:
- Build the application
- Create necessary directories
- Add symbolic links
- Create a desktop launcher automatically
- Make everything ready to use!

**After installation, run:** `loupixdeck` from terminal or launch from your application menu.

### Default Button Configuration
After you launch the app for the first time:
- **Button 0** (first physical button): App window toggle (show/hide)
- **Button 7** (last physical button): Device ON/OFF toggle

These defaults help you get started quickly!

See [INSTALL.md](INSTALL.md) for detailed manual installation instructions, additional setup options, and keyboard shortcut configuration.

### Windows Build

```bash
git clone https://github.com/ovehbe/LoupixDeck.git
cd LoupixDeck
dotnet publish LoupixDeck.csproj -c Release -r win-x64 --self-contained true `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=false `
    /p:EnableCompressionInSingleFile=true `
    /p:ReadyToRun=true `
    -o publish/win-x64
```

### Development Mode

```bash
git clone https://github.com/ovehbe/LoupixDeck.git
cd LoupixDeck
dotnet run
```

---

## 🎮 Razer Stream Controller Setup

### Device Detection
The app automatically detects your Razer Stream Controller on first run. If you encounter connection issues:

1. **Check USB connection**: `lsusb | grep 1532:0d06`
2. **Add udev rule** (Linux):
   ```bash
   sudo nano /etc/udev/rules.d/99-razer-stream-controller.rules
   ```
   Add:
   ```
   SUBSYSTEM=="usb", ATTRS{idVendor}=="1532", ATTRS{idProduct}=="0d06", MODE="0666"
   ```
   Then reload:
   ```bash
   sudo udevadm control --reload-rules
   sudo udevadm trigger
   ```

### Button Layout
```
[Knob1] [Left-Display] [Grid 4×3] [Right-Display] [Knob4]
[Knob2]                            [Knob5]
[Knob3]                            [Knob6]

           [Button0-7 with RGB LEDs]
```

### Configuration Files
- **Main config**: `~/.config/LoupixDeck/config_razer.json` - Your settings
- **OFF config**: `~/.config/LoupixDeck/config_razer_off.json` - Device clear state
- Both are created automatically from templates on first run

### Narrow Display Tips
The left and right narrow displays are perfect for:
- Volume indicators (use Rotation: 90° for vertical text)
- Status displays
- Labels for the adjacent knobs

Configure them like any other button, but use the **Rotation** slider in settings!

---

## 💻 CLI Commands

Control your device from the terminal while the app is running!

### Start the App First:
```bash
dotnet run
# Or published version:
~/Applications/LoupixDeck/LoupixDeck
```

### Available Commands:

**Device Control:**
```bash
./loupixdeck-cli off      # Turn device OFF (brightness=0, LEDs black, screen clears)
./loupixdeck-cli on       # Turn device ON (restore brightness, LEDs, graphics)
./loupixdeck-cli on-off   # Toggle device ON/OFF
./loupixdeck-cli wakeup   # Reconnect device and turn ON (for suspend/resume)
```

**Button Control:**
```bash
# Update button display properties
./loupixdeck-cli updateButton 0 text=Hello textColor=Red backColor=Blue
./loupixdeck-cli updateButton 1 image=/path/to/icon.png
```

**Window Control:**
```bash
./loupixdeck-cli toggle   # Toggle window show/hide
./loupixdeck-cli show     # Show window
./loupixdeck-cli hide     # Hide window
```

**App Control:**
```bash
./loupixdeck-cli quit     # Exit app (applies OFF config first)
```

### X Button Behavior:
- **X button** on window: **Hides** window (app keeps running)
- **To actually quit**: Use hamburger menu → Quit

### In-App Commands (Assign to Buttons):
You can also assign these commands to physical buttons or touch buttons:

**Device Control:**
- `System.DeviceOff` - Turn device OFF
- `System.DeviceOn` - Turn device ON
- `System.ToggleWindow` - Show/hide GUI window

**Page Navigation:**
- `System.NextPage` - Next touch page
- `System.PreviousPage` - Previous touch page
- `System.NextRotaryPage` - Next rotary page
- `System.PreviousRotaryPage` - Previous rotary page

**Button Control:**
- `System.UpdateButton(index,text=...,textColor=...,backColor=...,image=...)` - Update button properties

**Note:** For direct page navigation (page 1, 2, 3, etc.), use the CLI commands instead

### Example: Create Keyboard Shortcuts

**Elementary OS / Pantheon:**
1. System Settings → Keyboard → Shortcuts → Custom
2. Add shortcuts:
   - **Super+D**: `~/Applications/LoupixDeck/LoupixDeck off`
   - **Super+Shift+D**: `~/Applications/LoupixDeck/LoupixDeck on`
   - **Super+L**: `~/Applications/LoupixDeck/LoupixDeck toggle`

**Any Linux DE:**
Bind to your preferred keys - perfect for quickly turning off your device when locking screen or going AFK!

### Socket Communication:
Commands communicate via Unix socket: `/tmp/loupixdeck_app.sock`

**Note**: Tray icon disabled due to compatibility issues with some desktop environments.

---

## 🆕 What's New in This Fork

This fork adds comprehensive support for the **Razer Stream Controller**:

### New Device Support
- Complete Razer Stream Controller implementation
- 6 rotary encoders (vs 2 on Live S)
- 8 physical buttons (vs 4 on Live S)
- 4×3 touch grid + 2 narrow side displays
- Unified 480×270 display rendering

### New Features
- **Visual touch feedback**: Configurable flash animation on button press (Settings → General)
- **Per-button vibration control**: 
  - Enable/disable vibration for each touch button individually
  - Choose from 28 different haptic patterns per button
  - Smart defaults: Center buttons use minimal vibration (AscendFast), narrow buttons use short feedback (ShortLower)
  - Settings accessible in touch button properties dialog
- **Touch sliding prevention**: Prevents accidental command triggers when sliding finger across buttons
- **Enable When OFF**: Keep specific physical buttons and knobs functional when device is in OFF mode
  - Perfect for "Turn ON" buttons or window toggle controls
  - Configurable per button in button settings
- **Per-Page Global Commands** (Settings → Global Commands):
  - Configure different global commands for each page via dropdown selector
  - Add prefix/suffix commands to all buttons on the selected page
  - **Touch Buttons**: Wrap all touch button commands with pre/post actions
  - **Simple Buttons**: Global pre/post for physical LED buttons
  - **Knobs**: Separate prefix/suffix for rotate left, rotate right, and button press
  - Each page can have completely different global command behavior
  - Example uses: Page-specific logging, different notification styles, mode-dependent actions
- **Per-Page Wallpapers**:
  - Set unique wallpapers for each page
  - Independent opacity control per page
  - Easy wallpaper selection and removal
  - Configure via Settings → Wallpaper with page dropdown
- **Clean Page Switching**: No visual page indicator on display (silent page changes)
- **Rotation control**: Rotate content on narrow displays (0°/90°/180°/270°)
- **Template-based config**: Clean initial setup with default layouts
- **Power management**: Auto-clear device on exit
- **Smart config loading**: Preserves settings between runs (no overwrites)
- **Device-specific configs**: No conflicts when using multiple devices
- **CLI Commands**: Control device from terminal (`on`, `off`, `toggle`, etc.)
- **Window management**: X button hides window, Quit from menu exits
- **Device state management**: OFF/ON configs for clean power cycling

### Technical Improvements
- Dynamic UI layout (adapts to device type)
- Device registry system for easy device addition
- Unified display rendering (correct X-coordinate positioning)
- Enhanced error handling (graceful fallbacks)
- Suspend/resume detection with automatic state management

### New Files
- `Controllers/RazerStreamControllerController.cs` - Razer device controller
- `LoupedeckDevice/Device/RazerStreamControllerDevice.cs` - Device implementation
- `Services/SystemPowerService.cs` - Power event monitoring
- `Templates/RazerDefaultConfig.json` - Initial configuration template
- `Templates/RazerOffConfig.json` - Device clear state template

---

## 🎯 Usage Examples

### Per-Button Vibration Control

**⚠️ Important**: To use LoupixDeck's vibration control, you must first disable the device's built-in haptic feedback:
1. Open the official **Loupedeck software** on a Windows computer
2. Go to **Device Settings** → **Device Haptics**
3. **Disable or turn off** the automatic haptic feedback
4. This prevents conflicts between firmware vibration and LoupixDeck's per-button control

**Once disabled in Loupedeck software:**
1. Right-click any touch button in the LoupixDeck GUI
2. Scroll down to "Vibration Settings"
3. Check "Enable Vibration" to turn on haptic feedback
4. Choose from 28 patterns: Short, Medium, Long, Buzz, Rumble, etc.
5. **Tip**: Use `ShortLower` for a quick, subtle feedback

### Enable When OFF Feature
Perfect for essential controls that should work even when device is "OFF":
1. Right-click a physical button or knob
2. Check "Enable When Device Is OFF"
3. Now this button works even in OFF mode
4. **Use case**: Assign `System.DeviceOn` to a button so you can always turn device back on

### Per-Page Global Commands
Add logging, notifications, or state management to all buttons on a specific page:

**Example 1 - Page-specific logging:**
1. Settings → Global Commands
2. Select "Page 0" from dropdown
3. Touch Buttons → Enable Suffix: ✓
4. Suffix command: `echo "Page 0 button: $(date)" >> /tmp/page0_log.txt`
5. Select "Page 1" from dropdown
6. Configure different suffix: `echo "Page 1 button: $(date)" >> /tmp/page1_log.txt`
7. Now each page logs to its own file!

**Example 2 - Page-specific notifications:**
```bash
Page 0 - Gaming Controls:
  Prefix: notify-send "Game Control"
  
Page 1 - Music Controls:
  Prefix: notify-send "Music Action"
```

**Example 3 - Volume knob with feedback:**
```bash
Knob Left Suffix: notify-send "Volume Down"
Knob Right Suffix: notify-send "Volume Up"
Knob Press Prefix: notify-send "Mute Toggle"
```

### Per-Page Wallpapers
Set different backgrounds for different pages:

**Example - Professional vs Personal Pages:**
1. Settings → Wallpaper
2. Select "Page 0" - Your work page
3. Click "Select..." and choose a professional background
4. Adjust opacity for readability
5. Select "Page 1" - Your gaming page
6. Click "Select..." and choose a gaming-themed background
7. Each page now has its own unique look!

**Remove Wallpaper:**
- Simply click the "Remove" button to clear a page's wallpaper

### Vibration Patterns Guide
- **Minimal**: AscendFast, ShortLower, Lowest
- **Short**: Short, ShortLow, ShortLower  
- **Standard**: Medium, Low, Lower
- **Long**: Long, VeryLong
- **Special**: Buzz, Rumble1-5, RiseFall, DescendSlow/Med/Fast

---

## 🙏 Credits

- **Original LoupixDeck**: [RadiatorTwo/LoupixDeck](https://github.com/RadiatorTwo/LoupixDeck)
- **Razer Stream Controller Support**: Added by [@ovehbe](https://github.com/ovehbe)
- **Protocol Reference**: [foxxyz/loupedeck](https://github.com/foxxyz/loupedeck) (Node.js implementation)
- **Additional Reference**: [flowernert/loupedeckapp](https://github.com/flowernert/loupedeckapp) (Python implementation)

---

## 📝 License

MIT License - See [LICENSE](LICENSE) file for details.
