using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using LoupixDeck.Models;
using LoupixDeck.Services;
using LoupixDeck.Utils;
using LoupixDeck.ViewModels.Base;
using OBSWebsocketDotNet.Communication;
using SkiaSharp;

namespace LoupixDeck.ViewModels;

public class SettingsViewModel : DialogViewModelBase<DialogResult>
{
    public LoupedeckConfig Config { get; }
    private readonly IObsController _obs;
    public ICommand SaveObsCommand { get; }
    public ICommand TestConnectionCommand { get; }
    public ICommand SelectImageButtonCommand { get; }
    public ICommand RemoveWallpaperCommand { get; }
    public ICommand NavigateCommand { get; }

    private SKBitmap _wallpaperBitmap = null;

    private SettingsView _currentView;

    public SettingsView CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ObsConfig ObsConfig { get; }

    public ObservableCollection<BitmapHelper.ScalingOption> WallpaperScalingOptions { get; } =
    [
        BitmapHelper.ScalingOption.None,
        BitmapHelper.ScalingOption.Fill,
        BitmapHelper.ScalingOption.Fit,
        BitmapHelper.ScalingOption.Stretch,
        BitmapHelper.ScalingOption.Tile,
        BitmapHelper.ScalingOption.Center,
        // BitmapHelper.ScalingOption.CropToFill
    ];

    private BitmapHelper.ScalingOption _selectedWallpaperScalingOption = BitmapHelper.ScalingOption.Fit;

    public BitmapHelper.ScalingOption SelectedWallpaperScalingOption
    {
        get => _selectedWallpaperScalingOption;
        set
        {
            SetProperty(ref _selectedWallpaperScalingOption, value);
            ApplyScaling();
        }
    }

    private int _wallpaperScaling = 100;

    public int WallpaperScaling
    {
        get => _wallpaperScaling;
        set
        {
            SetProperty(ref _wallpaperScaling, value);
            ApplyScaling();
        }
    }

    private int _wallpaperPositionX;

    public int WallpaperPositionX
    {
        get => _wallpaperPositionX;
        set
        {
            SetProperty(ref _wallpaperPositionX, value);
            ApplyScaling();
        }
    }

    private int _wallpaperPositionY;

    public int WallpaperPositionY
    {
        get => _wallpaperPositionY;
        set
        {
            SetProperty(ref _wallpaperPositionY, value);
            ApplyScaling();
        }
    }

    private void ApplyScaling()
    {
        if (_wallpaperBitmap == null) return;

        var scaledImage = BitmapHelper.ScaleAndPositionBitmap(
            _wallpaperBitmap,
            480, 270,
            WallpaperScaling,
            WallpaperPositionX, WallpaperPositionY,
            SelectedWallpaperScalingOption);

        if (CurrentTouchButtonPageForWallpaper != null)
        {
            CurrentTouchButtonPageForWallpaper.Wallpaper = scaledImage;
        }
    }

    // Wallpaper page selection
    private int _selectedWallpaperPageIndex = 0;
    public int SelectedWallpaperPageIndex
    {
        get => _selectedWallpaperPageIndex;
        set
        {
            if (SetProperty(ref _selectedWallpaperPageIndex, value))
            {
                // Reset the temporary bitmap when switching pages
                _wallpaperBitmap = CurrentTouchButtonPageForWallpaper?.Wallpaper;
                
                // Notify wallpaper properties to refresh
                OnPropertyChanged(nameof(CurrentTouchButtonPageForWallpaper));
                OnPropertyChanged(nameof(CurrentWallpaper));
                OnPropertyChanged(nameof(WallpaperOpacity));
            }
        }
    }

    public TouchButtonPage CurrentTouchButtonPageForWallpaper => 
        Config.TouchButtonPages.Count > _selectedWallpaperPageIndex ? Config.TouchButtonPages[_selectedWallpaperPageIndex] : null;

    public SKBitmap CurrentWallpaper => CurrentTouchButtonPageForWallpaper?.Wallpaper;

    public double WallpaperOpacity
    {
        get => CurrentTouchButtonPageForWallpaper?.WallpaperOpacity ?? 0.0;
        set
        {
            if (CurrentTouchButtonPageForWallpaper != null)
            {
                CurrentTouchButtonPageForWallpaper.WallpaperOpacity = value;
                OnPropertyChanged();
            }
        }
    }

    // Page selection for Global Commands
    public ObservableCollection<string> AvailablePages { get; }

