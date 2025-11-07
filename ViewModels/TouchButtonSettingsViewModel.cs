using System.Collections.ObjectModel;
using System.Windows.Input;
using LoupixDeck.LoupedeckDevice;
using LoupixDeck.Models;
using LoupixDeck.Models.Converter;
using LoupixDeck.Services;
using LoupixDeck.Utils;
using LoupixDeck.ViewModels.Base;
using SkiaSharp;

namespace LoupixDeck.ViewModels;

public class TouchButtonSettingsViewModel : DialogViewModelBase<TouchButton, DialogResult>, IAsyncInitViewModel

{
    public override void Initialize(TouchButton parameter)
    {
        ButtonData = parameter;
        
        // Set the selected vibration pattern based on ButtonData
        if (ButtonData != null)
        {
            _selectedVibrationPattern = VibrationPatterns?.FirstOrDefault(p => p.Value == ButtonData.VibrationPattern);
        }
    }

    private readonly IObsController _obs;
    private readonly ElgatoDevices _elgatoDevices;
    private readonly ICoolerControlApiController _coolercontrol;
    private readonly ISysCommandService _sysCommandService;
    private readonly ICommandBuilder _commandBuilder;

    public ICommand SelectImageButtonCommand { get; }
    public ICommand RemoveImageButtonCommand { get; }
    public TouchButton ButtonData { get; set; }

    public ObservableCollection<MenuEntry> SystemCommandMenus { get; set; }
    public MenuEntry CurrentMenuEntry { get; set; }

    public ObservableCollection<VibrationPatternItem> VibrationPatterns { get; set; }
    
    private VibrationPatternItem _selectedVibrationPattern;
    public VibrationPatternItem SelectedVibrationPattern
    {
        get => _selectedVibrationPattern;
        set
        {
            if (_selectedVibrationPattern == value) return;
            _selectedVibrationPattern = value;
            if (ButtonData != null && value != null)
            {
                ButtonData.VibrationPattern = value.Value;
            }
            OnPropertyChanged(nameof(SelectedVibrationPattern));
        }
    }

    private MenuEntry _elgatoKeyLightMenu;

    public TouchButtonSettingsViewModel(IObsController obs,
        ElgatoDevices elgatoDevices,
        ICoolerControlApiController coolercontrol,
        ISysCommandService sysCommandService,
        ICommandBuilder commandBuilder)
    {
        _obs = obs;
        _elgatoDevices = elgatoDevices;
        _coolercontrol = coolercontrol;
        _sysCommandService = sysCommandService;
        _commandBuilder = commandBuilder;

        SelectImageButtonCommand = new AsyncRelayCommand(SelectImageButton_Click);
        RemoveImageButtonCommand = new RelayCommand(RemoveImageButton_Click);

        SystemCommandMenus = new ObservableCollection<MenuEntry>();
        
        // Initialize vibration patterns
        VibrationPatterns = new ObservableCollection<VibrationPatternItem>
        {
            new VibrationPatternItem("Short", Constants.VibrationPattern.Short),
            new VibrationPatternItem("Short Low", Constants.VibrationPattern.ShortLow),
            new VibrationPatternItem("Short Lower", Constants.VibrationPattern.ShortLower),
            new VibrationPatternItem("Medium", Constants.VibrationPattern.Medium),
            new VibrationPatternItem("Long", Constants.VibrationPattern.Long),
            new VibrationPatternItem("Very Long", Constants.VibrationPattern.VeryLong),
            new VibrationPatternItem("Low", Constants.VibrationPattern.Low),
            new VibrationPatternItem("Lower", Constants.VibrationPattern.Lower),
            new VibrationPatternItem("Lowest", Constants.VibrationPattern.Lowest),
            new VibrationPatternItem("Descend Slow", Constants.VibrationPattern.DescendSlow),
            new VibrationPatternItem("Descend Med", Constants.VibrationPattern.DescendMed),
            new VibrationPatternItem("Descend Fast", Constants.VibrationPattern.DescendFast),
            new VibrationPatternItem("Ascend Slow", Constants.VibrationPattern.AscendSlow),
            new VibrationPatternItem("Ascend Med", Constants.VibrationPattern.AscendMed),
            new VibrationPatternItem("Ascend Fast", Constants.VibrationPattern.AscendFast),
            new VibrationPatternItem("Rev Slowest", Constants.VibrationPattern.RevSlowest),
            new VibrationPatternItem("Rev Slow", Constants.VibrationPattern.RevSlow),
            new VibrationPatternItem("Rev Med", Constants.VibrationPattern.RevMed),
            new VibrationPatternItem("Rev Fast", Constants.VibrationPattern.RevFast),
            new VibrationPatternItem("Rev Faster", Constants.VibrationPattern.RevFaster),
            new VibrationPatternItem("Rev Fastest", Constants.VibrationPattern.RevFastest),
            new VibrationPatternItem("Rise Fall", Constants.VibrationPattern.RiseFall),
            new VibrationPatternItem("Buzz", Constants.VibrationPattern.Buzz),
            new VibrationPatternItem("Rumble 1", Constants.VibrationPattern.Rumble1),
            new VibrationPatternItem("Rumble 2", Constants.VibrationPattern.Rumble2),
            new VibrationPatternItem("Rumble 3", Constants.VibrationPattern.Rumble3),
            new VibrationPatternItem("Rumble 4", Constants.VibrationPattern.Rumble4),
            new VibrationPatternItem("Rumble 5", Constants.VibrationPattern.Rumble5)
        };
    }

