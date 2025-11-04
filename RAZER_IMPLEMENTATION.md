# Razer Stream Controller Implementation

## Overview

This document describes the implementation of Razer Stream Controller support for LoupixDeck.

## Device Specifications

### Razer Stream Controller
- **VID:PID**: `1532:0d06`
- **Display**: 480×270 pixels (divided into 3 sections)
  - Left screen: 60×270 (behind left 3 knobs)
  - Center screen: 360×270 (touch area)
  - Right screen: 60×270 (behind right 3 knobs)
- **Touch Grid**: 4×3 = 12 buttons (90×90 pixels each)
- **Rotary Encoders**: 6 knobs total
  - Left side: KNOB_TL, KNOB_CL, KNOB_BL (Top/Center/Bottom Left)
  - Right side: KNOB_TR, KNOB_CR, KNOB_BR (Top/Center/Bottom Right)
- **Physical Buttons**: 8 buttons below the screen (BUTTON0-7)
  - BUTTON0 is typically the "home" button

### Comparison with Loupedeck Live S
- **Live S**: Single 480×270 screen, 5×3 grid (15 buttons), 2 knobs, 4 buttons
- **Razer**: Three-section 480×270 display, 4×3 grid (12 buttons), 6 knobs, 8 buttons

## Implementation Details

### 1. Device Class
**File**: `LoupedeckDevice/Device/RazerStreamControllerDevice.cs`

Defines the device-specific configuration:
- Three display areas (left, center, right)
- 4×3 touch button grid layout
- Touch target detection for all three screen sections
- Button and knob mapping

Key differences from Live S:
- `Columns = 4` (instead of 5)
- `Rows = 3` (same)
- `VisibleX = [60, 420]` (center screen offset)
- Three display definitions instead of one

### 2. Controller
**File**: `Controllers/RazerStreamControllerController.cs`

Handles all device interactions:
- Initializes all 8 physical buttons with LED colors
- Manages all 6 rotary encoders (with press and rotate actions)
- Handles 12 touch buttons in 4×3 grid
- Separate config file: `config_razer.json`

Button initialization:
- BUTTON0: Blue (Previous Page)
- BUTTON1-2: Green (Customizable)
- BUTTON3: Blue (Next Page)
- BUTTON4-7: Red (Customizable)

### 3. Device Registry
**File**: `Registry/DeviceRegistry.cs`

Added entry:
```csharp
new("Razer Stream Controller", "1532", "0d06", typeof(RazerStreamControllerDevice))
```

### 4. Common Interface
**File**: `Controllers/IDeviceController.cs`

Created a common interface that both controllers implement:
- `IPageManager PageManager`
- `LoupedeckConfig Config`
- `Task Initialize(string port, int baudrate)`
- `void SaveConfig()`

### 5. Device Service Updates
**File**: `Services/DeviceService.cs`

Enhanced to support multiple device types:
- `IDeviceService.Device` now returns base `LoupedeckDevice` type
- `StartDevice()` accepts VID/PID parameters
- Dynamically creates appropriate device type based on VID/PID
- Falls back to Loupedeck Live S for backward compatibility

### 6. Configuration Model
**File**: `Models/LoupedeckConfig.cs`

Added properties:
- `string DeviceVid` - Stores device vendor ID
- `string DevicePid` - Stores device product ID

These are saved with the configuration for automatic device detection on startup.

### 7. Initialization Flow
**File**: `App.axaml.cs`

Updated initialization chain:
1. Device selection dialog passes VID/PID
2. VID/PID flows through initialization methods
3. Controller selection based on device type
4. Device service creates correct device instance
5. Controller initializes with device-specific settings

### 8. View Models
**File**: `ViewModels/InitSetupViewModel.cs` & `ViewModels/MainWindowViewModel.cs`

- `DeviceItem` now includes VID/PID information
- `MainWindowViewModel` uses `IDeviceController` interface instead of concrete type
- Device detection filters to only show supported devices

## File Structure

