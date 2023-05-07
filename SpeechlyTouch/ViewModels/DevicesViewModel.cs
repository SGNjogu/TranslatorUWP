using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Bluetooth;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Services.WiFiNetworks;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace SpeechlyTouch.ViewModels
{
    public class DevicesViewModel : ObservableObject
    {

        #region Properties

        private Visibility _isVisibleNetworkMessageBox;
        public Visibility IsVisibleNetworkMessageBox
        {
            get { return _isVisibleNetworkMessageBox; }
            set { SetProperty(ref _isVisibleNetworkMessageBox, value); }
        }
        private ObservableCollection<DeviceInformation> _bluetoothDevicesList;
        public ObservableCollection<DeviceInformation> BluetoothDevicesList
        {
            get { return _bluetoothDevicesList; }
            set { SetProperty(ref _bluetoothDevicesList, value); }
        }

        private ObservableCollection<DeviceInformation> _newBluetoothDevicesList;
        public ObservableCollection<DeviceInformation> NewBluetoothDevicesList
        {
            get { return _newBluetoothDevicesList; }
            set { SetProperty(ref _newBluetoothDevicesList, value); }
        }

        private string _networkMessage;
        public string NetworkMessage
        {
            get { return _networkMessage; }
            set { SetProperty(ref _networkMessage, value); }
        }

        private bool _isBluetoothOn;
        public bool IsBluetoothOn
        {
            get { return _isBluetoothOn; }
            set { SetProperty(ref _isBluetoothOn, value); }
        }

        private string _wiFiPassword;
        public string WiFiPassword
        {
            get { return _wiFiPassword; }
            set { SetProperty(ref _wiFiPassword, value); }
        }

        private Brush _blueToothStatusColor;
        public Brush BlueToothStatusColor
        {
            get { return _blueToothStatusColor; }
            set { SetProperty(ref _blueToothStatusColor, value); }
        }

        private Brush _wifiStatusColor;
        public Brush WifiStatusColor
        {
            get { return _wifiStatusColor; }
            set { SetProperty(ref _wifiStatusColor, value); }
        }

        private string _wifiStatus;
        public string WifiStatus
        {
            get { return _wifiStatus; }
            set { SetProperty(ref _wifiStatus, value); }
        }

        private string _blueToothStatusText;
        public string BlueToothStatusText
        {
            get { return _blueToothStatusText; }
            set { SetProperty(ref _blueToothStatusText, value); }
        }

        private bool _isCheckedAutoUpdate;
        public bool IsCheckedAutoUpdate
        {
            get { return _isCheckedAutoUpdate; }
            set { SetProperty(ref _isCheckedAutoUpdate, value); }
        }

        private bool _isScanNetworksBtnEnabled;
        public bool IsScanNetworksBtnEnabled
        {
            get { return _isScanNetworksBtnEnabled; }
            set { SetProperty(ref _isScanNetworksBtnEnabled, value); }
        }

        private bool _isConnectBtnEnabled;
        public bool IsConnectBtnEnabled
        {
            get { return _isConnectBtnEnabled; }
            set { SetProperty(ref _isConnectBtnEnabled, value); }
        }

        private Visibility _isVisibleWiFiConnectPanel;
        public Visibility IsVisibleWiFiConnectPanel
        {
            get { return _isVisibleWiFiConnectPanel; }
            set { SetProperty(ref _isVisibleWiFiConnectPanel, value); }
        }

        private Visibility _isVisibleWiFiPasswordBox;
        public Visibility IsVisibleWiFiPasswordBox
        {
            get { return _isVisibleWiFiPasswordBox; }
            set { SetProperty(ref _isVisibleWiFiPasswordBox, value); }
        }

        private ObservableCollection<WiFiNetwork> _wiFiNetworks;
        public ObservableCollection<WiFiNetwork> WiFiNetworks
        {
            get { return _wiFiNetworks; }
            set { SetProperty(ref _wiFiNetworks, value); }
        }

        private WiFiNetwork _selectedWiFiNetwork;
        public WiFiNetwork SelectedWiFiNetwork
        {
            get { return _selectedWiFiNetwork; }
            set
            {
                SetProperty(ref _selectedWiFiNetwork, value);
                if (SelectedWiFiNetwork != null && SelectedWiFiNetwork.ConnectivityLevel != "InternetAccess")
                    HandleNetworkSelectionChange();
            }
        }

        private string _networkScanText;
        public string NetworkScanText
        {
            get { return _networkScanText; }
            set { SetProperty(ref _networkScanText, value); }
        }

        private string _networkConnectBtnText;
        public string NetworkConnectBtnText
        {
            get { return _networkConnectBtnText; }
            set { SetProperty(ref _networkConnectBtnText, value); }
        }

        private ConnectionProfile _connectedNetworkProfile;
        public ConnectionProfile ConnectedNetworkprofile
        {
            get { return _connectedNetworkProfile; }
            set { SetProperty(ref _connectedNetworkProfile, value); }
        }

        private DeviceInformation _selectedNewBluetoothDevice;
        public DeviceInformation SelectedNewBluetoothDevice
        {
            get { return _selectedNewBluetoothDevice; }
            set
            {
                SetProperty(ref _selectedNewBluetoothDevice, value);
                if (SelectedNewBluetoothDevice != null)
                    _ = PairDevice();
            }
        }

        private ResourceLoader _resourceLoader;
        private BluetoothPairingDialog _bluetoothPairingDialog;
        private bool IsScanning { get; set; } = false;
        private readonly IBluetoothService _bluetoothService;
        private readonly IWiFiService _wiFiNetworkService;
        private readonly ICrashlytics _crashlytics;
        private readonly IAppAnalytics _appAnalytics;
        private readonly ISettingsService _settings;
        private readonly IDialogService _dialogService;

        private int WifiConnectionChangeCounter { get; set; } = 0;

        #endregion

        #region Constructor

        public DevicesViewModel
            (
            IBluetoothService bluetoothService,
            IWiFiService wiFiNetworkService,
            ICrashlytics crashlytics,
            IAppAnalytics appAnalytics,
            ISettingsService settings,
            IDialogService dialogService
            )
        {
            _bluetoothService = bluetoothService;
            _wiFiNetworkService = wiFiNetworkService;
            _crashlytics = crashlytics;
            _appAnalytics = appAnalytics;
            _settings = settings;
            _dialogService = dialogService;
            _resourceLoader = ResourceLoader.GetForCurrentView();

            BluetoothDevicesList = new ObservableCollection<DeviceInformation>();
            NewBluetoothDevicesList = new ObservableCollection<DeviceInformation>();
            SelectedNewBluetoothDevice = null;
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            GetPairedDevices();
            _ = InitializeWiFiNetworks();
            HandleNetworkChange();
        }
        #endregion

        #region Methods

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                if (_bluetoothPairingDialog == null)
                    _bluetoothPairingDialog = new BluetoothPairingDialog();

                await HandleBluetoothStatus();
                await InitializeWiFiNetworks();
            }
        }

        private async void ShowScanBluetoothDialog()
        {
            var user = await _settings.GetUser();
            if (_bluetoothPairingDialog == null)
                _bluetoothPairingDialog = new BluetoothPairingDialog();

            ScanForBluetoothDevices();

            _appAnalytics.CaptureCustomEvent("Settings Changes",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Blutooth Pairing Changed" }
                    });
           await _dialogService.ShowDialog(_bluetoothPairingDialog);
        }

        private void ScanForBluetoothDevices()
        {
            if (!IsScanning)
            {
                _bluetoothService.BluetoothDeviceAddedEvent += OnBluetoothDeviceAdded;
                _bluetoothService.BluetoothDeviceRemovedEvent += OnBluetoothDeviceRemoved;
                _bluetoothService.BluetoothEnumerationCompletedEvent += OnBluetoothEnumerationCompleted;
                _bluetoothService.StartScan();
                IsScanning = true;
            }
        }

        private async void GetPairedDevices()
        {
            try
            {
                var isBluetoothSupported = await _bluetoothService.IsBluetoothSupported();

                if (isBluetoothSupported)
                {
                    _bluetoothService.BluetoothRadio.StateChanged += OnBluetoothRadioStateChanged;
                    IsBluetoothOn = _bluetoothService.IsBluetoothOn();
                    await HandleBluetoothStatus();
                }
                else
                {
                    IsBluetoothOn = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task GetPairedBluetoothDevicesAsync()
        {
            try
            {
                if (BluetoothDevicesList.Any())
                    BluetoothDevicesList.Clear();

                var devices = await _bluetoothService.GetPairedBluetoothDevices();

                if (devices.Any())
                {
                    foreach (var item in devices)
                    {
                        BluetoothDevicesList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void OnBluetoothRadioStateChanged(Windows.Devices.Radios.Radio sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (sender.State == Windows.Devices.Radios.RadioState.On)
                    IsBluetoothOn = true;
                else IsBluetoothOn = false;
                await HandleBluetoothStatus();
            });
        }

        private async Task HandleBluetoothStatus()
        {
            if (IsBluetoothOn)
            {
                BlueToothStatusColor = new SolidColorBrush(Colors.LimeGreen);
                BlueToothStatusText = _resourceLoader.GetString("DevicesPage_Status_On");
                await GetPairedBluetoothDevicesAsync();
            }
            else
            {
                BlueToothStatusColor = new SolidColorBrush(Colors.Red);
                BlueToothStatusText = _resourceLoader.GetString("DevicesPage_Status_Off");

                if (BluetoothDevicesList.Any())
                    BluetoothDevicesList.Clear();
            }
        }

        private void StopScanForBluetoothDevices()
        {
            _bluetoothService.BluetoothDeviceAddedEvent -= OnBluetoothDeviceAdded;
            _bluetoothService.BluetoothDeviceRemovedEvent -= OnBluetoothDeviceRemoved;
            _bluetoothService.StopScan();
            IsScanning = false;
        }

        private async void OnBluetoothDeviceRemoved(object sender, BluetoothDeviceRemovedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var removedDeviceId = args.DeviceInformationUpdate.Id;
                var removedDevice = NewBluetoothDevicesList.FirstOrDefault(s => s.Id == removedDeviceId);
                if (removedDevice != null)
                {
                    NewBluetoothDevicesList.Remove(removedDevice);
                }
            });
        }

        private async void OnBluetoothDeviceAdded(object sender, BluetoothDeviceAddedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!string.IsNullOrEmpty(args.DeviceInformation.Name) && args.DeviceInformation.Pairing.CanPair == true && args.DeviceInformation.Pairing.IsPaired == false)
                    if (!NewBluetoothDevicesList.Contains(args.DeviceInformation))
                        NewBluetoothDevicesList.Add(args.DeviceInformation);
            });
        }

        private async void OnBluetoothEnumerationCompleted(object sender, BluetoothEnumerationCompletedEventArgs args)
        {
            StopScanForBluetoothDevices();
            await Task.Delay(1000);
            ScanForBluetoothDevices();
        }

        private async Task PairDevice()
        {
            var paired = await _bluetoothService.Pair(SelectedNewBluetoothDevice);
            if (paired)
                BluetoothDevicesList.Add(SelectedNewBluetoothDevice);
            CloseDialog();
        }

        private void CloseDialog()
        {
            StopScanForBluetoothDevices();
            _dialogService.HideDialog();
        }

        public async Task InitializeWiFiNetworks()
        {
            try
            {
                NetworkConnectBtnText = _resourceLoader.GetString("DevicesPage_Status_Connect");
                NetworkScanText = _resourceLoader.GetString("DevicesPage_Scan"); ;
                WiFiNetworks = new ObservableCollection<WiFiNetwork>();
                IsConnectBtnEnabled = true;
                IsScanNetworksBtnEnabled = false;
                IsVisibleWiFiPasswordBox = Visibility.Collapsed;
                IsVisibleWiFiConnectPanel = Visibility.Collapsed;
                IsVisibleNetworkMessageBox = Visibility.Collapsed;
                IsCheckedAutoUpdate = false;
                await Task.Delay(1000);
                WifiStatus = _resourceLoader.GetString("DevicesPage_Status_On");
                await HandleWiFiStatus();
                await ScanAvailableNetworks();
                IsScanNetworksBtnEnabled = true;
            }
            catch (Exception ex)
            {
                NetworkScanText = _resourceLoader.GetString("DevicesPage_Scan");
                IsScanNetworksBtnEnabled = true;
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void HandleNetworkChange()
        {
            //_wiFiNetworkService.wiFiAdapter.AvailableNetworksChanged += WiFiAdapter_AvailableNetworksChanged;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        private async void WiFiAdapter_AvailableNetworksChanged(WiFiAdapter sender, object args)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await ScanAvailableNetworks();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await HandleWiFiStatus();
                    await ScanAvailableNetworks();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void HandleNetworkSelectionChange()
        {
            try
            {
                var newSelectedNetwork = SelectedWiFiNetwork as WiFiNetwork;
                if (WifiStatus == _resourceLoader.GetString("DevicesPage_Status_Off") || newSelectedNetwork == null)
                {
                    return;
                }
                ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();

                if (ConnectedNetworkprofile != null && WifiConnectionChangeCounter == 0)
                {
                    SelectedWiFiNetwork = WiFiNetworks.FirstOrDefault(n => n.Ssid == ConnectedNetworkprofile.ProfileName); //Assign selectedNetwork on first run
                }

                //Show/Hide the connection panel
                if (ConnectedNetworkprofile != null && SelectedWiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName && WifiConnectionChangeCounter < 2)
                {
                    IsVisibleWiFiConnectPanel = Visibility.Collapsed;
                    WifiConnectionChangeCounter = WifiConnectionChangeCounter + 1;
                    return;
                }
                else
                {
                    IsVisibleWiFiConnectPanel = Visibility.Visible;
                }

                // Only show the password box if needed
                if (newSelectedNetwork.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                    newSelectedNetwork.NetworkEncryptionType == NetworkEncryptionType.None)
                {
                    IsVisibleWiFiPasswordBox = Visibility.Collapsed;
                }
                else
                {
                    if (ConnectedNetworkprofile != null && SelectedWiFiNetwork.Ssid == ConnectedNetworkprofile.ProfileName)
                    {
                        IsVisibleWiFiPasswordBox = Visibility.Collapsed;
                    }
                    else
                    {
                        IsVisibleWiFiPasswordBox = Visibility.Visible;
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
                var user = await _settings.GetUser();
                NetworkConnectBtnText = _resourceLoader.GetString("DevicesPage_Status_Connecting");
                IsConnectBtnEnabled = false;
                var selectedNetwork = SelectedWiFiNetwork;
                if (selectedNetwork == null || _wiFiNetworkService.wiFiAdapter == null)
                {
                    await HandleNetworkMessage(_resourceLoader.GetString("DevicesPage_NetworkError"));
                    NetworkConnectBtnText = _resourceLoader.GetString("DevicesPage_Status_Connect");
                    IsConnectBtnEnabled = true;
                    return;
                }
                WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Manual;
                if (IsCheckedAutoUpdate == true)
                {
                    reconnectionKind = WiFiReconnectionKind.Automatic;
                }

                WiFiConnectionResult result;
                if (selectedNetwork.NetworkAuthenticationType == NetworkAuthenticationType.Open80211 &&
                    selectedNetwork.NetworkEncryptionType == NetworkEncryptionType.None)
                {
                    result = await _wiFiNetworkService.wiFiAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind);
                }
                else
                {
                    var credential = new Windows.Security.Credentials.PasswordCredential();

                    if (!string.IsNullOrEmpty(WiFiPassword))
                    {
                        credential.Password = WiFiPassword;
                    }

                    result = await _wiFiNetworkService.wiFiAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind, credential);
                }

                if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                {
                    NetworkConnectBtnText = _resourceLoader.GetString("DevicesPage_Status_Connect");
                    await HandleNetworkMessage(_resourceLoader.GetString("DevicesPage_Status_Connected"));
                    WiFiPassword = "";
                    IsCheckedAutoUpdate = false;
                    IsVisibleWiFiPasswordBox = Visibility.Collapsed;
                    IsVisibleWiFiConnectPanel = Visibility.Collapsed;
                    ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();
                }
                else
                {
                    NetworkConnectBtnText = _resourceLoader.GetString("DevicesPage_Status_Connect");
                    WiFiPassword = "";
                    IsCheckedAutoUpdate = false;
                    IsVisibleWiFiPasswordBox = Visibility.Collapsed;
                    IsVisibleWiFiConnectPanel = Visibility.Collapsed;
                    await HandleNetworkMessage($"Could not connect to {selectedNetwork.Ssid}. Error: {result.ConnectionStatus}");
                    ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();
                    if (ConnectedNetworkprofile != null)
                    {
                        SelectedWiFiNetwork = WiFiNetworks.FirstOrDefault(n => n.Ssid == ConnectedNetworkprofile.ProfileName);
                    }
                    else
                    {
                        await HandleNetworkMessage($"Could not connect to {selectedNetwork.Ssid}. Error: {result.ConnectionStatus}");
                    }
                }
                IsConnectBtnEnabled = true;
                ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile(); //Update ConnectedNetworkProfile
                _appAnalytics.CaptureCustomEvent("Settings Changes",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Network Changed" }
                    });
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
                if (NetworkScanText == _resourceLoader.GetString("DevicesPage_Scan"))
                {
                    await HandleWiFiStatus();
                    NetworkScanText = _resourceLoader.GetString("DevicesPage_Scanning");
                    if (WiFiNetworks.Count != 0) WiFiNetworks.Clear();
                    var networks = await _wiFiNetworkService.ScanNetworks();
                    ConnectedNetworkprofile = await _wiFiNetworkService.GetConnectedProfile();
                    if (networks.Count != 0)
                    {
                        foreach (var network in networks)
                        {
                            WiFiNetworks.Add(network);
                        }
                        SelectedWiFiNetwork = WiFiNetworks.ElementAt(0);
                        if (ConnectedNetworkprofile != null)
                        {
                            SelectedWiFiNetwork = WiFiNetworks.FirstOrDefault(n => n.Ssid == ConnectedNetworkprofile.ProfileName);
                        }
                    }
                    else
                    {
                        SelectedWiFiNetwork = null;
                    }

                    NetworkScanText = _resourceLoader.GetString("DevicesPage_Scan");
                }
            }
            catch (Exception ex)
            {
                NetworkScanText = _resourceLoader.GetString("DevicesPage_Scan");
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task HandleNetworkMessage(string message)
        {
            NetworkMessage = message;
            IsVisibleNetworkMessageBox = Visibility.Visible;
            await Task.Delay(3000);
            IsVisibleNetworkMessageBox = Visibility.Collapsed;
        }

        private async Task HandleWiFiStatus()
        {
            try
            {
                var previousWiFiStatus = WifiStatus;
                bool wiFiStatus = await _wiFiNetworkService.IsWifiOn();
                if (wiFiStatus)
                {
                    WifiStatus = _resourceLoader.GetString("DevicesPage_Status_On");
                    WifiStatusColor = new SolidColorBrush(Colors.LimeGreen);
                    if (previousWiFiStatus == _resourceLoader.GetString("DevicesPage_Status_Off"))
                        await ScanAvailableNetworks();
                }
                else
                {
                    await ResetWiFiNetworks();
                    WifiStatus = _resourceLoader.GetString("DevicesPage_Status_Off");
                    WifiStatusColor = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task ResetWiFiNetworks()
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    IsVisibleWiFiPasswordBox = Visibility.Collapsed;
                    IsVisibleWiFiConnectPanel = Visibility.Collapsed;
                    IsVisibleNetworkMessageBox = Visibility.Collapsed;
                    NetworkConnectBtnText = _resourceLoader.GetString("DevicesPage_Status_Connect");
                    NetworkScanText = _resourceLoader.GetString("DevicesPage_Scan");
                    IsConnectBtnEnabled = true;
                    IsScanNetworksBtnEnabled = false;
                    IsCheckedAutoUpdate = false;
                    if (WiFiNetworks.Any()) WiFiNetworks.Clear();
                    SelectedWiFiNetwork = default;
                    ConnectedNetworkprofile = default;
                    IsScanNetworksBtnEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void SaveSettings()
        {
            StrongReferenceMessenger.Default.Send(new DevicesMessage { SaveDevicesSettings = true });
        }

        private RelayCommand _scanBluetoothCommand = null;
        public RelayCommand ScanBluetoothCommand
        {
            get
            {
                return _scanBluetoothCommand ?? (_scanBluetoothCommand = new RelayCommand(() => { ShowScanBluetoothDialog(); }));
            }
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() => { CloseDialog(); }));
            }
        }

        private RelayCommand _connectToNetwork = null;
        public RelayCommand ConnectToNetwork
        {
            get
            {
                return _connectToNetwork ?? (_connectToNetwork = new RelayCommand(async () => { await Connect(); }));
            }
        }

        private RelayCommand _scanNetworks = null;
        public RelayCommand ScanNetworks
        {
            get
            {
                return _scanNetworks ?? (_scanNetworks = new RelayCommand(async () => { await ScanAvailableNetworks(); }));
            }
        }

        private RelayCommand _saveSettingsCommand = null;
        public RelayCommand SaveSettingsCommand
        {
            get
            {
                return _saveSettingsCommand ?? (_saveSettingsCommand = new RelayCommand(() => { SaveSettings(); }));
            }
        }

        #endregion
    }
}
