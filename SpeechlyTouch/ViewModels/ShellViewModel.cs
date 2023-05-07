using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DTOs;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Internationalization;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.Pages;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { SetProperty(ref _currentUser, value); }
        }

        private string _pageTitle;
        public string PageTitle
        {
            get { return _pageTitle; }
            set { SetProperty(ref _pageTitle, value); }
        }

        private Visibility _titleViewVisibility;
        public Visibility TitleViewVisibility
        {
            get { return _titleViewVisibility; }
            set { SetProperty(ref _titleViewVisibility, value); }
        }

        //Menu Item Titles
        private string _homeTitle;
        public string HomeTitle
        {
            get { return _homeTitle; }
            set { SetProperty(ref _homeTitle, value); }
        }

        private string _historyTitle;
        public string HistoryTitle
        {
            get { return _historyTitle; }
            set { SetProperty(ref _historyTitle, value); }
        }

        private string _settingsTitle;
        public string SettingsTitle
        {
            get { return _settingsTitle; }
            set { SetProperty(ref _settingsTitle, value); }
        }

        private string _helpTitle;
        public string HelpTitle
        {
            get { return _helpTitle; }
            set { SetProperty(ref _helpTitle, value); }
        }
        private bool _navigationEnabled = true;
        public bool NavigationEnabled 
        {
            get { return _navigationEnabled; }
            set { SetProperty(ref _navigationEnabled, value); }
        }



        public Frame MainFrame;
        public ShellViewModelButtonsStyling BtnStyling { get; set; }
        private readonly ISettingsService _settings;
        private readonly IAuthService _authService;
        private readonly IInternationalizationService _internationalizationService;
        private readonly IAppAnalytics _appAnalytics;
        private readonly IDialogService _dialogService;

        private ResourceLoader _resourceLoader;

        public Timer AppIdleTimer;
        Timer timerReEnterPasscode;

        private ReEnterPasscodeDialog reEnterPasscodeDialog;
        private ErrorDialog errorDialog;    

        public ShellViewModel(ISettingsService settings, IAuthService authService, IInternationalizationService internationalizationService, IAppAnalytics appAnalytics, IDialogService dialogService)
        {
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<LoginMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            _settings = settings;
            _authService = authService;
            _dialogService = dialogService;
            _internationalizationService = internationalizationService;
            _appAnalytics = appAnalytics;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            BtnStyling = new ShellViewModelButtonsStyling();

            _authService.Login();

            InitializeMenuItems();
            GetCurrentUser();
            TitleViewVisibility = Visibility.Visible;

            AppIdleTimer = new Timer();
            AppIdleTimer.Elapsed += AppIdleTimerEllapsed;

            timerReEnterPasscode = new Timer();
            timerReEnterPasscode.Elapsed += TimerReEnterPasscode_Elapsed;
            reEnterPasscodeDialog = new ReEnterPasscodeDialog();
            errorDialog = new ErrorDialog();
        }

        private void HandleMessage(ErrorMessage m)
        {
            if (m.EnableNavigation == false)
            {
                NavigationEnabled = false;
            }
            else
            {
                NavigationEnabled = true;
            }
            if (!m.DisplayDialog)
            {
                if (errorDialog != null)
                    errorDialog.Hide();
            }
        }

        private void InitializeMenuItems()
        {
            HomeTitle = _resourceLoader.GetString("ShellView_Home");
            HistoryTitle = _resourceLoader.GetString("ShellView_History");
            SettingsTitle = _resourceLoader.GetString("SettingsPage_Title");
            HelpTitle = _resourceLoader.GetString("ShellView_Help");
        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                _internationalizationService.SetAppLanguage(message.LanguageCode);

                MainFrame.CacheSize = 0;
                MainFrame.BackStack.Clear();

                await Task.Delay(300);
                reEnterPasscodeDialog = new ReEnterPasscodeDialog();
                InitializeMenuItems();
                NavigateToSettingsView();
                if (message.IsDashboardChange)
                {
                    NavigateToHome();
                }
            }
        }

        private void HandleMessage(LoginMessage message)
        {
            if (message.AccountSwitch)
                GetCurrentUser();
        }

        private async void GetCurrentUser()
        {
            CurrentUser = await _settings.GetUser();

            // This will be removed when user login has been implemented
            if (CurrentUser == null)
            {
                CurrentUser = new User();
                CurrentUser.UserName = "Jane Doe";
            }
        }

        private async void TimerReEnterPasscode_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerReEnterPasscode.Stop();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                reEnterPasscodeDialog.Hide();
                NavigateToHome();
            });
        }

        private async void AppIdleTimerEllapsed(object sender, ElapsedEventArgs e)
        {
            AppIdleTimer.Stop();
            timerReEnterPasscode.Interval = TimeSpan.FromSeconds(Constants.ReEnterPasscodeTimeoutSeconds).TotalMilliseconds;
            timerReEnterPasscode.Start();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseSettingsView = true });
                if(errorDialog != null)
                {
                    errorDialog.Hide();
                   await _dialogService.ShowDialog(reEnterPasscodeDialog);
                   
                }
                 
            });
        }

        private void HandleMessage(NavigationMessage message)
        {
            if (message.LoadSettingsView)
                NavigateToSettingsView();
            if (message.ReLoadSettingsView)
                CloseRenterPasscodeDialog();
            if (message.StartAppIdleTimerCountDown)
                AppIdleTimer.Start();

            if (message.HideShellTitleView)
                TitleViewVisibility = Visibility.Collapsed;
            else
                TitleViewVisibility = Visibility.Visible;
        }

        private async void NavigateToHistory()
        {
            if (NavigationEnabled == false)
            {
               await _dialogService.ShowDialog(errorDialog);
            }
            if (MainFrame != null && BtnStyling.IsHistoryActive == false && NavigationEnabled == true)
                MainFrame.Navigate(typeof(HistoryPage));

            BtnStyling.SelectHistory();
            PageTitle = _resourceLoader.GetString("History");

            _appAnalytics.CaptureCustomEvent("History Navigated",
                    new Dictionary<string, string> { });
        }

        private async void NavigateToSettingsView()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseSessionDetails = true });
            if (NavigationEnabled == false)
            {

               await _dialogService.ShowDialog(errorDialog);
            }
            if (MainFrame != null && NavigationEnabled == true)
                MainFrame.Navigate(typeof(SettingsPage));

            BtnStyling.SelectSettings();
            PageTitle = _resourceLoader.GetString("Settings");

            _appAnalytics.CaptureCustomEvent("Settings Navigated",
                    new Dictionary<string, string> { });
        }

        public void StartAppIdleCountDown()
        {
            int? timeOutMinutes = _settings.AdminModeTimeout;

            if (timeOutMinutes == null)
                timeOutMinutes = 1;

            AppIdleTimer.Interval = TimeSpan.FromMinutes((int)timeOutMinutes).TotalMilliseconds;
            AppIdleTimer.Start();
        }

        private void CloseRenterPasscodeDialog()
        {
            timerReEnterPasscode.Stop();
            reEnterPasscodeDialog.Hide();
        }

        private async void NavigateToHelpView()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseSessionDetails = true });
            if (NavigationEnabled == false)
            {
               await _dialogService.ShowDialog(errorDialog);
            }
            if (MainFrame != null && BtnStyling.IsHelpActive == false && NavigationEnabled == true)
                MainFrame.Navigate(typeof(HelpPage));

            BtnStyling.SelectHelp();
            PageTitle = _resourceLoader.GetString("Help");

            _appAnalytics.CaptureCustomEvent("Help Navigated",
                    new Dictionary<string, string> { });
        }

        private async void NavigateToHome()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseSessionDetails = true });
            if (NavigationEnabled == false)
            {
               await _dialogService.ShowDialog(errorDialog);
            }
            if (NavigationEnabled == true)
            {
                AppIdleTimer.Stop();
                timerReEnterPasscode.Stop();
                NavigationService.Navigate(typeof(DashboardPage));

                _appAnalytics.CaptureCustomEvent("Dashboard Navigated",
                        new Dictionary<string, string> { });
            }
        }

        private RelayCommand _navigateToHomeCommand = null;
        public RelayCommand NavigateToHomeCommand
        {
            get
            {
                return _navigateToHomeCommand ?? (_navigateToHomeCommand = new RelayCommand(() => { NavigateToHome(); }));
            }
        }

        private RelayCommand _navigateToHisoryCommand = null;
        public RelayCommand NavigateToHistoryCommand
        {
            get
            {
                return _navigateToHisoryCommand ?? (_navigateToHisoryCommand = new RelayCommand(() => { NavigateToHistory(); }));
            }
        }

        private RelayCommand _navigateToSettingsCommand = null;
        public RelayCommand NavigateToSettingsCommand
        {
            get
            {
                return _navigateToSettingsCommand ?? (_navigateToSettingsCommand = new RelayCommand(() => { NavigateToSettingsView(); }));
            }
        }

        private RelayCommand _navigateToHelpCommand = null;
        public RelayCommand NavigateToHelpCommand
        {
            get
            {
                return _navigateToHelpCommand ?? (_navigateToHelpCommand = new RelayCommand(() => { NavigateToHelpView(); }));
            }
        }
    }
}