```
LoupixDeck/
├── Controllers/
│   ├── IDeviceController.cs (NEW)
│   ├── LoupedeckLiveSController.cs (MODIFIED - implements IDeviceController)
│   └── RazerStreamControllerController.cs (NEW)
├── LoupedeckDevice/
│   └── Device/
│       ├── LoupedeckDevice.cs (unchanged)
│       ├── LoupedeckLiveSDevice.cs (unchanged)
│       └── RazerStreamControllerDevice.cs (NEW)
├── Models/
│   └── LoupedeckConfig.cs (MODIFIED - added DeviceVid/DevicePid)
├── Registry/
│   └── DeviceRegistry.cs (MODIFIED - added Razer entry)
├── Services/
│   └── DeviceService.cs (MODIFIED - dynamic device creation)
├── ViewModels/
│   ├── InitSetupViewModel.cs (MODIFIED - VID/PID tracking)
│   └── MainWindowViewModel.cs (MODIFIED - uses IDeviceController)
├── App.axaml.cs (MODIFIED - VID/PID flow)
├── ServiceCollectionExtensions.cs (MODIFIED - registered RazerStreamControllerController)
└── README.md (UPDATED - documented Razer support)
```

## Configuration Files

The application creates separate configuration files for each device:
- `config.json` - Loupedeck Live S configuration
- `config_razer.json` - Razer Stream Controller configuration

This allows using different devices without configuration conflicts.

## Testing Checklist

### Connection & Detection
- [ ] Device appears in setup dialog with correct name and VID:PID
- [ ] Serial connection succeeds at 921600 baud
- [ ] Device info (serial, version) retrieved successfully

### Physical Buttons
- [ ] All 8 buttons respond to press
- [ ] Button LEDs light up with correct colors
- [ ] Button commands execute properly
- [ ] Button color changes when configured

### Rotary Encoders
- [ ] All 6 knobs rotate (left/right detection)
- [ ] All 6 knobs press (click detection)
- [ ] Rotation commands execute
- [ ] Click commands execute
- [ ] Page switching for rotary buttons works

### Touch Screen
- [ ] All 12 touch buttons (4×3 grid) respond
- [ ] Touch button graphics render correctly
- [ ] Touch commands execute
- [ ] Haptic feedback (vibration) works on touch
- [ ] Wallpaper renders across buttons
- [ ] Page switching for touch buttons works

### Display
- [ ] Center screen renders correctly
- [ ] Left screen renders (behind left knobs) - if used
- [ ] Right screen renders (behind right knobs) - if used
- [ ] Brightness adjustment works
- [ ] Text rendering works
- [ ] Image rendering works

### Configuration
- [ ] Settings save to `config_razer.json`
- [ ] Settings load on restart
- [ ] Multiple pages work for touch buttons
- [ ] Multiple pages work for rotary buttons

## Known Differences from Live S

1. **Touch Grid**: 4 columns instead of 5 (12 buttons vs 15)
2. **Rotary Encoders**: 6 instead of 2
3. **Physical Buttons**: 8 instead of 4
4. **Display Layout**: 3-section vs single screen
5. **Config File**: Separate file to avoid conflicts

## Troubleshooting

### Device Not Detected
- Ensure VID:PID is `1532:0d06`
- Check USB connection
- Try different USB port
- Check permissions (Linux: add udev rules)

### Buttons Not Responding
- Check serial baudrate (should be 921600)
- Verify device firmware version
- Check button mapping in Constants.cs

### Display Issues
- Verify correct display IDs (left/center/right)
- Check image buffer size (width × height × 2 bytes)
- Ensure RGB565 format (little-endian)

### Configuration Not Saving
- Check write permissions for config file
- Verify config path is accessible
- Check for JSON serialization errors

## Future Enhancements

Potential improvements for Razer Stream Controller support:
1. Utilize left/right display areas for knob feedback
2. Add visual indicators for active knob
3. Custom knob icons on side displays
4. Per-knob brightness control
5. Advanced haptic feedback patterns
6. Knob sensitivity settings

## References

- [foxxyz/loupedeck](https://github.com/foxxyz/loupedeck) - Node.js implementation reference
- Loupedeck protocol documentation (community-driven)
- Razer Stream Controller uses same protocol as Loupedeck Live

## Credits

Implementation based on the existing LoupixDeck codebase by RadiatorTwo, with Razer Stream Controller support added through protocol analysis and reference from the foxxyz/loupedeck Node.js library.

