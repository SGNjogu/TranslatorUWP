using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.Connectivity;
using SpeechlyTouch.Services.WiFiNetworks;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using ColorConverter = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

namespace SpeechlyTouch.ViewModels
{
    public class InitialSetupNetworkViewModel : ObservableObject
    {
        private string _connectErrorMessage;
        public string ConnectErrorMessage
        {
            get { return _connectErrorMessage; }
            set { SetProperty(ref _connectErrorMessage, value); }
        }

        private string _wiFiPassword;
        public string WiFiPassword
        {
            get { return _wiFiPassword; }
            set { SetProperty(ref _wiFiPassword, value); }
        }

        private Visibility _errorMessageVisibility;
        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set { SetProperty(ref _errorMessageVisibility, value); }
        }

        private Visibility _networkListLoadingVisibility = Visibility.Collapsed;
        public Visibility NetworkListLoadingVisibility
        {
            get { return _networkListLoadingVisibility; }
            set { SetProperty(ref _networkListLoadingVisibility, value); }
        }

        private Visibility _networkUnavailableMessageVisibility = Visibility.Collapsed;
        public Visibility NetworkUnavailableMessageVisibility
        {
            get { return _networkUnavailableMessageVisibility; }
            set { SetProperty(ref _networkUnavailableMessageVisibility, value); }
        }

        private Visibility _isVisiblePopupBackground = Visibility.Collapsed;
        public Visibility IsVisiblePopupBackground
        {
            get { return _isVisiblePopupBackground; }
            set { SetProperty(ref _isVisiblePopupBackground, value); }
        }

        private bool _isPasswordPopupOpen = false;
        public bool IsPasswordPopupOpen
        {
            get { return _isPasswordPopupOpen; }
            set
            {
                SetProperty(ref _isPasswordPopupOpen, value);
                if (IsPasswordPopupOpen)
                {
                    IsVisiblePopupBackground = Visibility.Visible;
                }
                else
                {
                    IsVisiblePopupBackground = Visibility.Collapsed;
                }
            }
        }

        private bool _isConnectBtnEnabled = true;
        public bool IsConnectBtnEnabled
        {
            get { return _isConnectBtnEnabled; }
            set { SetProperty(ref _isConnectBtnEnabled, value); }
        }

        private ObservableCollection<WiFiNetwork> _wiFiNetworks;
        public ObservableCollection<WiFiNetwork> WiFiNetworks
        {
            get { return _wiFiNetworks; }
            set { SetProperty(ref _wiFiNetworks, value); }
        }

        private ObservableCollection<InitialNetworkSetupItem> _initialSetupWiFiNetworks;
        public ObservableCollection<InitialNetworkSetupItem> InitialSetupWiFiNetworks
        {
            get { return _initialSetupWiFiNetworks; }
            set { SetProperty(ref _initialSetupWiFiNetworks, value); }
        }

        private InitialNetworkSetupItem _selectedWiFiNetwork;
        public InitialNetworkSetupItem SelectedWiFiNetwork
        {
            get { return _selectedWiFiNetwork; }
            set
            {
                SetProperty(ref _selectedWiFiNetwork, value);
                if (SelectedWiFiNetwork != null && SelectedWiFiNetwork.WiFiNetwork.ConnectivityLevel != "InternetAccess")
                    HandleNetworkSelectionChange();
            }
        }

        private ConnectionProfile _connectedNetworkProfile;
        public ConnectionProfile ConnectedNetworkprofile
        {
            get { return _connectedNetworkProfile; }
            set { SetProperty(ref _connectedNetworkProfile, value); }
        }

        private readonly IWiFiService _wiFiNetworkService;
        private readonly IConnectivityService _connectivityService;
        private readonly ICrashlytics _crashlytics;

        private int WifiConnectionChangeCounter { get; set; } = 0;
        private ResourceLoader _resourceLoader;

        public InitialSetupNetworkViewModel(IWiFiService wiFiNetworkService, IConnectivityService connectivityService, ICrashlytics crashlytics)
        {
            _wiFiNetworkService = wiFiNetworkService;
            _connectivityService = connectivityService;
            _crashlytics = crashlytics;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            IsVisiblePopupBackground = Visibility.Collapsed;
            NetworkListLoadingVisibility = Visibility.Collapsed;
            NetworkUnavailableMessageVisibility = Visibility.Collapsed;
            _ = InitializeWiFiNetworks();
        }

