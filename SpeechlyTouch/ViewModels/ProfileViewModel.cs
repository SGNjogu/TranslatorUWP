using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class ProfileViewModel : ObservableObject
    {
        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { SetProperty(ref _currentUser, value); }
        }

        private string _userDataExportEmail;
        public string UserDataExportEmail
        {
            get { return _userDataExportEmail; }
            set { SetProperty(ref _userDataExportEmail, value); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                SetProperty(ref _isEditing, value);
                if (IsEditing)
                    EditBtnText = _resourceLoader.GetString("Save_Text");
                else
                    EditBtnText = _resourceLoader.GetString("Edit"); ;
            }
        }

        private string _editBtnText;
        public string EditBtnText
        {
            get { return _editBtnText; }
            set { SetProperty(ref _editBtnText, value); }
        }

        private Visibility _errorMessageVisibility;
        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set { SetProperty(ref _errorMessageVisibility, value); }
        }

        private ObservableCollection<int> _adminModeTimeOutList;

        public ObservableCollection<int> AdminModeTimeOutList
        {
            get { return _adminModeTimeOutList; }
            set
            {
                SetProperty(ref _adminModeTimeOutList, value);
            }
        }

        private int _adminModeTimeOut;

        public int AdminModeTimeOut
        {
            get { return _adminModeTimeOut; }
            set
            {
                SetProperty(ref _adminModeTimeOut, value);
                SetAdminModeTimeOut();
            }
        }

        private bool _unsavedTimeoutChanges;

        public bool UnsavedTimeoutChanges
        {
            get { return _unsavedTimeoutChanges; }
            set
            {
                SetProperty(ref _unsavedTimeoutChanges, value);
                
            }
        }

        private ResourceLoader _resourceLoader;
        private ChangePasscodeDialog changePasscodeDialog;
        private readonly ISettingsService _settings;
        private readonly IDialogService _dialogService;

        public ProfileViewModel(ISettingsService settings, IDialogService dialogService)
        {
            StrongReferenceMessenger.Default.Register<PasscodeMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            _settings = settings;
            _dialogService = dialogService; 
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            _ = GetCurrentUser();
            IsEditing = false;
            UnsavedTimeoutChanges = false;
            ErrorMessageVisibility = Visibility.Collapsed;
            Initialize();
        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                changePasscodeDialog = new ChangePasscodeDialog();
            }
        }

        public async void Initialize()
        {
            try
            {
                AdminModeTimeOutList = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
                if (_settings.AdminModeTimeout != null)
                    AdminModeTimeOut = (int)_settings.AdminModeTimeout;
                else
                    AdminModeTimeOut = 1;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    changePasscodeDialog = new ChangePasscodeDialog();
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void SetAdminModeTimeOut()
        {
            _settings.AdminModeTimeout = AdminModeTimeOut;
        }

        public async Task GetCurrentUser()
        {
            await Task.Delay(1000);
            CurrentUser = await _settings.GetUser();

            // This will be removed when user login has been implemented
            if (CurrentUser == null)
            {
                CurrentUser = new User();
                CurrentUser.UserName = "Jane Doe";
                CurrentUser.PolicyType = "Speechly Business Core";
                CurrentUser.UserDataExportEmail = "jane.doe@fitts.io";
                CurrentUser.UserEmail = "jane.doe@fitts.io";
            }

            UserDataExportEmail = CurrentUser.UserDataExportEmail;
        }

        private async Task ShowChangePasscodeDialog()
        {
           await _dialogService.ShowDialog(changePasscodeDialog);
        }

        private void HandleMessage(PasscodeMessage message)
        {
            if (message.ClosePasscodeDialogs)
            {
                _dialogService.HideDialog();
                StrongReferenceMessenger.Default.Send(new NavigationMessage { StartAppIdleTimerCountDown = true });
            }
        }

        private async Task SaveUserDataExportEmail()
        {
            if (IsEditing)
            {
                if (!string.IsNullOrEmpty(UserDataExportEmail) && IsValidEmail(UserDataExportEmail))
                {
                    CurrentUser.UserDataExportEmail = UserDataExportEmail;
                    await _settings.SaveUser(CurrentUser);
                    IsEditing = false;
                }
                else
                {
                    ErrorMessageVisibility = Visibility.Visible;
                    await Task.Delay(4000);
                    ErrorMessageVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                IsEditing = true;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public void LogTimeOutChangeMade()
        {
            StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("TimeoutUpdated") });

        }

        private RelayCommand _showChangePasscodeCommand = null;
        public RelayCommand ShowChangePasscodeCommand
        {
            get
            {
                return _showChangePasscodeCommand ?? (_showChangePasscodeCommand = new RelayCommand(async () => { await ShowChangePasscodeDialog(); }));
            }
        }

        private RelayCommand _editCommand = null;
        public RelayCommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new RelayCommand(async () => { await SaveUserDataExportEmail(); }));
            }
        }

        private RelayCommand _logTimeOutChange = null;
        public RelayCommand LogTimeOutChange
        {
            get
            {
                return _logTimeOutChange ?? (_logTimeOutChange = new RelayCommand( () => { LogTimeOutChangeMade(); }));
            }
        }
    }
}
