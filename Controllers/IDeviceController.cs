using LoupixDeck.Models;
using LoupixDeck.Services;

namespace LoupixDeck.Controllers;

/// <summary>
/// Common interface for all device controllers
/// </summary>
public interface IDeviceController
{
    IPageManager PageManager { get; }
    LoupedeckConfig Config { get; }
    Task Initialize(string port = null, int baudrate = 0);
    void SaveConfig();
    Task ClearDeviceState();
    Task RestoreDeviceState();
}

