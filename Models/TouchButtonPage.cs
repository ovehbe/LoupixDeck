using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SkiaSharp;

namespace LoupixDeck.Models;

public class TouchButtonPage : INotifyPropertyChanged
{
    public TouchButtonPage(int pageSize)
    {
        TouchButtons = new ObservableCollection<TouchButton>();

        for (var i = 0; i < pageSize; i++)
        {
            var newButton = new TouchButton(i);
            TouchButtons.Add(newButton);
        }
    }
        
    private int _page;
    private bool _selected;

    public string PageName => $"Page: {Page}";

    public int Page
    {
        get => _page;
        set
        {
            if (_page == value) return;
            _page = value;
            OnPropertyChanged();
        }
    }
    
    [JsonIgnore]
    public bool Selected
    {
        get => _selected;
        set
        {
            if (value == _selected) return;
            _selected = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TouchButton> TouchButtons { get; set; }

    // Wallpaper settings for this page
    private SKBitmap _wallpaper;
    [JsonIgnore]
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

    // Global Commands for Touch Buttons on this page
    private bool _touchButtonPrefixEnabled;
    public bool TouchButtonPrefixEnabled
    {
        get => _touchButtonPrefixEnabled;
        set
        {
            if (value == _touchButtonPrefixEnabled) return;
            _touchButtonPrefixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _touchButtonPrefixCommand = string.Empty;
    public string TouchButtonPrefixCommand
    {
        get => _touchButtonPrefixCommand;
        set
        {
            if (value == _touchButtonPrefixCommand) return;
            _touchButtonPrefixCommand = value;
            OnPropertyChanged();
        }
    }

    private bool _touchButtonSuffixEnabled;
    public bool TouchButtonSuffixEnabled
    {
        get => _touchButtonSuffixEnabled;
        set
        {
            if (value == _touchButtonSuffixEnabled) return;
            _touchButtonSuffixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _touchButtonSuffixCommand = string.Empty;
    public string TouchButtonSuffixCommand
    {
        get => _touchButtonSuffixCommand;
        set
        {
            if (value == _touchButtonSuffixCommand) return;
            _touchButtonSuffixCommand = value;
            OnPropertyChanged();
        }
    }
        
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}