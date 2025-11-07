using Avalonia.Media;
using Newtonsoft.Json;
using SkiaSharp;

namespace LoupixDeck.Models;

public class TouchButton(int index) : LoupedeckButton
{
    public int Index { get; } = index;

    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            if (value == _text) return;
            _text = value;
            //OnPropertyChanged(nameof(Text));
            Refresh();
        }
    }

    private bool _textCentered = true;

    public bool TextCentered
    {
        get => _textCentered;
        set
        {
            if (value == _textCentered) return;
            _textCentered = value;
            Refresh();
            OnPropertyChanged(nameof(TextCentered));
        }
    }

    private int _textSize = 16;

    public int TextSize
    {
        get => _textSize;
        set
        {
            if (value == _textSize) return;
            _textSize = value;
            Refresh();
        }
    }

    private int _textPositionX;

    public int TextPositionX
    {
        get => _textPositionX;
        set
        {
            if (_textPositionY == value) return;
            _textPositionX = value;
            Refresh();
            OnPropertyChanged(nameof(TextPositionX));
        }
    }

    private int _textPositionY;

    public int TextPositionY
    {
        get => _textPositionY;
        set
        {
            if (_textPositionY == value) return;
            _textPositionY = value;
            Refresh();
            OnPropertyChanged(nameof(TextPositionY));
        }
    }

    private Color _textColor = Colors.White;

    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (Equals(value, _textColor)) return;
            _textColor = value;
            Refresh();
        }
    }

    private bool _bold;

    public bool Bold
    {
        get => _bold;
        set
        {
            if (value == _bold) return;
            _bold = value;
            Refresh();
        }
    }

    private bool _italic;

    public bool Italic
    {
        get => _italic;
        set
        {
            if (value == _italic) return;
            _italic = value;
            Refresh();
        }
    }

    private Color _backColor = Colors.Black;

    public Color BackColor
    {
        get => _backColor;
        set
        {
            if (Equals(value, _backColor)) return;
            _backColor = value;
            Refresh();
        }
    }

    private bool _outlined;

    public bool Outlined
    {
        get => _outlined;
        set
        {
            if (value == _outlined) return;
            _outlined = value;
            Refresh();
            OnPropertyChanged(nameof(Outlined));
        }
    }

    private Color _outlineColor = Colors.Black;

    public Color OutlineColor
    {
        get => _outlineColor;
        set
        {
            if (Equals(value, _outlineColor)) return;
            _outlineColor = value;
            Refresh();
        }
    }

    private SKBitmap _image;

    public SKBitmap Image
    {
        get => _image;
        set
        {
            if (Equals(value, _image)) return;
            _image = value;
            Refresh();
        }
    }

    private int _imagePositionX;

    public int ImagePositionX
    {
        get => _imagePositionX;
        set
        {
            if (_imagePositionX == value) return;
            _imagePositionX = value;
            Refresh();
            OnPropertyChanged(nameof(ImagePositionX));
        }
    }

    private int _imagePositionY;

    public int ImagePositionY
    {
        get => _imagePositionY;
        set
        {
            if (_imagePositionY == value) return;
            _imagePositionY = value;
            Refresh();
            OnPropertyChanged(nameof(ImagePositionY));
        }
    }

    private int _imageScale = 100;

    public int ImageScale
    {
        get => _imageScale;
        set
        {
            if (_imageScale == value) return;
            _imageScale = value;
            Refresh();
            OnPropertyChanged(nameof(ImageScale));
        }
    }

    private int _rotation = 0;

    [Obsolete("Use TextRotation or ImageRotation instead")]
    public int Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation == value) return;
            _rotation = value;
            // For backward compatibility, set both text and image rotation
            _textRotation = value;
            _imageRotation = value;
            Refresh();
            OnPropertyChanged(nameof(Rotation));
            OnPropertyChanged(nameof(TextRotation));
            OnPropertyChanged(nameof(ImageRotation));
        }
    }

    private int _textRotation = 0;

    public int TextRotation
    {
        get => _textRotation;
        set
        {
            if (_textRotation == value) return;
            _textRotation = value;
            Refresh();
            OnPropertyChanged(nameof(TextRotation));
        }
    }

    private int _imageRotation = 0;

    public int ImageRotation
    {
        get => _imageRotation;
        set
        {
            if (_imageRotation == value) return;
            _imageRotation = value;
            Refresh();
            OnPropertyChanged(nameof(ImageRotation));
        }
    }

    private SKBitmap _renderedImage;

    [JsonIgnore]
    public SKBitmap RenderedImage
    {
        get => _renderedImage;
        set
        {
            if (Equals(value, _renderedImage)) return;
            _renderedImage = value;
            OnPropertyChanged(nameof(RenderedImage));
        }
    }

    private bool _vibrationEnabled = false;

    public bool VibrationEnabled
    {
        get => _vibrationEnabled;
        set
        {
            if (value == _vibrationEnabled) return;
            _vibrationEnabled = value;
            OnPropertyChanged(nameof(VibrationEnabled));
        }
    }

    private byte _vibrationPattern;

    public byte VibrationPattern
    {
        get
        {
            // If not explicitly set, return ShortLower as default for all buttons
            if (_vibrationPattern == 0)
            {
                return LoupedeckDevice.Constants.VibrationPattern.ShortLower;
            }
            return _vibrationPattern;
        }
        set
        {
            if (value == _vibrationPattern) return;
            _vibrationPattern = value;
            OnPropertyChanged(nameof(VibrationPattern));
        }
    }
}