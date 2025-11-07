using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace LoupixDeck.Models;

public class RotaryButtonPage : INotifyPropertyChanged
{
    public RotaryButtonPage(int pageSize)
    {
        RotaryButtons = new ObservableCollection<RotaryButton>();

        for (var i = 0; i < pageSize; i++)
        {
            var newButton = new RotaryButton(i, string.Empty, string.Empty);
            RotaryButtons.Add(newButton);
        }
    }

    public string PageName => $"Rotary Page: {Page}";

    private int _page;
    private bool _selected;

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

    public ObservableCollection<RotaryButton> RotaryButtons { get; set; }

    // Global Commands for Simple Buttons on this page
    private bool _simpleButtonPrefixEnabled;
    public bool SimpleButtonPrefixEnabled
    {
        get => _simpleButtonPrefixEnabled;
        set
        {
            if (value == _simpleButtonPrefixEnabled) return;
            _simpleButtonPrefixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _simpleButtonPrefixCommand = string.Empty;
    public string SimpleButtonPrefixCommand
    {
        get => _simpleButtonPrefixCommand;
        set
        {
            if (value == _simpleButtonPrefixCommand) return;
            _simpleButtonPrefixCommand = value;
            OnPropertyChanged();
        }
    }

    private bool _simpleButtonSuffixEnabled;
    public bool SimpleButtonSuffixEnabled
    {
        get => _simpleButtonSuffixEnabled;
        set
        {
            if (value == _simpleButtonSuffixEnabled) return;
            _simpleButtonSuffixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _simpleButtonSuffixCommand = string.Empty;
    public string SimpleButtonSuffixCommand
    {
        get => _simpleButtonSuffixCommand;
        set
        {
            if (value == _simpleButtonSuffixCommand) return;
            _simpleButtonSuffixCommand = value;
            OnPropertyChanged();
        }
    }

    // Global Commands for Knobs (Left) on this page
    private bool _knobLeftPrefixEnabled;
    public bool KnobLeftPrefixEnabled
    {
        get => _knobLeftPrefixEnabled;
        set
        {
            if (value == _knobLeftPrefixEnabled) return;
            _knobLeftPrefixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _knobLeftPrefixCommand = string.Empty;
    public string KnobLeftPrefixCommand
    {
        get => _knobLeftPrefixCommand;
        set
        {
            if (value == _knobLeftPrefixCommand) return;
            _knobLeftPrefixCommand = value;
            OnPropertyChanged();
        }
    }

    private bool _knobLeftSuffixEnabled;
    public bool KnobLeftSuffixEnabled
    {
        get => _knobLeftSuffixEnabled;
        set
        {
            if (value == _knobLeftSuffixEnabled) return;
            _knobLeftSuffixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _knobLeftSuffixCommand = string.Empty;
    public string KnobLeftSuffixCommand
    {
        get => _knobLeftSuffixCommand;
        set
        {
            if (value == _knobLeftSuffixCommand) return;
            _knobLeftSuffixCommand = value;
            OnPropertyChanged();
        }
    }

    // Global Commands for Knobs (Right) on this page
    private bool _knobRightPrefixEnabled;
    public bool KnobRightPrefixEnabled
    {
        get => _knobRightPrefixEnabled;
        set
        {
            if (value == _knobRightPrefixEnabled) return;
            _knobRightPrefixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _knobRightPrefixCommand = string.Empty;
    public string KnobRightPrefixCommand
    {
        get => _knobRightPrefixCommand;
        set
        {
            if (value == _knobRightPrefixCommand) return;
            _knobRightPrefixCommand = value;
            OnPropertyChanged();
        }
    }

    private bool _knobRightSuffixEnabled;
    public bool KnobRightSuffixEnabled
    {
        get => _knobRightSuffixEnabled;
        set
        {
            if (value == _knobRightSuffixEnabled) return;
            _knobRightSuffixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _knobRightSuffixCommand = string.Empty;
    public string KnobRightSuffixCommand
    {
        get => _knobRightSuffixCommand;
        set
        {
            if (value == _knobRightSuffixCommand) return;
            _knobRightSuffixCommand = value;
            OnPropertyChanged();
        }
    }

    // Global Commands for Knobs (Press) on this page
    private bool _knobPressPrefixEnabled;
    public bool KnobPressPrefixEnabled
    {
        get => _knobPressPrefixEnabled;
        set
        {
            if (value == _knobPressPrefixEnabled) return;
            _knobPressPrefixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _knobPressPrefixCommand = string.Empty;
    public string KnobPressPrefixCommand
    {
        get => _knobPressPrefixCommand;
        set
        {
            if (value == _knobPressPrefixCommand) return;
            _knobPressPrefixCommand = value;
            OnPropertyChanged();
        }
    }

    private bool _knobPressSuffixEnabled;
    public bool KnobPressSuffixEnabled
    {
        get => _knobPressSuffixEnabled;
        set
        {
            if (value == _knobPressSuffixEnabled) return;
            _knobPressSuffixEnabled = value;
            OnPropertyChanged();
        }
    }

    private string _knobPressSuffixCommand = string.Empty;
    public string KnobPressSuffixCommand
    {
        get => _knobPressSuffixCommand;
        set
        {
            if (value == _knobPressSuffixCommand) return;
            _knobPressSuffixCommand = value;
            OnPropertyChanged();
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}