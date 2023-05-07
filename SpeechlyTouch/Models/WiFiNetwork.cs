
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using Windows.Devices.WiFi;

namespace SpeechlyTouch.Models
{
    public class WiFiNetwork : ObservableObject, ICloneable
    {
        private string _ssid;
        public string Ssid
        {
            get { return _ssid; }
            set { SetProperty(ref _ssid, value); }
        }

        private string _bssid;
        public string Bssid
        {
            get { return _bssid; }
            set { SetProperty(ref _bssid, value); }
        }

        private string _channelCenterFrequency;
        public string ChannelCenterFrequency
        {
            get { return _channelCenterFrequency; }
            set { SetProperty(ref _channelCenterFrequency, value); }
        }

        private string _connectivityLevel;
        public string ConnectivityLevel
        {
            get { return _connectivityLevel; }
            set { SetProperty(ref _connectivityLevel, value); }
        }

        private string _securitySettings;
        public string SecuritySettings
        {
            get { return _securitySettings; }
            set { SetProperty(ref _securitySettings, value); }
        }

        private WiFiAvailableNetwork _availableNetwork;
        public WiFiAvailableNetwork AvailableNetwork
        {
            get { return _availableNetwork; }
            set { SetProperty(ref _availableNetwork, value); }
        }

        private Windows.Networking.Connectivity.NetworkAuthenticationType _networkAuthenticationType;
        public Windows.Networking.Connectivity.NetworkAuthenticationType NetworkAuthenticationType
        {
            get { return _networkAuthenticationType; }
            set { SetProperty(ref _networkAuthenticationType, value); }
        }

        private Windows.Networking.Connectivity.NetworkEncryptionType _networkEncryptionType;
        public Windows.Networking.Connectivity.NetworkEncryptionType NetworkEncryptionType
        {
            get { return _networkEncryptionType; }
            set { SetProperty(ref _networkEncryptionType, value); }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
