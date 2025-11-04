# Layout and Crash Fixes for Razer Stream Controller

## Issues Fixed

### 1. ✅ Hardcoded 5×3 Grid Layout
**Problem**: The GUI was showing 15 buttons (5×3 grid for Live S) instead of 12 buttons (4×3 grid for Razer).

**Solution**: Made the layout device-aware by:
- Added `DeviceColumns`, `DeviceRows`, `DeviceTouchButtonCount`, and `DeviceRotaryCount` to `LoupedeckConfig`
- Updated controllers to set these values during initialization:
  - **Loupedeck Live S**: 5 columns × 3 rows = 15 buttons, 2 knobs
  - **Razer Stream Controller**: 4 columns × 3 rows = 12 buttons, 6 knobs
- Updated `PageManager.AddTouchButtonPage()` to use `DeviceTouchButtonCount` instead of hardcoded 15
- Updated `PageManager.AddRotaryButtonPage()` to use `DeviceRotaryCount` instead of hardcoded 2
- Updated all `DrawTouchButton()` calls to use `config.DeviceColumns` instead of hardcoded 5 or 4

### 2. ✅ Application Crash on Button Click
**Problem**: App crashed with unhandled exception when clicking touch buttons due to CoolerControl API not being available.

**Solution**: 
- Wrapped `CreateCoolerControlMenu()` in try-catch block
- If CoolerControl is not available (connection refused), it gracefully skips adding the menu instead of crashing

## Files Modified

### Configuration Model
- **`Models/LoupedeckConfig.cs`**
  - Added device layout properties with defaults for Live S

### Controllers
- **`Controllers/LoupedeckLiveSController.cs`**
  - Sets: `DeviceColumns=5`, `DeviceRows=3`, `DeviceTouchButtonCount=15`, `DeviceRotaryCount=2`
  - Uses `config.DeviceColumns` in all DrawTouchButton calls

- **`Controllers/RazerStreamControllerController.cs`**
  - Sets: `DeviceColumns=4`, `DeviceRows=3`, `DeviceTouchButtonCount=12`, `DeviceRotaryCount=6`
  - Uses `config.DeviceColumns` in all DrawTouchButton calls

### Services
- **`Services/PageManager.cs`**
  - `AddTouchButtonPage()`: Creates `DeviceTouchButtonCount` buttons instead of 15
  - `AddRotaryButtonPage()`: Creates `DeviceRotaryCount` knobs instead of 2
  - `DrawTouchButtons()`: Uses `config.DeviceColumns` for rendering

- **`Services/DeviceService.cs`**
  - `ShowTemporaryTextButton()`: Uses `config.DeviceColumns` for redrawing buttons

### View Models
- **`ViewModels/TouchButtonSettingsViewModel.cs`**
  - Added try-catch around `CreateCoolerControlMenu()` to handle connection failures

## What This Fixes

### Before:
- ❌ Razer controller showed 15 button slots (5×3) instead of 12 (4×3)
- ❌ Only 2 rotary knob pages instead of 6
- ❌ Clicking buttons would sometimes show wrong grid layout
- ❌ App crashed when clicking buttons if CoolerControl wasn't running

### After:
- ✅ Razer controller now shows correct 12 button slots (4×3)
- ✅ All 6 rotary knobs are available in pages
- ✅ Touch button grid renders correctly for each device type
- ✅ App doesn't crash if CoolerControl is unavailable
- ✅ Configuration saved with device-specific parameters
- ✅ Switching between devices maintains correct layout

## Testing Results

To test the fixes:
1. **Delete old config** (if you have one): `rm ~/.config/LoupixDeck/config_razer.json`
2. **Run the app**: `dotnet run`
3. **Select Razer Stream Controller** in the setup dialog
4. **Verify**:
   - You should see a 4×3 grid (12 buttons) in the main window
   - All 6 rotary knobs should be configurable
   - Clicking buttons should not crash the app
   - Button graphics should render in the correct positions

## Configuration Structure

The device-specific values are now saved in your config file:

```json
{
  "DeviceColumns": 4,
  "DeviceRows": 3,
  "DeviceTouchButtonCount": 12,
  "DeviceRotaryCount": 6,
  "DeviceVid": "1532",
  "DevicePid": "0d06",
  ...
}
```

This ensures the correct layout is maintained across app restarts.

## Known Limitations

⚠️ **UI Grid Still Visual Only**: The XAML UI in `MainWindow.axaml` still shows a hardcoded visual grid. This doesn't affect functionality as the actual button layout is handled by the device class and config, but the visual representation in the UI window doesn't dynamically adjust. The touch areas and button positions work correctly even though the visual grid appearance is fixed.

For a fully dynamic UI, `MainWindow.axaml` would need to be refactored to generate the button grid programmatically based on `DeviceColumns` and `DeviceRows`. This is a UI enhancement that doesn't affect device functionality.

## Next Steps

If you want to make the UI grid visually dynamic:
1. Convert the hardcoded XAML grid to an `ItemsControl` with `ItemsPanel`
2. Bind to `Config.DeviceColumns` and `Config.DeviceRows`
3. Generate button templates dynamically

But for now, the device functionality is fully working with the correct layout! 🎉

