
using SpeechlyTouch.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

namespace SpeechlyTouch.Services.WiFiNetworks
{
    public interface IWiFiService
    {
        WiFiAdapter wiFiAdapter { get; }
        Task<List<WiFiNetwork>> ScanNetworks();
        Task<bool> IsWifiOn();
        Task<ConnectionProfile> GetConnectedProfile();
    }
}