    private int _selectedPageIndex = 0;
    public int SelectedPageIndex
    {
        get => _selectedPageIndex;
        set
        {
            if (SetProperty(ref _selectedPageIndex, value))
            {
                // Notify all global command properties to refresh
                OnPropertyChanged(nameof(CurrentTouchButtonPage));
                OnPropertyChanged(nameof(CurrentRotaryButtonPage));
                OnPropertyChanged(nameof(TouchButtonPrefixEnabled));
                OnPropertyChanged(nameof(TouchButtonPrefixCommand));
                OnPropertyChanged(nameof(TouchButtonSuffixEnabled));
                OnPropertyChanged(nameof(TouchButtonSuffixCommand));
                OnPropertyChanged(nameof(SimpleButtonPrefixEnabled));
                OnPropertyChanged(nameof(SimpleButtonPrefixCommand));
                OnPropertyChanged(nameof(SimpleButtonSuffixEnabled));
                OnPropertyChanged(nameof(SimpleButtonSuffixCommand));
                OnPropertyChanged(nameof(KnobLeftPrefixEnabled));
                OnPropertyChanged(nameof(KnobLeftPrefixCommand));
                OnPropertyChanged(nameof(KnobLeftSuffixEnabled));
                OnPropertyChanged(nameof(KnobLeftSuffixCommand));
                OnPropertyChanged(nameof(KnobRightPrefixEnabled));
                OnPropertyChanged(nameof(KnobRightPrefixCommand));
                OnPropertyChanged(nameof(KnobRightSuffixEnabled));
                OnPropertyChanged(nameof(KnobRightSuffixCommand));
                OnPropertyChanged(nameof(KnobPressPrefixEnabled));
                OnPropertyChanged(nameof(KnobPressPrefixCommand));
                OnPropertyChanged(nameof(KnobPressSuffixEnabled));
                OnPropertyChanged(nameof(KnobPressSuffixCommand));
            }
        }
    }

    public TouchButtonPage CurrentTouchButtonPage => 
        Config.TouchButtonPages.Count > _selectedPageIndex ? Config.TouchButtonPages[_selectedPageIndex] : null;

    public RotaryButtonPage CurrentRotaryButtonPage => 
        Config.RotaryButtonPages.Count > _selectedPageIndex ? Config.RotaryButtonPages[_selectedPageIndex] : null;