        public async Task InitializeWiFiNetworks()
        {
            try
            {
                WiFiNetworks = new ObservableCollection<WiFiNetwork>();
                InitialSetupWiFiNetworks = new ObservableCollection<InitialNetworkSetupItem>();

                await ScanAvailableNetworks();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task ScanAvailableNetworks()
        {
            try
            {
                NetworkListLoadingVisibility = Visibility.Visible;

                if (WiFiNetworks.Count != 0) WiFiNetworks.Clear();
                var networks = await _wiFiNetworkService.ScanNetworks();
                ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();
                if (networks.Count != 0)
                {
                    foreach (var network in networks)
                    {
                        WiFiNetworks.Add(network);
                        InitialSetupWiFiNetworks.Add(new InitialNetworkSetupItem
                        {
                            Foreground = new SolidColorBrush(ColorConverter.ToColor("#B5BDDE")),
                            WiFiNetwork = network,
                            IsVisibleLockIcon = network.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                            network.NetworkEncryptionType == NetworkEncryptionType.None ?
                            Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible
                        });
                    }
                    SelectedWiFiNetwork = InitialSetupWiFiNetworks.ElementAt(0);
                    if (ConnectedNetworkprofile != null)
                    {
                        SelectedWiFiNetwork = InitialSetupWiFiNetworks.FirstOrDefault(n => n.WiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName);
                        SelectedWiFiNetwork.Foreground = new SolidColorBrush(ColorConverter.ToColor("#0A2B93"));
                    }
                }
                else
                {
                    SelectedWiFiNetwork = null;
                }

                NetworkListLoadingVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            NetworkListLoadingVisibility = Visibility.Collapsed;
        }

        private async void HandleNetworkSelectionChange()
        {
            try
            {
                var newSelectedNetwork = SelectedWiFiNetwork as InitialNetworkSetupItem;
                if (newSelectedNetwork == null && WifiConnectionChangeCounter == 0)
                {
                    return;
                }
                ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();

                if (ConnectedNetworkprofile != null && WifiConnectionChangeCounter == 0)
                {
                    SelectedWiFiNetwork = InitialSetupWiFiNetworks.FirstOrDefault(n => n.WiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName && WifiConnectionChangeCounter < 2); //Assign selectedNetwork on first run
                }

                if (ConnectedNetworkprofile != null && SelectedWiFiNetwork.WiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName)
                {
                    WifiConnectionChangeCounter = WifiConnectionChangeCounter + 1;
                    return;
                }
                else
                {
                    if (newSelectedNetwork.WiFiNetwork.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                        newSelectedNetwork.WiFiNetwork.NetworkEncryptionType == NetworkEncryptionType.None)
                    {
                        await Connect();
                    }
                    else
                    {
                        if (ConnectedNetworkprofile != null && SelectedWiFiNetwork.WiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName)
                        {
                            IsPasswordPopupOpen = false;
                        }
                        else
                        {
                            if (!IsPasswordPopupOpen)
                            {
                                IsPasswordPopupOpen = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task Connect()
        {
            try
            {
                IsConnectBtnEnabled = false;
                var selectedNetwork = SelectedWiFiNetwork;
                if (selectedNetwork == null || _wiFiNetworkService.wiFiAdapter == null)
                {
                    await HandleNetworkMessage(_resourceLoader.GetString("DevicesPage_NetworkError"));
                    IsConnectBtnEnabled = true;
                    return;
                }
                WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Automatic;

                WiFiConnectionResult result;
                if (selectedNetwork.WiFiNetwork.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                    selectedNetwork.WiFiNetwork.NetworkEncryptionType == NetworkEncryptionType.None)
                {
                    result = await _wiFiNetworkService.wiFiAdapter.ConnectAsync(selectedNetwork.WiFiNetwork.AvailableNetwork, reconnectionKind);
                }
                else
                {
                    var credential = new Windows.Security.Credentials.PasswordCredential();

                    if (!string.IsNullOrEmpty(WiFiPassword))
                    {
                        credential.Password = WiFiPassword;
                    }

                    result = await _wiFiNetworkService.wiFiAdapter.ConnectAsync(selectedNetwork.WiFiNetwork.AvailableNetwork, reconnectionKind, credential);
                }

                if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                {
                    await HandleNetworkMessage($"{_resourceLoader.GetString("DevicesPage_Status_Connected")}");
                    WiFiPassword = "";
                    ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();
                    IsPasswordPopupOpen = false;
                }
                else
                {
                    WiFiPassword = "";
                    await HandleNetworkMessage($"{_resourceLoader.GetString("NetworkConnectionError")} {result.ConnectionStatus}");
                    ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();
                    if (ConnectedNetworkprofile != null)
                    {
                        SelectedWiFiNetwork = InitialSetupWiFiNetworks.FirstOrDefault(n => n.WiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName);
                    }
                    else
                    {
                        await HandleNetworkMessage($"{_resourceLoader.GetString("NetworkConnectionError")} {result.ConnectionStatus}");
                    }
                }
                IsConnectBtnEnabled = true;
                if (IsPasswordPopupOpen)
                {
                    IsPasswordPopupOpen = false;
                    WiFiPassword = "";
                }

                ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task HandleNetworkMessage(string message)
        {
            ConnectErrorMessage = message;
            ErrorMessageVisibility = Visibility.Visible;
            await Task.Delay(3000);
            ErrorMessageVisibility = Visibility.Collapsed;
            ConnectErrorMessage = "";
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() =>
                {
                    if (ConnectedNetworkprofile != null)
                    {
                        SelectedWiFiNetwork = InitialSetupWiFiNetworks.FirstOrDefault(n => n.WiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName);
                    }
                    IsPasswordPopupOpen = false;
                    WiFiPassword = "";
                    IsConnectBtnEnabled = true;
                }));
            }
        }

        private RelayCommand _connectCommand = null;
        public RelayCommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommand(async () =>
                {
                    await Connect();
                }));
            }
        }

        private RelayCommand _moveToDevicesSetupCommand = null;
        public RelayCommand MoveToDevicesSetupCommand
        {
            get
            {
                return _moveToDevicesSetupCommand ?? (_moveToDevicesSetupCommand = new RelayCommand(() =>
                {
                    if (_connectivityService.IsConnectionAvailable())
                    {
                        StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadDevicesInitialSetup = true });
                        NetworkUnavailableMessageVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        NetworkUnavailableMessageVisibility = Visibility.Visible;
                    }
                }));
            }
        }
    }
}
