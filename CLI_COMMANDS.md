# LoupixDeck CLI Commands

## Usage

Control your Razer Stream Controller from the terminal while the app is running.

### Start the Application First

```bash
dotnet run
# Or if you built it:
./bin/Debug/net9.0/LoupixDeck
```

---

## Available Commands

### 1. Device OFF (Load OFF Config)
```bash
./loupixdeck-cli off
# or
./bin/Debug/net9.0/LoupixDeck off
```
**Effect**: Loads `config_razer_off.json` - turns off display, all LEDs black, screen clears

### 2. Device ON (Reload Config)
```bash
./loupixdeck-cli on
# or
./bin/Debug/net9.0/LoupixDeck on
```
**Effect**: Reloads `config_razer.json` - restores brightness, buttons, everything

### 3. Device ON/OFF Toggle
```bash
./loupixdeck-cli on-off
```
**Effect**: Toggles device state - if OFF turns ON, if ON turns OFF

### 4. Device Wakeup (Reconnect)
```bash
./loupixdeck-cli wakeup
```
**Effect**: Reconnects the device (useful after system suspend/resume) and turns it ON

### 5. Toggle Window Visibility
```bash
./loupixdeck-cli toggle
# or
./loupixdeck-cli show
# or
./loupixdeck-cli hide
```
**Effect**: Shows window if hidden, hides if shown

### 6. Quit Application
```bash
./loupixdeck-cli quit
```
**Effect**: Applies OFF config, then exits (device goes dark)

---

## Example Use Cases

### Automatic Device Control with Scripts

**Turn off device when locking screen:**
```bash
# Add to ~/.config/systemd/user/loupixdeck-lock.service
./loupixdeck-cli off
```

**Turn on device when unlocking:**
```bash
./loupixdeck-cli on
```

**Reconnect device after system suspend/resume:**
```bash
# Add to wakeup script or systemd resume service
./loupixdeck-cli wakeup
```

### Keyboard Shortcuts

Bind to keyboard shortcuts in your DE:
- `Super+L` → `./loupixdeck-cli off` (turn off device)
- `Super+O` → `./loupixdeck-cli on` (turn on device)
- `Super+D` → `./loupixdeck-cli toggle` (show/hide GUI)

### Systemd Integration

**Auto-start on boot:**
```bash
# Create ~/.config/systemd/user/loupixdeck.service
[Unit]
Description=LoupixDeck Razer Stream Controller

[Service]
ExecStart=/home/ovehbe/Code/LoupixDeck-master/bin/Debug/net9.0/LoupixDeck
Restart=always

[Install]
WantedBy=default.target
```

Then:
```bash
systemctl --user enable loupixdeck
systemctl --user start loupixdeck
```

**Control from terminal anytime:**
```bash
./loupixdeck-cli off  # Turn device off
./loupixdeck-cli on   # Turn device on
```

---

## Response Messages

All commands return a response:
- `OK: System.DeviceOff executed` - Command successful
- `ERROR: ...` - Command failed
- `Unknown command. Available: on, off, on-off, wakeup, toggle, show, hide, quit` - Invalid command

---

## Installation for System-Wide Access

### Option 1: Symlink to ~/bin
```bash
mkdir -p ~/bin
ln -s /home/ovehbe/Code/LoupixDeck-master/loupixdeck-cli ~/bin/loupixdeck
# Add ~/bin to PATH if not already
export PATH="$HOME/bin:$PATH"
```

Then use anywhere:
```bash
loupixdeck off
loupixdeck on
loupixdeck toggle
```

### Option 2: Alias in ~/.bashrc
```bash
echo 'alias loupixdeck="/home/ovehbe/Code/LoupixDeck-master/loupixdeck-cli"' >> ~/.bashrc
source ~/.bashrc
```

Then:
```bash
loupixdeck off
loupixdeck on
```

---

## Troubleshooting

### "LoupixDeck is not running"
- Start the app first: `dotnet run` or `./bin/Debug/net9.0/LoupixDeck`

### "Connection refused"
- App crashed or exited
- Check if socket exists: `ls -la /tmp/loupixdeck_app.sock`

### Commands don't work
- Make sure CLI wrapper is executable: `chmod +x loupixdeck-cli`
- Try direct binary: `./bin/Debug/net9.0/LoupixDeck off`

---

## Summary

**While app is running**, control it from terminal:
- `./loupixdeck-cli on` - Turn device ON
- `./loupixdeck-cli off` - Turn device OFF
- `./loupixdeck-cli on-off` - Toggle device ON/OFF
- `./loupixdeck-cli wakeup` - Reconnect device and turn ON (for suspend/resume)
- `./loupixdeck-cli toggle` - Show/Hide window
- `./loupixdeck-cli quit` - Exit app (with device clear)

Perfect for automation, scripts, and keyboard shortcuts! 🚀

