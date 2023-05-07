using System;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Versioning;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class AppUpdateViewModel :ObservableObject
    {
        private Visibility _dismissBtnVisibility;
        public Visibility DismissBtnVisibility
        {
            get { return _dismissBtnVisibility; }
            set { SetProperty(ref _dismissBtnVisibility, value); }
        }

        private string _dialogTitle;
        public string DialogTitle
        {
            get { return _dialogTitle; }
            set
            {
                SetProperty(ref _dialogTitle, value);
            }
        }

        private string _dialogCaption;
        public string DialogCaption
        {
            get { return _dialogCaption; }
            set
            {
                SetProperty(ref _dialogCaption, value);
            }
        }

        private string _dialogDescription;
        public string DialogDescription
        {
            get { return _dialogDescription; }
            set
            {
                SetProperty(ref _dialogDescription, value);
            }
        }

        private string _installButtonText;
        public string InstallButtonText
        {
            get { return _installButtonText; }
            set
            {
                SetProperty(ref _installButtonText, value);
            }
        }

        private ObservableCollection<string> _releaseNotes;
        public ObservableCollection<string> ReleaseNotes
        {
            get { return _releaseNotes; }
            set { SetProperty(ref _releaseNotes, value); }
        }

        private string _releaseDate;
        public string ReleaseDate
        {
            get { return _releaseDate; }
            set
            {
                SetProperty(ref _releaseDate, value);
            }
        }

        private Visibility _isVisibleReleaseNotes = Visibility.Collapsed;
        public Visibility IsVisibleReleaseNotes
        {
            get { return _isVisibleReleaseNotes; }
            set { SetProperty(ref _isVisibleReleaseNotes, value); }
        }

        private readonly IAppVersionService _appVersionService;
        private ResourceLoader _resourceLoader;
        public AppUpdateViewModel(IAppVersionService appVersionService)
        {
            _appVersionService = appVersionService;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            DismissBtnVisibility = Visibility.Visible;
        }

        public void AssignContent()
        {
            var currentLicense = _appVersionService.CurrentVersion;

            ReleaseNotes = new ObservableCollection<string>(currentLicense.ReleaseNotesList);
            ReleaseDate = currentLicense.ReleaseDate.ToString("MMMM yyyy");
            if (ReleaseNotes.Count > 0)
                IsVisibleReleaseNotes = Visibility.Visible; ;

            if (currentLicense.IsForcedUpdate)
            {
                DialogTitle = _resourceLoader.GetString("ForcedUpdateDialogTitle");
                DialogCaption = _resourceLoader.GetString("ForcedUpdateDialogCaption") + " Tala" + _resourceLoader.GetString("ForcedUpdateDialogCaptionContinuation");
                DialogDescription = "";
                InstallButtonText = _resourceLoader.GetString("ForcedUpdateDialogInstallBtnText") + " Tala";
                DismissBtnVisibility = Visibility.Collapsed;
                return;
            }
            else if (!currentLicense.IsLatestVersion)
            {
                DialogTitle = _resourceLoader.GetString("NewVersionDialogTitle");
                DialogCaption = _resourceLoader.GetString("NewVersionDialogCaption");
                DialogDescription = _resourceLoader.GetString("NewVersionDialogDescription") + " Tala " + _resourceLoader.GetString("NewVersionDialogDescription2") + " Tala " + _resourceLoader.GetString("NewVersionDialogDescription3");
                InstallButtonText = _resourceLoader.GetString("NewVersionDialogInstallBtnText");
                DismissBtnVisibility = Visibility.Visible;
            }
            else if (currentLicense.IsUnsupportedVersion)
            {
                DialogTitle = _resourceLoader.GetString("UnsupportedVersionDialogTitle");
                DialogCaption = _resourceLoader.GetString("UnsupportedVersionDialogCaption") + " Tala " + _resourceLoader.GetString("UnsupportedVersionDialogCaption2");
                DialogDescription = _resourceLoader.GetString("UnsupportedVersionDialogDescription");
                InstallButtonText = _resourceLoader.GetString("UnsupportedVersionDialogBtntext") + " Tala";
                DismissBtnVisibility = Visibility.Visible;
            }
        }

        private async void Update()
        {
            //Handle update from the store
            string urlToLaunch = "https://www.speechly.app/";
            var uri = new Uri(urlToLaunch);
            await Windows.System.Launcher.LaunchUriAsync(uri);

            StrongReferenceMessenger.Default.Send(new AppUpdateMessage { CloseUpdateDialog = true });
        }

        private RelayCommand _dismissCommand = null;

        public RelayCommand DismissCommand
        {
            get
            {
                return _dismissCommand ?? (_dismissCommand = new RelayCommand(() => { StrongReferenceMessenger.Default.Send(new AppUpdateMessage { CloseUpdateDialog = true }); }));
            }
        }

        private RelayCommand _installCommand = null;

        public RelayCommand InstallCommand
        {
            get
            {
                return _installCommand ?? (_installCommand = new RelayCommand(() => { Update(); }));
            }
        }
    }
}
