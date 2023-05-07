using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Audio;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using DataServiceInputDevice = SpeechlyTouch.DataService.Models.InputDevice;
using DataServiceOutputDevice = SpeechlyTouch.DataService.Models.OutputDevice;
using CustomProfile = SpeechlyTouch.DataService.Models.CustomProfile;

namespace SpeechlyTouch.ViewModels
{
    public class AudioDevicesViewModel : ObservableObject
    {
        private ObservableCollection<InputDevice> _personOneInputDevicesList;
        public ObservableCollection<InputDevice> PersonOneInputDevicesList
        {
            get { return _personOneInputDevicesList; }
            set { SetProperty(ref _personOneInputDevicesList, value); }
        }

        private ObservableCollection<InputDevice> _personTwoInputDevicesList;
        public ObservableCollection<InputDevice> PersonTwoInputDevicesList
        {
            get { return _personTwoInputDevicesList; }
            set { SetProperty(ref _personTwoInputDevicesList, value); }
        }

        private InputDevice _personOneSelectedInputDevice;
        public InputDevice PersonOneSelectedInputDevice
        {
            get { return _personOneSelectedInputDevice; }
            set
            {
                SetProperty(ref _personOneSelectedInputDevice, value);
                if (PersonOneSelectedInputDevice != null)
                {
                    PersonTwoInputDevicesList = new ObservableCollection<InputDevice>(PersonOneInputDevicesList.Where(i => i != PersonOneSelectedInputDevice));
                }
                if(PersonOneSelectedInputDevice?.Name != null && PersonOneInitialSelectedInputDevice?.Name != null)
                {
                    if(PersonOneSelectedInputDevice.Name != PersonOneInitialSelectedInputDevice.Name)
                    {
                        StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = false, SettingsType = SettingsType.Devices });
                    }
                }
            }
        }

        private InputDevice _personTwoSelectedInputDevice;
        public InputDevice PersonTwoSelectedInputDevice
        {
            get { return _personTwoSelectedInputDevice; }
            set
            {
                SetProperty(ref _personTwoSelectedInputDevice, value);
                if (PersonTwoSelectedInputDevice?.Name != null && PersonTwoInitialSelectedInputDevice?.Name != null)
                {
                    if (PersonTwoSelectedInputDevice.Name != PersonTwoInitialSelectedInputDevice.Name)
                    {
                        StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = false, SettingsType = SettingsType.Devices });
                    }
                }
            }
        }

        private ObservableCollection<OutputDevice> _personOneOutputDevicesList;
        public ObservableCollection<OutputDevice> PersonOneOutputDevicesList
        {
            get { return _personOneOutputDevicesList; }
            set { SetProperty(ref _personOneOutputDevicesList, value); }
        }

        private ObservableCollection<OutputDevice> _personTwoOutputDevicesList;
        public ObservableCollection<OutputDevice> PersonTwoOutputDevicesList
        {
            get { return _personTwoOutputDevicesList; }
            set {
                SetProperty(ref _personTwoOutputDevicesList, value);
               
            }
        }

        private OutputDevice _personOneSelectedOutputDevice;
        public OutputDevice PersonOneSelectedOutputDevice
        {
            get { return _personOneSelectedOutputDevice; }
            set
            {
                SetProperty(ref _personOneSelectedOutputDevice, value);
                if (PersonOneSelectedOutputDevice != null)
                {
                    PersonTwoOutputDevicesList = new ObservableCollection<OutputDevice>(PersonOneOutputDevicesList.Where(i => i != PersonOneSelectedOutputDevice));
                }
                if (PersonOneSelectedOutputDevice?.Name != null && PersonOneInitialSelectedOutputDevice?.Name != null)
                {
                    if (PersonOneSelectedOutputDevice.Name != PersonOneInitialSelectedOutputDevice.Name)
                    {
                        StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = false, SettingsType = SettingsType.Devices });
                    }
                }
            }
        }

        private OutputDevice _personTwoSelectedOutputDevice;
        public OutputDevice PersonTwoSelectedOutputDevice
        {
            get { return _personTwoSelectedOutputDevice; }
            set {
                SetProperty(ref _personTwoSelectedOutputDevice, value);
                if (PersonTwoSelectedOutputDevice?.Name != null && PersonTwoInitialSelectedOutputDevice?.Name != null)
                {
                    if (PersonTwoSelectedOutputDevice.Name != PersonTwoInitialSelectedOutputDevice.Name)
                    {
                        StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = false, SettingsType = SettingsType.Devices });
                    }
                }
            }
        }

        private bool _isCheckedSingleDevice;
        public bool IsCheckedSingleDevice
        {
            get { return _isCheckedSingleDevice; }
            set
            {
                SetProperty(ref _isCheckedSingleDevice, value);
                if (IsCheckedSingleDevice)
                    IsCheckedDualDevice = false;
            }
        }

        private bool _isCheckedDualDevice;
        public bool IsCheckedDualDevice
        {
            get { return _isCheckedDualDevice; }
            set
            {
                SetProperty(ref _isCheckedDualDevice, value);
                if (IsCheckedDualDevice)
                {
                    IsCheckedSingleDevice = false;
                    IsVisibleParticipantTitles = Visibility.Visible;
                    IsVisiblePersonTwoSetup = Visibility.Visible;
                }
                else
                {
                    IsVisiblePersonTwoSetup = Visibility.Collapsed;
                    IsVisibleParticipantTitles = Visibility.Collapsed;
                }
            }
        }
        private bool _dualSetupChanges;
        public bool DualSetupChanges
        {
            get { return _dualSetupChanges; }
            set
            {
                SetProperty(ref _dualSetupChanges, value);
            }
        }

        private Visibility _isVisibleParticipantTitles;
        public Visibility IsVisibleParticipantTitles
        {
            get { return _isVisibleParticipantTitles; }
            set { SetProperty(ref _isVisibleParticipantTitles, value); }
        }

        private Visibility _isVisiblePersonTwoSetup;
        public Visibility IsVisiblePersonTwoSetup
        {
            get { return _isVisiblePersonTwoSetup; }
            set { SetProperty(ref _isVisiblePersonTwoSetup, value); }
        }

        private InputDevice _personOneInitialSelectedInputDevice;
        public InputDevice PersonOneInitialSelectedInputDevice
        {
            get { return _personOneInitialSelectedInputDevice; }
            set
            {
                SetProperty(ref _personOneInitialSelectedInputDevice, value);
            }
        }
        private OutputDevice _personOneInitialSelectedOutputDevice;
        public OutputDevice PersonOneInitialSelectedOutputDevice
        {
            get { return _personOneInitialSelectedOutputDevice; }
            set
            {
                SetProperty(ref _personOneInitialSelectedOutputDevice, value);
            }
        }

        private InputDevice _personTwoInitialSelectedInputDevice;
        public InputDevice PersonTwoInitialSelectedInputDevice
        {
            get { return _personTwoInitialSelectedInputDevice; }
            set
            {
                SetProperty(ref _personTwoInitialSelectedInputDevice, value);
            }
        }

        private OutputDevice _personTwoInitialSelectedOutputDevice;
        public OutputDevice PersonTwoInitialSelectedOutputDevice
        {
            get { return _personTwoInitialSelectedOutputDevice; }
            set { SetProperty(ref _personTwoInitialSelectedOutputDevice, value); }
        }

        private Visibility _isVisibleProfileSetup;
        public Visibility IsVisibleProfileSetup
        {
            get { return _isVisibleProfileSetup; }
            set { SetProperty(ref _isVisibleProfileSetup, value); }
        }

        private ObservableCollection<CustomProfile> _customProfilesList;
        public ObservableCollection<CustomProfile> CustomProfilesList
        {
            get { return _customProfilesList; }
            set { SetProperty(ref _customProfilesList, value); }
        }

        private CustomProfile _selectedCustomProfile;
        public CustomProfile SelectedCustomProfile
        {
            get { return _selectedCustomProfile; }
            set
            {
                SetProperty(ref _selectedCustomProfile, value);
                HandleProfileSelection();
            }
        }

        private string _profileName;
        public string ProfileName
        {
            get { return _profileName; }
            set { SetProperty(ref _profileName, value); }
        }

        private string _defaultProfile;
        public string DefaultProfile
        {
            get { return _defaultProfile; }
            set { SetProperty(ref _defaultProfile, value); }
        }

        private bool _isCheckedDefaultProfile;
        public bool IsCheckedDefaultProfile
        {
            get { return _isCheckedDefaultProfile; }
            set { SetProperty(ref _isCheckedDefaultProfile, value); }
        }

        private Visibility _isVisibleAvailableProfiles;
        public Visibility IsVisibleAvailableProfiles
        {
            get { return _isVisibleAvailableProfiles; }
            set { SetProperty(ref _isVisibleAvailableProfiles, value); }
        }

        private Visibility _isVisibleDefaultProfile;
        public Visibility IsVisibleDefaultProfile
        {
            get { return _isVisibleDefaultProfile; }
            set { SetProperty(ref _isVisibleDefaultProfile, value); }
        }

        private readonly IDataService _dataService;
        private readonly IAudioService _audioService;
        private readonly ISettingsService _settingsService;
        private readonly IAppAnalytics _appAnalytics;
        private ResourceLoader _resourceLoader;

        public AudioDevicesViewModel(IDataService dataService, IAudioService audioService, ISettingsService settingsService, IAppAnalytics appAnalytics)
        {
            _dataService = dataService;
            _audioService = audioService;
            _settingsService = settingsService;
            _appAnalytics = appAnalytics;
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            StrongReferenceMessenger.Default.Register<DevicesMessage>(this, (r, message) =>
            {
                HandleMessage(message);
            });
            StrongReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            IsVisibleParticipantTitles = Visibility.Collapsed;
            IsVisiblePersonTwoSetup = Visibility.Collapsed;
            IsVisibleProfileSetup = Visibility.Collapsed;
            IsVisibleAvailableProfiles = Visibility.Collapsed;
            IsVisibleDefaultProfile = Visibility.Visible;
            PersonOneInputDevicesList = new ObservableCollection<InputDevice>();
            PersonOneOutputDevicesList = new ObservableCollection<OutputDevice>();
            PersonOneSelectedInputDevice = new InputDevice();
            PersonOneSelectedOutputDevice = new OutputDevice();
            PersonTwoSelectedInputDevice = new InputDevice();
            PersonTwoSelectedOutputDevice = new OutputDevice();
            CustomProfilesList = new ObservableCollection<CustomProfile>();
            SelectedCustomProfile = new CustomProfile();
            IsCheckedDefaultProfile = false;
            CheckDevicesTypeSelection();
            SetInitialDevice();
        }
        private async void HandleMessage(ErrorMessage m)
        {
            if (m.SettingsType == Enums.SettingsType.Devices && m.EnableNavigation)
            {
                await SaveDevicesSettings();
            }
        }

        private async void HandleMessage(DevicesMessage message)
        {
            if (message.SaveDevicesSettings)
            await SaveDevicesSettings();
            if (message.ReloadAudioInputDevices || message.ReloadAudioOuputDevices)
               await LoadAudioDevices();
        }

        private void HandleProfileSelection()
        {
            if(SelectedCustomProfile == null || SelectedCustomProfile.Name == null)
            {
                return;
            }

            IsCheckedSingleDevice = SelectedCustomProfile.IsSingleDevice;
            if (!IsCheckedSingleDevice)
            {
                IsCheckedDualDevice = true;
            }

            //Set person one input device
            var personOneInput = PersonOneInputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonOneInputDevice);
            if (personOneInput)
            {
                PersonOneSelectedInputDevice = PersonOneInputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonOneInputDevice);
            }

            //Set person one output device
            var personOneOutput = PersonOneOutputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonOneOutputDevice);
            if (personOneOutput)
            {
                PersonOneSelectedOutputDevice = PersonOneOutputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonOneOutputDevice);
            }

            if (!SelectedCustomProfile.IsSingleDevice)
            {
                //Set person two input device
                var personTwoInput = PersonTwoInputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonTwoInputDevice);
                if (personTwoInput)
                {
                    PersonTwoSelectedInputDevice = PersonOneInputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonTwoInputDevice);
                }

                //Set person two output device
                var personTwoOutput = PersonTwoOutputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonTwoOutputDevice);
                if (personTwoOutput)
                {
                    PersonTwoSelectedOutputDevice = PersonOneOutputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonTwoOutputDevice);
                }
            }
        }

        private void CheckDevicesTypeSelection()
        {
            if (_settingsService.IsCheckedSingleDevice)
                IsCheckedSingleDevice = true;
            else
                IsCheckedDualDevice = true;
        }

        private void SetInitialDevice()
        {
            PersonOneInitialSelectedInputDevice = PersonOneSelectedInputDevice;
            PersonTwoInitialSelectedInputDevice = PersonTwoSelectedInputDevice;
            PersonOneInitialSelectedOutputDevice = PersonOneSelectedOutputDevice;
            PersonTwoInitialSelectedOutputDevice = PersonTwoSelectedOutputDevice;
        }

        public async Task LoadAudioDevices()
        {
            CheckDevicesTypeSelection();
            await LoadAudioInputDevices();
            await LoadAudioOutputDevices();
            await LoadCustomProfiles();
            SetInitialDevice();
        }

        private async Task LoadAudioInputDevices()
        {
            var inputDevices = await _audioService.InputDevices();
            PersonOneInputDevicesList = new ObservableCollection<InputDevice>(inputDevices);
            PersonOneSelectedInputDevice = await _audioService.ParticipantOneInputDevice();
            var personTwoSelectedInputDevice = await _audioService.ParticipantTwoInputDevice();

            if (PersonOneSelectedInputDevice != null && PersonOneInputDevicesList.Count > 1)
            {
                PersonTwoInputDevicesList = new ObservableCollection<InputDevice>(PersonOneInputDevicesList.Where(i => i != PersonOneSelectedInputDevice));
                PersonTwoSelectedInputDevice = personTwoSelectedInputDevice != null ? personTwoSelectedInputDevice : PersonTwoInputDevicesList.FirstOrDefault();
            }
        }

        private async Task LoadAudioOutputDevices()
        {
            var outputDevices = await _audioService.OutputDevices();
            PersonOneOutputDevicesList = new ObservableCollection<OutputDevice>(outputDevices);
            PersonOneSelectedOutputDevice = await _audioService.ParticipantOneOutputDevice();
            var personTwoSelectedOutputDevice = await _audioService.ParticipantTwoOutputDevice();

            if (PersonOneSelectedOutputDevice != null && PersonOneOutputDevicesList.Count > 1)
            {
                PersonTwoOutputDevicesList = new ObservableCollection<OutputDevice>(PersonOneOutputDevicesList.Where(i => i != PersonOneSelectedOutputDevice));
                PersonTwoSelectedOutputDevice = personTwoSelectedOutputDevice != null ? personTwoSelectedOutputDevice : PersonTwoOutputDevicesList.FirstOrDefault();
            }
        }

        private async Task LoadCustomProfiles()
        {
            CustomProfilesList = new ObservableCollection<CustomProfile>();
            SelectedCustomProfile = new CustomProfile();
            var storedProfiles = await _dataService.GetCustomProfilesAsync();
            if(storedProfiles == null || !storedProfiles.Any())
            {
                IsVisibleAvailableProfiles = Visibility.Collapsed;
                IsVisibleDefaultProfile = Visibility.Collapsed;
            }
            else
            {
                foreach(var profile in storedProfiles)
                {
                    bool validInputOne = PersonOneInputDevicesList.Any(n => n.Name == profile.PersonOneInputDevice);
                    bool validOutputOne = PersonOneOutputDevicesList.Any(n => n.Name == profile.PersonOneOutputDevice);
                    bool validInputTwo = true;
                    bool validOutputTwo = true;
                    if (!profile.IsSingleDevice)
                    {
                        validInputTwo = PersonOneInputDevicesList.Any(n => n.Name == profile.PersonTwoInputDevice);
                        validOutputTwo = PersonOneOutputDevicesList.Any(n => n.Name == profile.PersonTwoOutputDevice);
                    }

                    if(validInputOne && validOutputOne && validInputTwo && validOutputTwo)
                    {
                        CustomProfilesList.Add(profile);
                    }
                }

                IsVisibleAvailableProfiles = Visibility.Visible;
                if(CustomProfilesList != null && CustomProfilesList.Any(p => p.IsDefault))
                {
                    SelectedCustomProfile = CustomProfilesList.FirstOrDefault(n => n.IsDefault);

                    IsCheckedSingleDevice = SelectedCustomProfile.IsSingleDevice;
                    if (!IsCheckedSingleDevice)
                    {
                        IsCheckedDualDevice = true;
                    }

                    //Set person one input device
                    var personOneInput = PersonOneInputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonOneInputDevice);
                    if (personOneInput)
                    {
                        PersonOneSelectedInputDevice = PersonOneInputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonOneInputDevice);
                    }

                    //Set person one output device
                    var personOneOutput = PersonOneOutputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonOneOutputDevice);
                    if (personOneOutput)
                    {
                        PersonOneSelectedOutputDevice = PersonOneOutputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonOneOutputDevice);
                    }

                    if (!SelectedCustomProfile.IsSingleDevice)
                    {
                        //Set person two input device
                        var personTwoInput = PersonTwoInputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonTwoInputDevice);
                        if (personTwoInput)
                        {
                            PersonTwoSelectedInputDevice = PersonOneInputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonTwoInputDevice);
                        }

                        //Set person two input device
                        var personTwoOutput = PersonTwoOutputDevicesList.Any(n => n.Name == SelectedCustomProfile.PersonTwoOutputDevice);
                        if (personTwoOutput)
                        {
                            PersonTwoSelectedOutputDevice = PersonOneOutputDevicesList.FirstOrDefault(n => n.Name == SelectedCustomProfile.PersonTwoOutputDevice);
                        }
                    }

                    IsVisibleDefaultProfile = Visibility.Visible;
                }
                else
                {
                    SelectedCustomProfile = CustomProfilesList.FirstOrDefault();
                    IsVisibleDefaultProfile = Visibility.Collapsed;
                }
            }
        }

        private void CheckDeviceSelectionUpdate()
        {
           
            if (PersonOneInitialSelectedInputDevice != PersonOneSelectedInputDevice || PersonOneInitialSelectedOutputDevice != PersonOneSelectedOutputDevice)
            {
                if (IsCheckedSingleDevice)
                {
                    StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                    StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("SingleDeviceUpdated")});
                }
                else
                {
                    DualSetupChanges = true;
                }
              
            }
            if (PersonTwoInitialSelectedInputDevice?.Name != PersonTwoSelectedInputDevice?.Name ||
                PersonTwoInitialSelectedOutputDevice?.Name != PersonTwoSelectedOutputDevice?.Name ||
                PersonTwoInitialSelectedInputDevice == null ||
                PersonTwoInitialSelectedOutputDevice == null) 
            {
                if (IsCheckedDualDevice && DualSetupChanges)
                {
                    StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                    StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("DualDeviceUpdated")});
                }
                if (IsCheckedDualDevice)
                {
                    StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                    StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("DualDeviceUpdated")});
                }
              
            }
        }

        private async Task SaveDevicesSettings()
        {
            var user = await _settingsService.GetUser();
            await _dataService.DeleteAllItemsAsync<DataServiceInputDevice>();
            await _dataService.DeleteAllItemsAsync<DataServiceOutputDevice>();

            if (PersonOneSelectedInputDevice?.Name != null)
            {
                await _dataService.AddInputDeviceAsync(new DataServiceInputDevice
                {
                    Name = PersonOneSelectedInputDevice.Name,
                    DeviceId = PersonOneSelectedInputDevice.DeviceId,
                    IsJabra = PersonOneSelectedInputDevice.IsJabra,
                    Participant = "Person One"
                });
            }

            if (PersonOneSelectedOutputDevice?.Name != null)
            {
                await _dataService.AddOutputDeviceAsync(new DataServiceOutputDevice
                {
                    Name = PersonOneSelectedOutputDevice.Name,
                    DeviceId = PersonOneSelectedOutputDevice.DeviceId,
                    IsJabra = PersonOneSelectedOutputDevice.IsJabra,
                    Participant = "Person One"
                });
            }

            if (!IsCheckedSingleDevice)
            {
                if (PersonTwoSelectedInputDevice?.Name != null)
                {
                    await _dataService.AddInputDeviceAsync(new DataServiceInputDevice
                    {
                        Name = PersonTwoSelectedInputDevice.Name,
                        DeviceId = PersonTwoSelectedInputDevice.DeviceId,
                        IsJabra = PersonTwoSelectedInputDevice.IsJabra,
                        Participant = "Person Two"
                    });
                }

                if (PersonTwoSelectedOutputDevice?.Name != null)
                {
                    await _dataService.AddOutputDeviceAsync(new DataServiceOutputDevice
                    {
                        Name = PersonTwoSelectedOutputDevice.Name,
                        DeviceId = PersonTwoSelectedOutputDevice.DeviceId,
                        IsJabra = PersonTwoSelectedOutputDevice.IsJabra,
                        Participant = "Person Two"
                    });
                }
            }
            CheckDeviceSelectionUpdate();
            SetInitialDevice();
            if(_settingsService.IsCheckedSingleDevice != IsCheckedSingleDevice)
            {
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("DeviceSetupChanged") });
            }
            _settingsService.IsCheckedSingleDevice = IsCheckedSingleDevice;
            await _audioService.LoadDeviceLists();
            StrongReferenceMessenger.Default.Send(new DevicesMessage { ReloadDashboardAudioDevices = true });

            _appAnalytics.CaptureCustomEvent("Settings Changes",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Audio Device Selection Changed" }
                    });
        }

        private async Task SaveSettings()
        {
            await SaveDevicesSettings();
            StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadLanguagesInitialSetup = true });
        }

        private void ShowProfileSetup()
        {
            if(IsVisibleProfileSetup == Visibility.Visible)
            {
                IsVisibleProfileSetup = Visibility.Collapsed;
            }
            else
            {
                IsVisibleProfileSetup = Visibility.Visible;
            }
        }

        private async Task SaveProfile()
        {
            if(CustomProfilesList.Any() && CustomProfilesList.Count > 0 && SelectedCustomProfile != null)
            {
                SelectedCustomProfile.PersonOneInputDevice = PersonOneSelectedInputDevice.Name;
                SelectedCustomProfile.PersonOneOutputDevice = PersonOneSelectedOutputDevice.Name;
                SelectedCustomProfile.IsDefault = IsCheckedDefaultProfile;
                
                if (IsCheckedSingleDevice)
                {
                    SelectedCustomProfile.IsSingleDevice = true;
                }
                else
                {
                    SelectedCustomProfile.IsSingleDevice = false;
                    SelectedCustomProfile.PersonTwoInputDevice = PersonTwoSelectedInputDevice?.Name;
                    SelectedCustomProfile.PersonTwoOutputDevice = PersonTwoSelectedOutputDevice?.Name;
                }

                if (IsCheckedDefaultProfile)
                {
                    var existingprofiles = await _dataService.GetCustomProfilesAsync();
                    var defaultProfiles = existingprofiles.FindAll(n => n.IsDefault);
                    foreach(var profile in defaultProfiles)
                    {
                        profile.IsDefault = false;
                        await _dataService.UpdateItemAsync<CustomProfile>(profile);
                    }
                }

                var existingProfile = await _dataService.GetCustomProfile(SelectedCustomProfile.Name);
                if(existingProfile != null)
                {
                    existingProfile = SelectedCustomProfile;
                    await _dataService.UpdateItemAsync<CustomProfile>(existingProfile);
                }
                else
                {
                    await _dataService.AddItemAsync<CustomProfile>(SelectedCustomProfile);
                }

                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("CustomProfileProfileSaved") });
            }
            else
            {
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("CustomProfileNotSaved") });
            }
        }

        private void AddProfile()
        {
            if (!string.IsNullOrWhiteSpace(ProfileName) && !CustomProfilesList.Any(n => n.Name == ProfileName))
            {
                CustomProfilesList.Add(new CustomProfile { Name = ProfileName });
                SelectedCustomProfile = CustomProfilesList.FirstOrDefault(n => n.Name == ProfileName);
                IsVisibleAvailableProfiles = Visibility.Visible;

                ProfileName = string.Empty;
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("CustomProfileAdded") });
            }
            else
            {
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("CustomProfileInvalidName") });
            }
        }

        private async Task DeleteProfile()
        {
            if(SelectedCustomProfile != null && CustomProfilesList.Any())
            {
                CustomProfilesList.Remove(SelectedCustomProfile);
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("CustomProfileDeleted") });
                if (CustomProfilesList.Any() && CustomProfilesList.Count > 0)
                {
                    SelectedCustomProfile = CustomProfilesList.FirstOrDefault();
                    var existingProfile = await _dataService.GetCustomProfile(SelectedCustomProfile.Name);
                    if(existingProfile != null)
                    {
                        await _dataService.DeleteItemAsync<CustomProfile>(existingProfile);
                    }

                    if(CustomProfilesList.Count == 0)
                    {
                        IsVisibleAvailableProfiles = Visibility.Collapsed;
                        IsVisibleDefaultProfile = Visibility.Collapsed;
                        SelectedCustomProfile = new CustomProfile();
                    }
                }
                else
                {
                    IsVisibleAvailableProfiles = Visibility.Collapsed;
                    IsVisibleDefaultProfile = Visibility.Collapsed;
                    SelectedCustomProfile = new CustomProfile();
                }
            }
            else
            {
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("CustomProfileNotDeleted") });
            }
        }

        private RelayCommand _moveToLanguageSetupCommand = null;
        public RelayCommand MoveToLanguageSetupCommand
        {
            get
            {
                return _moveToLanguageSetupCommand ?? (_moveToLanguageSetupCommand = new RelayCommand(async () =>
                {
                    await SaveSettings();
                }));
            }
        }

        private RelayCommand _beginSetupCommand = null;
        public RelayCommand BeginSetupCommand
        {
            get
            {
                return _beginSetupCommand ?? (_beginSetupCommand = new RelayCommand(() =>
                {
                    ShowProfileSetup();
                }));
            }
        }

        private RelayCommand _saveProfileCommand = null;
        public RelayCommand SaveProfileCommand
        {
            get
            {
                return _saveProfileCommand ?? (_saveProfileCommand = new RelayCommand(async () =>
                {
                    await SaveProfile();
                }));
            }
        }

        private RelayCommand _addProfileCommand = null;
        public RelayCommand AddProfileCommand
        {
            get
            {
                return _addProfileCommand ?? (_addProfileCommand = new RelayCommand(() =>
                {
                    AddProfile();
                }));
            }
        }

        private RelayCommand _deleteProfileCommand = null;
        public RelayCommand DeleteProfileCommand
        {
            get
            {
                return _deleteProfileCommand ?? (_deleteProfileCommand = new RelayCommand(async () =>
                {
                    await DeleteProfile();
                }));
            }
        }
    }
}
