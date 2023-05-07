
using SpeechlyTouch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

namespace SpeechlyTouch.Services.WiFiNetworks
{
    public class WiFiService : IWiFiService
    {
        public WiFiAdapter wiFiAdapter { get; private set; }

        private async Task InitializeFirstAdapter()
        {
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                //Access granted on the package manager file, will not get thrown.
                throw new Exception("Wifi Access not allowed.");
            }
            else
            {
                var wifiAdapterResults =
                  await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (wifiAdapterResults.Count >= 1)
                {
                    wiFiAdapter = await WiFiAdapter.FromIdAsync(wifiAdapterResults[0].Id);
                }
                else
                {
                    Debug.WriteLine("WiFi Adapter not found.");
                }
            }
        }

        public async Task<List<WiFiNetwork>> ScanNetworks()
        {
            List<WiFiNetwork> networksAvailable = new List<WiFiNetwork>();

            if (wiFiAdapter == null)
                await InitializeFirstAdapter();

            await wiFiAdapter.ScanAsync();
            var networksList = wiFiAdapter.NetworkReport.AvailableNetworks;
            foreach (var network in networksList)
            {
                if (!networksAvailable.Exists(n => n.Ssid == network.Ssid))
                {
                    networksAvailable.Add(new WiFiNetwork()
                    {
                        Ssid = network.Ssid,
                        Bssid = network.Bssid,
                        ChannelCenterFrequency = string.Format("{0}kHz", network.ChannelCenterFrequencyInKilohertz),
                        ConnectivityLevel = await UpdateConnectivityLevel(network),
                        SecuritySettings = string.Format("Authentication: {0}; Encryption: {1}", network.SecuritySettings.NetworkAuthenticationType, network.SecuritySettings.NetworkEncryptionType),
                        NetworkAuthenticationType = network.SecuritySettings.NetworkAuthenticationType,
                        NetworkEncryptionType = network.SecuritySettings.NetworkEncryptionType,
                        AvailableNetwork = network
                    });
                }
            }

            return networksAvailable;
        }

        private async Task<string> UpdateConnectivityLevel(WiFiAvailableNetwork network)
        {
            string connectivityLevel = "Not Connected";
            string connectedSsid = null;

            var connectedProfile = await wiFiAdapter.NetworkAdapter.GetConnectedProfileAsync();
            if (connectedProfile != null &&
                connectedProfile.IsWlanConnectionProfile &&
                connectedProfile.WlanConnectionProfileDetails != null)
            {
                connectedSsid = connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid();
            }

            if (!string.IsNullOrEmpty(connectedSsid))
            {
                if (connectedSsid.Equals(network.Ssid))
                {
                    connectivityLevel = connectedProfile.GetNetworkConnectivityLevel().ToString();
                }
            }

            return connectivityLevel;
        }

        public async Task<bool> IsWifiOn()
        {
            await Radio.RequestAccessAsync();

            var radios = await Radio.GetRadiosAsync();
            foreach (var radio in radios)
            {
                if (radio.Kind == RadioKind.WiFi)
                {
                    return radio.State == RadioState.On;
                }
            }
            return false;
        }

        public async Task<ConnectionProfile> GetConnectedProfile()
        {
            return await wiFiAdapter.NetworkAdapter.GetConnectedProfileAsync();
        }

    }
}
