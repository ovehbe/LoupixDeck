# LoupixDeck

[![.NET Release](https://github.com/RadiatorTwo/LoupixDeck/actions/workflows/release.yml/badge.svg)](https://github.com/RadiatorTwo/LoupixDeck/actions/workflows/release.yml)
[![Platform](https://img.shields.io/badge/platform-linux-blue)](https://github.com/RadiatorTwo/LoupixDeck)
[![Platform](https://img.shields.io/badge/platform-windows-blue)](https://github.com/RadiatorTwo/LoupixDeck)

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
  - Tiled wallpaper rendered across all touchscreen buttons
  - Rotation support for narrow side displays (0°, 90°, 180°, 270°)
- **Full input support**:
  - Touch input, rotary encoder turns/clicks, and physical button presses
  - Haptic feedback (vibration) when touching buttons
  - **Visual touch feedback** with configurable color and opacity
- **Dynamic page system**:
  - Independent pages for touch and rotary buttons
  - Temporary on-screen display of the active page index
- **Flexible command actions**:
  - Execute shell commands on button press
  - Send keyboard macros
- **Device configuration**:
  - Adjust display brightness
  - Set individual RGB colors for each physical button
  - Configurable touch feedback (enable/disable, color, opacity)
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


## 🛠️ Build Instructions

### Linux

```bash
git clone https://github.com/ovehbe/LoupixDeck.git
cd LoupixDeck
dotnet publish LoupixDeck.csproj -c Release -r linux-x64 --self-contained true \
            /p:PublishSingleFile=true \
            /p:PublishTrimmed=false \
            /p:EnableCompressionInSingleFile=true \
            /p:ReadyToRun=true \
            -o publish/linux-x64
```

### Windows

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

### Quick Development Run

```bash
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

## 🆕 What's New in This Fork

This fork adds comprehensive support for the **Razer Stream Controller**:

### New Device Support
- Complete Razer Stream Controller implementation
- 6 rotary encoders (vs 2 on Live S)
- 8 physical buttons (vs 4 on Live S)
- 4×3 touch grid + 2 narrow side displays
- Unified 480×270 display rendering

### New Features
- **Visual touch feedback**: Configurable flash animation on button press
- **Rotation control**: Rotate content on narrow displays (0°/90°/180°/270°)
- **Template-based config**: Clean initial setup with default layouts
- **Power management**: Auto-clear device on exit/suspend
- **Smart config loading**: Preserves settings between runs
- **Device-specific configs**: No conflicts when using multiple devices

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

## 🙏 Credits

- **Original LoupixDeck**: [RadiatorTwo/LoupixDeck](https://github.com/RadiatorTwo/LoupixDeck)
- **Razer Stream Controller Support**: Added by [@ovehbe](https://github.com/ovehbe)
- **Protocol Reference**: [foxxyz/loupedeck](https://github.com/foxxyz/loupedeck) (Node.js implementation)
- **Additional Reference**: [flowernert/loupedeckapp](https://github.com/flowernert/loupedeckapp) (Python implementation)

---

## 📝 License

MIT License - See [LICENSE](LICENSE) file for details.
