using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.DataSync.Interfaces;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.ContentControls.SessionHistory;
using SpeechlyTouch.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.ViewModels
{
    public class HistoryViewModel : ObservableObject
    {
        private bool IsFirstLoad { get; set; }
        private ObservableCollection<Session> _histories;
        public ObservableCollection<Session> Histories
        {
            get { return _histories; }
            set { SetProperty(ref _histories, value); }
        }

        private ObservableCollection<Session> _originalSessionsList;
        public ObservableCollection<Session> OriginalSessionsList
        {
            get { return _originalSessionsList; }
            set { SetProperty(ref _originalSessionsList, value); }
        }

        private string _filterSearchText;

        public string FilterSearchText
        {
            get { return _filterSearchText; }
            set { SetProperty(ref _filterSearchText, value); }
        }

        private DateTimeOffset? _searchFromDate;

        public DateTimeOffset? SearchFromDate
        {
            get { return _searchFromDate; }
            set { SetProperty(ref _searchFromDate, value); }
        }

        private DateTimeOffset? _searchToDate;

        public DateTimeOffset? SearchToDate
        {
            get { return _searchToDate; }
            set { SetProperty(ref _searchToDate, value); }
        }

        private Session _selectedSession;

        public Session SelectedSession
        {
            get { return _selectedSession; }
            set
            {
                SetProperty(ref _selectedSession, value);
                if (SelectedSession != null)
                    GetSessionDetails(SelectedSession);
            }
        }

        private Visibility _isBusy;

        public Visibility IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        /// <summary>
        /// This property binds to the visibility of the export and share buttons in the HistorySection
        /// </summary>
        private Visibility _isVisibleExportAndShareButtons;

        public Visibility IsVisibleExportAndShareButtons
        {
            get { return _isVisibleExportAndShareButtons; }
            set { SetProperty(ref _isVisibleExportAndShareButtons, value); }
        }

        private Visibility _isVisibleHistorySection = Visibility.Visible;

        public Visibility IsVisibleHistorySection
        {
            get { return _isVisibleHistorySection; }
            set { SetProperty(ref _isVisibleHistorySection, value); }
        }

        private Visibility _isVisibleHistoryNotEnabledMessage = Visibility.Collapsed;

        public Visibility IsVisibleHistoryNotEnabledMessage
        {
            get { return _isVisibleHistoryNotEnabledMessage; }
            set { SetProperty(ref _isVisibleHistoryNotEnabledMessage, value); }
        }

        private bool IsEnabledHistorySection { get; set; } = false;
        public static bool IsFileShare { get; set; } = false;
        public static StorageFile File { get; set; } = null;
        public Frame ContentFrame;
        private readonly IDataService _dataService;
        private readonly IPullDataService _pullDataService;
        private readonly ICrashlytics _crashlytics;
        private readonly ISettingsService _settings;
        private readonly IAppAnalytics _appAnalytics;

        public HistoryViewModel(IDataService dataService, IPullDataService pullDataService, ICrashlytics crashlytics, ISettingsService settings, IAppAnalytics appAnalytics)
        {
            _dataService = dataService;
            _pullDataService = pullDataService;
            _crashlytics = crashlytics;
            _settings = settings;
            _appAnalytics = appAnalytics;

            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<LoginMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<HistoryMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            SearchFromDate = new DateTimeOffset(DateTimeOffset.Now.Year, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0));
            SearchToDate = DateTimeOffset.Now;

            IsVisibleExportAndShareButtons = Visibility.Visible;
            IsBusy = Visibility.Collapsed;
            IsFirstLoad = true;

            LoadHistory();
        }

        private async void HandleMessage(LoginMessage message)
        {
            if (message.AccountSwitch && !IsFirstLoad)
            {
                try
                {
                    await SetHistory();
                    //Re-assign histories list after an account switch without closing the application
                    Thread pullThread = new Thread(_pullDataService.BeginDataSync);
                    pullThread.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                }

            }
        }

        private async void HandleMessage(HistoryMessage m)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (m.RefreshHistory)
                {
                    await SetHistory();
                }
            });
        }

        public async Task LoadOrganizationSettings()
        {
            try
            {
                var orgSettings = await _dataService.GetOrganizationSettingsAsync();

                if (orgSettings != null)
                {
                    IsEnabledHistorySection = orgSettings.FirstOrDefault().HistoryPlaybackEnabled;
                    if (orgSettings.FirstOrDefault().ExportEnabled)
                    {
                        IsVisibleExportAndShareButtons = Visibility.Visible;
                    }
                    else
                    {
                        IsVisibleExportAndShareButtons = Visibility.Collapsed;
                    }

                    if (IsEnabledHistorySection)
                    {
                        IsVisibleHistorySection = Visibility.Visible;
                        IsVisibleHistoryNotEnabledMessage = Visibility.Collapsed;
                    }
                    else
                    {
                        IsVisibleHistorySection = Visibility.Collapsed;
                        IsVisibleHistoryNotEnabledMessage = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void LoadHistory()
        {
            await Task.Delay(500);
            await SetHistory();
        }

        /// <summary>
        /// Method to get Sessions from Database
        /// </summary>
        /// <returns>Collection of Sessions</returns>
        private async Task<ObservableCollection<Session>> GetInitialSessions()
        {
            var sessions = await _dataService.GetSessionsAsync();
            return new ObservableCollection<Session>(sessions);
        }

        /// <summary>
        /// Method to assign fetched Sessions to the 
        /// Histories List to be displayed in the UI
        /// </summary>
        public async Task SetHistory()
        {
            try
            {
                OriginalSessionsList = await GetInitialSessions();
                foreach (Session session in OriginalSessionsList)
                {
                    if (session.AllModelsSyncedToServer)
                    {
                        session.SyncedIconFillColor = "#00997E"; //Green
                        session.SyncedIconGlyph = "\uE73E"; //Check
                    }
                    else
                    {
                        session.SyncedIconFillColor = "#68718C"; //Gray
                        session.SyncedIconGlyph = "\uE894"; //Cross
                    }
                }
                Histories = OriginalSessionsList;
                IsFirstLoad = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }



        public async void Filter()
        {
            try
            {
                IQueryable<Session> query = OriginalSessionsList.AsQueryable();
                if (!string.IsNullOrEmpty(FilterSearchText))
                    query = query.Where(n => (n.SessionNumber != null && n.SessionNumber.ToLower().Contains(FilterSearchText.ToLower()))
                    || (n.SourceLanguage != null && n.SourceLanguage.ToLower().Contains(FilterSearchText.ToLower()))
                    || (n.TargeLanguage != null && n.TargeLanguage.ToLower().Contains(FilterSearchText.ToLower()))
                    || (n.SessionName != null && n.SessionName.ToLower().Contains(FilterSearchText.ToLower()))
                    || (n.CustomTags != null && n.CustomTags.ToLower().Contains(FilterSearchText.ToLower())));

                if (SearchFromDate != null)
                    query = query.Where(n => DateTimeUtility.ReturnDateTimeFromlongSeconds(n.StartTime) >= SearchFromDate);
                if (SearchToDate != null)
                    query = query.Where(n => DateTimeUtility.ReturnDateTimeFromlongSeconds(n.EndTime) <= (SearchToDate.Value.AddDays(1)));

                Histories = new ObservableCollection<Session>(query.ToList());
            }
            catch (Exception ex)
            {
                ClearFilter();
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public void ClearFilter()
        {
            FilterSearchText = null;
            SearchFromDate = new DateTimeOffset(DateTimeOffset.Now.Year, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0));
            SearchToDate = DateTimeOffset.Now;
            Histories = OriginalSessionsList;
        }

        private void GetSessionDetails(Session session)
        {
            if (session != null)
            {
                var selectedSession = session.Clone();
                SelectedSession = null;
                ContentFrame.Navigate(typeof(SessionDetailsPage), selectedSession);
            }
        }

        private void HandleMessage(NavigationMessage message)
        {
            if (message.LoadSessionHistory)
            {
                ContentFrame.Navigate(typeof(SessionHistoy));
            }
        }

        public async Task ExportSessions()
        {
            try
            {
                var user = await _settings.GetUser();
                IsBusy = Visibility.Visible;
                await Task.Delay(2000);
                await DataExportHelper.GetExcelFile().ConfigureAwait(true);
                IsBusy = Visibility.Collapsed;
                _appAnalytics.CaptureCustomEvent("History Page Events",
                   new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Sessions Exported" }
                   });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task ShareSessions()
        {
            try
            {
                var user = await _settings.GetUser();

                IsBusy = Visibility.Visible;
                await Task.Delay(2000);

                File = await DataExportHelper.ShareFile().ConfigureAwait(true);

                if (File != null)
                {
                    IsFileShare = true;
                    DataTransferManager.ShowShareUI();
                }

                IsBusy = Visibility.Collapsed;

                _appAnalytics.CaptureCustomEvent("History Page Events",
                   new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Sessions Shared" }
                   });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private RelayCommand _searchCommand = null;
        public RelayCommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(() => { Filter(); }));
            }
        }

        private RelayCommand _clearSearchCommand = null;
        public RelayCommand ClearSearchCommand
        {
            get
            {
                return _clearSearchCommand ?? (_clearSearchCommand = new RelayCommand(() => { ClearFilter(); }));
            }
        }

        private RelayCommand _exportCommand = null;
        public RelayCommand ExportCommand
        {
            get
            {
                return _exportCommand ?? (_exportCommand = new RelayCommand(async () => { await ExportSessions(); }));
            }
        }

        private RelayCommand _shareCommand = null;
        public RelayCommand ShareCommand
        {
            get
            {
                return _shareCommand ?? (_shareCommand = new RelayCommand(async () => { await ShareSessions(); }));
            }
        }
    }
}
