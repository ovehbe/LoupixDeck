# LoupixDeck Installation Guide

## Prerequisites

- **.NET 9.0 SDK** or later
- **Linux** (tested on Elementary OS, Ubuntu, etc.) or **Windows**

### Install .NET SDK:
```bash
# Ubuntu/Debian/Elementary OS
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# Or use your package manager (may have older version)
sudo apt install dotnet-sdk-9.0
```

Verify installation:
```bash
dotnet --version
```

---

## Installation Methods

### Method 1: Quick Install (Recommended)

```bash
# Download and extract (choose your preferred tag)
cd ~/Downloads
wget https://github.com/ovehbe/LoupixDeck/archive/refs/tags/v1.1-toggle-command.tar.gz
tar -xzf v1.1-toggle-command.tar.gz
cd LoupixDeck-v1.1-toggle-command

# Build release version
dotnet publish LoupixDeck.csproj -c Release -r linux-x64 --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:EnableCompressionInSingleFile=true \
    /p:ReadyToRun=true \
    -o ~/Applications/LoupixDeck

# Make executable
chmod +x ~/Applications/LoupixDeck/LoupixDeck
```

**Run it:**
```bash
~/Applications/LoupixDeck/LoupixDeck
```

---

### Method 2: System-Wide Installation

```bash
# Clone or download
git clone https://github.com/ovehbe/LoupixDeck.git
cd LoupixDeck

# Build and install to /usr/local
sudo dotnet publish LoupixDeck.csproj -c Release -r linux-x64 --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:EnableCompressionInSingleFile=true \
    /p:ReadyToRun=true \
    -o /usr/local/bin/loupixdeck

# Create symlink (optional)
sudo ln -s /usr/local/bin/loupixdeck/LoupixDeck /usr/local/bin/loupixdeck
```

**Run it:**
```bash
loupixdeck
```

---

### Method 3: Development Mode

```bash
git clone https://github.com/ovehbe/LoupixDeck.git
cd LoupixDeck
dotnet restore
dotnet build

# Run directly
dotnet run
```

---

## Create Desktop Entry (Optional)

Create `~/.local/share/applications/loupixdeck.desktop`:

```ini
[Desktop Entry]
Name=LoupixDeck
Comment=Razer Stream Controller & Loupedeck Live S Control
Exec=/home/YOUR_USERNAME/Applications/LoupixDeck/LoupixDeck
Icon=/home/YOUR_USERNAME/Applications/LoupixDeck/LoupixDeck.ico
Terminal=false
Type=Application
Categories=Utility;AudioVideo;
StartupNotify=true
```

Replace `YOUR_USERNAME` with your actual username.

**Update desktop database:**
```bash
update-desktop-database ~/.local/share/applications
```

Now LoupixDeck will appear in your app launcher!

---

## Set Up Keyboard Shortcut

### Elementary OS / Pantheon:
1. System Settings → Keyboard → Shortcuts → Custom
2. Click **+**
3. **Name**: Toggle LoupixDeck
4. **Command**: `/home/YOUR_USERNAME/Applications/LoupixDeck/LoupixDeck`
5. **Shortcut**: Press `Super+L` (or your preference)

### GNOME:
```bash
gsettings set org.gnome.settings-daemon.plugins.media-keys custom-keybindings "['/org/gnome/settings-daemon/plugins/media-keys/custom-keybindings/loupixdeck/']"
gsettings set org.gnome.settings-daemon.plugins.media-keys.custom-keybinding:/org/gnome/settings-daemon/plugins/media-keys/custom-keybindings/loupixdeck/ name 'Toggle LoupixDeck'
gsettings set org.gnome.settings-daemon.plugins.media-keys.custom-keybinding:/org/gnome/settings-daemon/plugins/media-keys/custom-keybindings/loupixdeck/ command '/home/YOUR_USERNAME/Applications/LoupixDeck/LoupixDeck'
gsettings set org.gnome.settings-daemon.plugins.media-keys.custom-keybinding:/org/gnome/settings-daemon/plugins/media-keys/custom-keybindings/loupixdeck/ binding '<Super>l'
```

---

## Linux USB Permissions (Razer Stream Controller)

Create udev rule:
```bash
sudo nano /etc/udev/rules.d/99-razer-stream-controller.rules
```

Add:
```
SUBSYSTEM=="usb", ATTRS{idVendor}=="1532", ATTRS{idProduct}=="0d06", MODE="0666"
```

Reload:
```bash
sudo udevadm control --reload-rules
sudo udevadm trigger
```

---

## Uninstall

### Method 1 (User install):
```bash
rm -rf ~/Applications/LoupixDeck
rm ~/.local/share/applications/loupixdeck.desktop
update-desktop-database ~/.local/share/applications
```

### Method 2 (System install):
```bash
sudo rm -rf /usr/local/bin/loupixdeck
sudo rm /usr/local/bin/loupixdeck  # If symlink was created
```

---

## Updating

```bash
cd LoupixDeck
git pull origin main
# Or download new release

# Rebuild
dotnet publish LoupixDeck.csproj -c Release -r linux-x64 --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:EnableCompressionInSingleFile=true \
    /p:ReadyToRun=true \
    -o ~/Applications/LoupixDeck
```

---

## Troubleshooting

### "dotnet: command not found"
Install .NET SDK (see Prerequisites above)

### Device not detected
- Check USB: `lsusb | grep 1532:0d06`
- Add udev rules (see above)
- Try different USB port

### Permission denied
Add udev rules for USB device access

### App won't start
Check logs in terminal:
```bash
~/Applications/LoupixDeck/LoupixDeck
```

Look for error messages and check prerequisites are installed.

