using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Newtonsoft.Json;
using SkiaSharp;

namespace LoupixDeck.Models;

/// <summary>
/// This data model holds all configuration settings,
/// which are loaded and saved via JSON.
/// </summary>
public class LoupedeckConfig : INotifyPropertyChanged
{
    private int _currentRotaryPageIndex = -1;
    private int _currentTouchPageIndex = -1;

    private int _brightness = 100;

    public string DevicePort { get; set; }
    public int DeviceBaudrate { get; set; }
    public string DeviceVid { get; set; }
    public string DevicePid { get; set; }
    public int DeviceColumns { get; set; } = 5; // Default to Live S (5x3)
    public int DeviceRows { get; set; } = 3;
    public int DeviceTouchButtonCount { get; set; } = 15; // 5x3 = 15
    public int DeviceRotaryCount { get; set; } = 2; // Default to Live S (2 knobs)

    public SimpleButton[] SimpleButtons { get; set; }

    public ObservableCollection<RotaryButtonPage> RotaryButtonPages { get; set; } = [];

    [JsonIgnore]
    public int CurrentRotaryPageIndex
    {
        get => _currentRotaryPageIndex;
        set
        {
            if (_currentRotaryPageIndex != value)
            {
                _currentRotaryPageIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentRotaryButtonPage));
            }
        }
    }

    [JsonIgnore]
    public RotaryButtonPage CurrentRotaryButtonPage =>
        (RotaryButtonPages != null &&
         _currentRotaryPageIndex >= 0 &&
         _currentRotaryPageIndex < RotaryButtonPages.Count)
            ? RotaryButtonPages[_currentRotaryPageIndex]
            : null;

    public ObservableCollection<TouchButtonPage> TouchButtonPages { get; set; } = [];

    [JsonIgnore]
    public int CurrentTouchPageIndex
    {
        get => _currentTouchPageIndex;
        set
        {
            if (_currentTouchPageIndex != value)
            {
                _currentTouchPageIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentTouchButtonPage));
            }
        }
    }

    [JsonIgnore]
    public TouchButtonPage CurrentTouchButtonPage =>
        (TouchButtonPages != null &&
         _currentTouchPageIndex >= 0 &&
         _currentTouchPageIndex < TouchButtonPages.Count)
            ? TouchButtonPages[_currentTouchPageIndex]
            : null;

    public int Brightness
    {
        get => _brightness;
        set
        {
            if (_brightness == value) return;
            _brightness = value;
            OnPropertyChanged();
        }
    }

    private SKBitmap _wallpaper;

    public SKBitmap Wallpaper
    {
        get => _wallpaper;
        set
        {
            if (Equals(value, _wallpaper)) return;
            _wallpaper = value;
            OnPropertyChanged();
        }
    }

    private double _wallpaperOpacity;

    public double WallpaperOpacity
    {
        get => _wallpaperOpacity;
        set
        {
            if (!(Math.Abs(_wallpaperOpacity - value) > 0.0001)) return;
            _wallpaperOpacity = value;
            OnPropertyChanged();
        }
    }

    // Touch feedback settings
    private bool _touchFeedbackEnabled = true;
    public bool TouchFeedbackEnabled
    {
        get => _touchFeedbackEnabled;
        set
        {
            if (_touchFeedbackEnabled == value) return;
            _touchFeedbackEnabled = value;
            OnPropertyChanged();
        }
    }

    private Color _touchFeedbackColor = Colors.White;
    public Color TouchFeedbackColor
    {
        get => _touchFeedbackColor;
        set
        {
            if (Equals(_touchFeedbackColor, value)) return;
            _touchFeedbackColor = value;
            OnPropertyChanged();
        }
    }

    private double _touchFeedbackOpacity = 0.7;
    public double TouchFeedbackOpacity
    {
        get => _touchFeedbackOpacity;
        set
        {
            if (!(Math.Abs(_touchFeedbackOpacity - value) > 0.0001)) return;
            _touchFeedbackOpacity = value;
            OnPropertyChanged();
        }
    }

    // Global Commands - Touch Buttons
    private bool _touchButtonPrefixEnabled = false;
    public bool TouchButtonPrefixEnabled
    {
        get => _touchButtonPrefixEnabled;
        set
        {
            if (_touchButtonPrefixEnabled == value) return;
            _touchButtonPrefixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string TouchButtonPrefixCommand { get; set; } = "";
    
    private bool _touchButtonSuffixEnabled = false;
    public bool TouchButtonSuffixEnabled
    {
        get => _touchButtonSuffixEnabled;
        set
        {
            if (_touchButtonSuffixEnabled == value) return;
            _touchButtonSuffixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string TouchButtonSuffixCommand { get; set; } = "";

    // Global Commands - Simple Buttons
    private bool _simpleButtonPrefixEnabled = false;
    public bool SimpleButtonPrefixEnabled
    {
        get => _simpleButtonPrefixEnabled;
        set
        {
            if (_simpleButtonPrefixEnabled == value) return;
            _simpleButtonPrefixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string SimpleButtonPrefixCommand { get; set; } = "";
    
    private bool _simpleButtonSuffixEnabled = false;
    public bool SimpleButtonSuffixEnabled
    {
        get => _simpleButtonSuffixEnabled;
        set
        {
            if (_simpleButtonSuffixEnabled == value) return;
            _simpleButtonSuffixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string SimpleButtonSuffixCommand { get; set; } = "";

    // Global Commands - Knobs (3 types: left, right, press)
    private bool _knobLeftPrefixEnabled = false;
    public bool KnobLeftPrefixEnabled
    {
        get => _knobLeftPrefixEnabled;
        set
        {
            if (_knobLeftPrefixEnabled == value) return;
            _knobLeftPrefixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string KnobLeftPrefixCommand { get; set; } = "";
    
    private bool _knobLeftSuffixEnabled = false;
    public bool KnobLeftSuffixEnabled
    {
        get => _knobLeftSuffixEnabled;
        set
        {
            if (_knobLeftSuffixEnabled == value) return;
            _knobLeftSuffixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string KnobLeftSuffixCommand { get; set; } = "";
    
    private bool _knobRightPrefixEnabled = false;
    public bool KnobRightPrefixEnabled
    {
        get => _knobRightPrefixEnabled;
        set
        {
            if (_knobRightPrefixEnabled == value) return;
            _knobRightPrefixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string KnobRightPrefixCommand { get; set; } = "";
    
    private bool _knobRightSuffixEnabled = false;
    public bool KnobRightSuffixEnabled
    {
        get => _knobRightSuffixEnabled;
        set
        {
            if (_knobRightSuffixEnabled == value) return;
            _knobRightSuffixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string KnobRightSuffixCommand { get; set; } = "";
    
    private bool _knobPressPrefixEnabled = false;
    public bool KnobPressPrefixEnabled
    {
        get => _knobPressPrefixEnabled;
        set
        {
            if (_knobPressPrefixEnabled == value) return;
            _knobPressPrefixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string KnobPressPrefixCommand { get; set; } = "";
    
    private bool _knobPressSuffixEnabled = false;
    public bool KnobPressSuffixEnabled
    {
        get => _knobPressSuffixEnabled;
        set
        {
            if (_knobPressSuffixEnabled == value) return;
            _knobPressSuffixEnabled = value;
            OnPropertyChanged();
        }
    }
    
    public string KnobPressSuffixCommand { get; set; } = "";

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Helper method for programmatic property setting
    public Task SetBrightness(double value)
    {
        Brightness = (int)(value * 100);
        return Task.CompletedTask;
    }
}