    public Task InitializeAsync()
    {
        return CreateSystemMenu();
    }

    private async Task CreateSystemMenu()
    {
        CreatePagesMenu();
        CreateMacroMenu();
        await CreateObsMenu();
        CreateElgatoMenu();
        await CreateCoolerControlMenu();
    }

    private void CreatePagesMenu()
    {
        // Get Only Pages Commands
        var commands = _sysCommandService.GetCommandInfos().Where(ci => ci.Group == "Pages");

        var groupMenu = new MenuEntry("Pages", string.Empty);

        foreach (var command in commands)
        {
            groupMenu.Children.Add(new MenuEntry(command.DisplayName, command.CommandName));
        }

        SystemCommandMenus.Add(groupMenu);
    }

    private async Task CreateObsMenu()
    {
        var commands = _sysCommandService.GetCommandInfos().Where(ci => ci.Group == "OBS");

        var groupMenu = new MenuEntry("OBS", string.Empty);

        foreach (var command in commands)
        {
            if (command.CommandName == "System.ObsSetScene")
                continue;

            groupMenu.Children.Add(new MenuEntry(command.DisplayName, command.CommandName));
        }

        var scenesMenu = new MenuEntry("Scenes", string.Empty);
        var scenes = await _obs.GetScenes();

        foreach (var scene in scenes)
        {
            scenesMenu.Children.Add(new MenuEntry(scene.Name, $"System.ObsSetScene"));
        }

        groupMenu.Children.Add(scenesMenu);

        SystemCommandMenus.Add(groupMenu);
    }

    private void CreateMacroMenu()
    {
        var commands = _sysCommandService.GetCommandInfos()
            .Where(ci => ci.Group == "Macros")
            .OrderBy(ci => ci.Group);

        var groupMenu = new MenuEntry("Macros", string.Empty);

        foreach (var command in commands)
        {
            groupMenu.Children.Add(new MenuEntry(command.DisplayName, command.CommandName));
        }

        SystemCommandMenus.Add(groupMenu);
    }

    private void CreateElgatoMenu()
    {
        _elgatoKeyLightMenu = new MenuEntry("Elgato Keylights", string.Empty);

        foreach (var keyLight in _elgatoDevices.KeyLights)
        {
            AddKeyLightMenuEntry(keyLight);
        }

        _elgatoDevices.KeyLightAdded += KeyLightAdded;

        SystemCommandMenus.Add(_elgatoKeyLightMenu);
    }

    private async Task CreateCoolerControlMenu()
    {
        try
        {
            var commands = _sysCommandService.GetCommandInfos().Where(ci => ci.Group == "Cooler Control");

            var groupMenu = new MenuEntry("Cooler Control", string.Empty);

            foreach (var command in commands)
            {
                if (command.CommandName == "System.CoolerControlSetMode")
                    continue;

                groupMenu.Children.Add(new MenuEntry(command.DisplayName, command.CommandName));
            }

            var modesMenu = new MenuEntry("Modes", string.Empty);
            var modes = await _coolercontrol.GetModes();

            foreach (var mode in modes)
            {
                modesMenu.Children.Add(
                    new MenuEntry(mode["name"]?.ToString(),
                        $"System.CoolerControlSetMode",
                        null,
                        new Dictionary<string, string>() { { "UID", mode["uid"]?.ToString() ?? string.Empty } })
                );
            }

            groupMenu.Children.Add(modesMenu);

            SystemCommandMenus.Add(groupMenu);
        }
        catch (Exception ex)
        {
            // CoolerControl not available - skip adding the menu
            Console.WriteLine($"CoolerControl not available: {ex.Message}");
        }
    }

    private void KeyLightAdded(object sender, KeyLight e)
    {
        AddKeyLightMenuEntry(e);
    }

    private void AddKeyLightMenuEntry(KeyLight keyLight)
    {
        var checkKeyLight = _elgatoKeyLightMenu.Children.FirstOrDefault(kl => kl.Name == keyLight.DisplayName);

        if (checkKeyLight != null)
            return;

        var keyLightGroup = new MenuEntry(keyLight.DisplayName, null);

        var commands = _sysCommandService.GetCommandInfos().Where(ci => ci.Group == "Elgato Keylights");

        foreach (var command in commands)
        {
            keyLightGroup.Children.Add(new MenuEntry(command.DisplayName, command.CommandName, keyLight.DisplayName));
        }

        _elgatoKeyLightMenu.Children.Add(keyLightGroup);
    }

    private async Task SelectImageButton_Click()
    {
        var result = await FileDialogHelper.OpenFileDialog();

        if (result == null || !File.Exists(result)) return;

        ButtonData.Image = SKBitmap.Decode(result);
    }

    private void RemoveImageButton_Click()
    {
        ButtonData.Image = null;
    }

    public void InsertCommand(MenuEntry menuEntry)
    {
        var formattedCommand = _commandBuilder.CreateCommandFromMenuEntry(menuEntry);

        ButtonData.Command += formattedCommand;
    }
}