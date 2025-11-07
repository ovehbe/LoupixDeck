#!/bin/bash
set -e  # Stop on error

APP_NAME="LoupixDeck"
DESCRIPTION="Loupedeck controller app"
PUBLISH_DIR="$(pwd)/publish"
EXEC_PATH="$PUBLISH_DIR/LoupixDeck"
ICON_PATH="$PUBLISH_DIR/LoupixDeck.png"
DESKTOP_FILE="$HOME/.local/share/applications/${APP_NAME}.desktop"

echo "🚀 Publishing $APP_NAME..."
dotnet publish LoupixDeck.csproj -c Release -r linux-x64 --self-contained true \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:EnableCompressionInSingleFile=true \
    /p:ReadyToRun=true \
    -o "$PUBLISH_DIR"

echo "🔗 Creating symlink in ~/.local/bin..."
mkdir -p ~/.local/bin
ln -sf "$EXEC_PATH" "$HOME/.local/bin/${APP_NAME,,}"  # lowercase command name

echo "🖼️  Creating .desktop launcher..."
mkdir -p "$(dirname "$DESKTOP_FILE")"

cat > "$DESKTOP_FILE" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=$APP_NAME
Comment=$DESCRIPTION
Exec=$EXEC_PATH
Icon=$ICON_PATH
Terminal=false
Categories=Utility;
EOF

chmod +x "$DESKTOP_FILE"

echo "✅ Done!"
echo "You can now launch '$APP_NAME' from your app menu or run '${APP_NAME,,}' from terminal."
