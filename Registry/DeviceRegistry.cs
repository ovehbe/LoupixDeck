using LoupixDeck.LoupedeckDevice.Device;

namespace LoupixDeck.Registry;

public static class DeviceRegistry
{
    public record DeviceInfo(string Name, string VendorId, string ProductId, Type DeviceType);

    public static readonly List<DeviceInfo> SupportedDevices =
    [
        new("Loupedeck Live S", "2ec2", "0006", typeof(LoupedeckLiveSDevice)),
        new("Razer Stream Controller", "1532", "0d06", typeof(RazerStreamControllerDevice))
        //new DeviceInfo("Loupedeck CT", "2ec2", "0007", typeof(LoupedeckCTDevice))
    ];

    public static DeviceInfo GetDeviceByVidPid(string vid, string pid)
    {
        return SupportedDevices.FirstOrDefault(d => d.VendorId.ToLower() == vid.ToLower() && d.ProductId.ToLower() == pid.ToLower());
    }
}