    // Wrapper properties for Touch Button global commands
    public bool TouchButtonPrefixEnabled
    {
        get => CurrentTouchButtonPage?.TouchButtonPrefixEnabled ?? false;
        set 
        { 
            if (CurrentTouchButtonPage != null) 
            {
                CurrentTouchButtonPage.TouchButtonPrefixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string TouchButtonPrefixCommand
    {
        get => CurrentTouchButtonPage?.TouchButtonPrefixCommand ?? string.Empty;
        set 
        { 
            if (CurrentTouchButtonPage != null) 
            {
                CurrentTouchButtonPage.TouchButtonPrefixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public bool TouchButtonSuffixEnabled
    {
        get => CurrentTouchButtonPage?.TouchButtonSuffixEnabled ?? false;
        set 
        { 
            if (CurrentTouchButtonPage != null) 
            {
                CurrentTouchButtonPage.TouchButtonSuffixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string TouchButtonSuffixCommand
    {
        get => CurrentTouchButtonPage?.TouchButtonSuffixCommand ?? string.Empty;
        set 
        { 
            if (CurrentTouchButtonPage != null) 
            {
                CurrentTouchButtonPage.TouchButtonSuffixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    // Wrapper properties for Simple Button global commands
    public bool SimpleButtonPrefixEnabled
    {
        get => CurrentRotaryButtonPage?.SimpleButtonPrefixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.SimpleButtonPrefixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string SimpleButtonPrefixCommand
    {
        get => CurrentRotaryButtonPage?.SimpleButtonPrefixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.SimpleButtonPrefixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public bool SimpleButtonSuffixEnabled
    {
        get => CurrentRotaryButtonPage?.SimpleButtonSuffixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.SimpleButtonSuffixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string SimpleButtonSuffixCommand
    {
        get => CurrentRotaryButtonPage?.SimpleButtonSuffixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.SimpleButtonSuffixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    // Wrapper properties for Knob Left global commands
    public bool KnobLeftPrefixEnabled
    {
        get => CurrentRotaryButtonPage?.KnobLeftPrefixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobLeftPrefixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string KnobLeftPrefixCommand
    {
        get => CurrentRotaryButtonPage?.KnobLeftPrefixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobLeftPrefixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public bool KnobLeftSuffixEnabled
    {
        get => CurrentRotaryButtonPage?.KnobLeftSuffixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobLeftSuffixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string KnobLeftSuffixCommand
    {
        get => CurrentRotaryButtonPage?.KnobLeftSuffixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobLeftSuffixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    // Wrapper properties for Knob Right global commands
    public bool KnobRightPrefixEnabled
    {
        get => CurrentRotaryButtonPage?.KnobRightPrefixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobRightPrefixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string KnobRightPrefixCommand
    {
        get => CurrentRotaryButtonPage?.KnobRightPrefixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobRightPrefixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public bool KnobRightSuffixEnabled
    {
        get => CurrentRotaryButtonPage?.KnobRightSuffixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobRightSuffixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string KnobRightSuffixCommand
    {
        get => CurrentRotaryButtonPage?.KnobRightSuffixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobRightSuffixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    // Wrapper properties for Knob Press global commands
    public bool KnobPressPrefixEnabled
    {
        get => CurrentRotaryButtonPage?.KnobPressPrefixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobPressPrefixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string KnobPressPrefixCommand
    {
        get => CurrentRotaryButtonPage?.KnobPressPrefixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobPressPrefixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public bool KnobPressSuffixEnabled
    {
        get => CurrentRotaryButtonPage?.KnobPressSuffixEnabled ?? false;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobPressSuffixEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string KnobPressSuffixCommand
    {
        get => CurrentRotaryButtonPage?.KnobPressSuffixCommand ?? string.Empty;
        set 
        { 
            if (CurrentRotaryButtonPage != null) 
            {
                CurrentRotaryButtonPage.KnobPressSuffixCommand = value;
                OnPropertyChanged();
            }
        }
    }

    public SettingsViewModel(LoupedeckConfig config, IObsController obs)
    {
        Config = config;
        SaveObsCommand = new RelayCommand(SaveObs);
        TestConnectionCommand = new RelayCommand(TestConnection);
        SelectImageButtonCommand = new AsyncRelayCommand(SelectImageButton_Click);
        RemoveWallpaperCommand = new RelayCommand(RemoveWallpaper);

        NavigateCommand = new RelayCommand<SettingsView>(Navigate);
        CurrentView = SettingsView.General;

        ConnectionTestVisible = true;

        ObsConfig = ObsConfig.LoadConfig();
        _obs = obs;

        _obs.Connected += ObsConnected;
        _obs.Disconnected += ObsDisconnected;

        // Build the available pages list
        AvailablePages = new ObservableCollection<string>();
        for (int i = 0; i < Math.Max(Config.TouchButtonPages.Count, Config.RotaryButtonPages.Count); i++)
        {
            AvailablePages.Add($"Page {i}");
        }
    }

    private void ObsConnected(object sender, EventArgs e)
    {
        ConnectionResult = "Successfully connected";
        ConnectionTestVisible = true;
        TextColor = Colors.Green;
    }

    private void ObsDisconnected(object sender, ObsDisconnectionInfo e)
    {
        ConnectionResult = $"Error: {e.WebsocketDisconnectionInfo.CloseStatusDescription}";
        ConnectionTestVisible = true;
        TextColor = Colors.Red;
    }

    private bool _connectionTestVisible;

    public bool ConnectionTestVisible
    {
        get => _connectionTestVisible;
        set => SetProperty(ref _connectionTestVisible, value);
    }

    private Color _textColor = Colors.Blue;

    public Color TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    private string _connectionResult;

    public string ConnectionResult
    {
        get => _connectionResult;
        set => SetProperty(ref _connectionResult, value);
    }

    private void SaveObs()
    {
        ObsConfig.SaveConfig();
    }

    private void TestConnection()
    {
        _obs.Connect(ObsConfig.Ip, ObsConfig.Port, ObsConfig.Password);
    }

    private async Task SelectImageButton_Click()
    {
        var result = await FileDialogHelper.OpenFileDialog();

        if (result == null || !File.Exists(result)) return;

        _wallpaperBitmap = SKBitmap.Decode(result);

        ApplyScaling();
        OnPropertyChanged(nameof(CurrentWallpaper));
    }

    private void RemoveWallpaper()
    {
        if (CurrentTouchButtonPageForWallpaper != null)
        {
            CurrentTouchButtonPageForWallpaper.Wallpaper = null;
            _wallpaperBitmap = null;
            OnPropertyChanged(nameof(CurrentWallpaper));
        }
    }

    private void Navigate(SettingsView settingsPage)
    {
        CurrentView = settingsPage;
    }
}