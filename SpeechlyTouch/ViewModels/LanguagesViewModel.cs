using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Languages;
using SpeechlyTouch.Services.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class LanguagesViewModel : ObservableObject
    {
        private List<Language> _languages;

        private ObservableCollection<Language> _defaultLanguages;
        public ObservableCollection<Language> DefaultLanguages
        {
            get { return _defaultLanguages; }
            set { SetProperty(ref _defaultLanguages, value); }
        }

        private ObservableCollection<InternationalizationLanguage> _applicationLanguages;
        public ObservableCollection<InternationalizationLanguage> ApplicationLanguages
        {
            get { return _applicationLanguages; }
            set
            {
                SetProperty(ref _applicationLanguages, value);
                DefaultLanguageChangedVisibility = Visibility.Visible;
            }
        }

        private ObservableCollection<Language> _quickViewLanguages;
        public ObservableCollection<Language> QuickViewLanguages
        {
            get { return _quickViewLanguages; }
            set { SetProperty(ref _quickViewLanguages, value); }
        }

        private Language _selectedTranslationDefaultLanguage;
        public Language SelectedTranslationDefaultLanguage
        {
            get { return _selectedTranslationDefaultLanguage; }
            set
            {
                SetProperty(ref _selectedTranslationDefaultLanguage, value);
                if ( SelectedTranslationDefaultLanguage?.Code != null && LanguagesInitialized)
                {
                    if(_settings.DefaultTranslationLanguageCode != SelectedTranslationDefaultLanguage.Code)
                    {
                        StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = false, SettingsType = SettingsType.Language });
                    }
                  
                }
            }
        }

        private bool _isEnabledAutoSwitch;
        public bool IsEnabledAutoSwitch
        {
            get { return _isEnabledAutoSwitch; }
            set
            {
                SetProperty(ref _isEnabledAutoSwitch, value);
                if (_settings != null)
                {
                    if (LanguagesInitialized && (IsEnabledAutoSwitch != _settings.IsEnabledAutoLanguageSwitch))
                    {
                        StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("UserFlagSwitch") });
                        _settings.IsEnabledAutoLanguageSwitch = IsEnabledAutoSwitch;
                    }
                    _settings.IsEnabledAutoLanguageSwitch = IsEnabledAutoSwitch;
                }
            }
        }

        private InternationalizationLanguage _selectedApplicationLanguage;
        public InternationalizationLanguage SelectedApplicationLanguage
        {
            get { return _selectedApplicationLanguage; }
            set
            {
                SetProperty(ref _selectedApplicationLanguage, value);
                if (!string.IsNullOrEmpty(SelectedApplicationLanguage?.Code))
                {
                    if (SelectedApplicationLanguage.Code != _selectedAppLanguageCode)
                        IsAppLanguageUpdated = true;
                    else
                        IsAppLanguageUpdated = false;
                }
                if(SelectedApplicationLanguage != null)
                {
                    if (LanguagesInitialized && (_settings.ApplicationLanguageCode != SelectedApplicationLanguage?.Code))
                    {
                        StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = false, SettingsType = SettingsType.Language });
                    }
                }
            }
        }

        private ObservableCollection<Language> _selectedQuickViewLanguages;
        public ObservableCollection<Language> SelectedQuickViewLanguages
        {
            get { return _selectedQuickViewLanguages; }
            set { SetProperty(ref _selectedQuickViewLanguages, value); }
        }

        private Language _selectedQuickViewLanguage;
        public Language SelectedQuickViewLanguage
        {
            get { return _selectedQuickViewLanguage; }
            set
            {
                SetProperty(ref _selectedQuickViewLanguage, value);
                if (SelectedQuickViewLanguage != null)
                    QuickViewLanguageAdded();
            }
        }

        private Visibility _defaultLanguageChangedVisibility;
        public Visibility DefaultLanguageChangedVisibility
        {
            get { return _defaultLanguageChangedVisibility; }
            set
            {
                SetProperty(ref _defaultLanguageChangedVisibility, value);
            }
        }

        private ObservableCollection<string> _defaultPlaybackLanguages;
        public ObservableCollection<string> DefaultPlaybackLanguages
        {
            get { return _defaultPlaybackLanguages; }
            set
            {
                SetProperty(ref _defaultPlaybackLanguages, value);
            }
        }

        private string _selectedDefaultPlaybackLanguage;
        public string SelectedDefaultPlaybackLanguage
        {
            get { return _selectedDefaultPlaybackLanguage; }
            set
            {
                SetProperty(ref _selectedDefaultPlaybackLanguage, value);
                if (_settings.DefaultPlaybackLanguage != SelectedDefaultPlaybackLanguage)
                {
                    StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = false, SettingsType = SettingsType.Language });
                }
            }
        }

        private bool _languagesInitialized;
        public bool LanguagesInitialized
        {
            get { return _languagesInitialized; }
            set
            {
                SetProperty(ref _languagesInitialized, value);
            }
        }

        private bool _changesSaved = false;
        public bool ChangesSaved
        {
            get { return _changesSaved; }
            set
            {
                SetProperty(ref _changesSaved, value);
            }
        }

        private ResourceLoader _resourceLoader;
        private readonly ILanguagesService _languagesService;
        private readonly ISettingsService _settings;
        private readonly IDataService _dataService;
        private readonly IAppAnalytics _appAnalytics;

        private bool IsAppLanguageUpdated { get; set; }
        private bool IsQuickViewLanguagesUpdated { get; set; }
        private string _selectedAppLanguageCode { get; set; }

        public LanguagesViewModel(ILanguagesService languagesService, ISettingsService settings, IDataService dataService, IAppAnalytics appAnalytics)
        {
            _languagesService = languagesService;
            _settings = settings;
            _dataService = dataService;
            _appAnalytics = appAnalytics;
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();

            StrongReferenceMessenger.Default.Register<LanguageMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
           
            SelectedTranslationDefaultLanguage = new Language();
            SelectedApplicationLanguage = new InternationalizationLanguage();
            SelectedQuickViewLanguages = new ObservableCollection<Language>();
            DefaultPlaybackLanguages = new ObservableCollection<string>();
            _ = LoadLanguages();
            DefaultLanguageChangedVisibility = Visibility.Collapsed;
            IsAppLanguageUpdated = false;
          
        }


        private async void HandleMessage(ErrorMessage m)
        {
           
                if (m.SettingsType == Enums.SettingsType.Language && m.EnableNavigation)
                {
                await SaveSettings();
                }
           
        }

        private async void HandleMessage(LanguageMessage message)
        {
            if (message != null && message.UpdateLanguages)
            {
               await LoadLanguages();
            }
        }

        private void GetDefaultPlaybackLanguages()
        {
            var language = _resourceLoader.GetString("Language");
            if(DefaultPlaybackLanguages.Count() == 0 )
            {
                DefaultPlaybackLanguages.Add($"{language} 1");
                DefaultPlaybackLanguages.Add($"{language} 2");
            }
            var defaultPlaybackLanguage = _settings.DefaultPlaybackLanguage;

            if (string.IsNullOrEmpty(defaultPlaybackLanguage))
                _settings.DefaultPlaybackLanguage = DefaultPlaybackLanguages.FirstOrDefault();

            SelectedDefaultPlaybackLanguage = _settings.DefaultPlaybackLanguage;
        }

        public async Task LoadLanguages()
        {
            await Task.Delay(1000);
            _languages = await _languagesService.GetSupportedLanguagesAsync();
            var applicationLanguages = await _dataService.GetInternationalizationLanguages();
            DefaultLanguages = new ObservableCollection<Language>(_languages.OrderBy(l => l.DisplayName));
            ApplicationLanguages = new ObservableCollection<InternationalizationLanguage>(applicationLanguages.OrderBy(s => s.Name));
            QuickViewLanguages = new ObservableCollection<Language>(_languages.OrderBy(l => l.DisplayName));
            GetDefaultLanguages();
            IsEnabledAutoSwitch = _settings.IsEnabledAutoLanguageSwitch;
            LanguagesInitialized = true;
        }

        private async void GetDefaultLanguages()
        {
            GetDefaultPlaybackLanguages();

            string defaultLanguageCode = _settings.DefaultTranslationLanguageCode;

            if (string.IsNullOrEmpty(defaultLanguageCode))
                defaultLanguageCode = "en-GB";

            _settings.DefaultTranslationLanguageCode = defaultLanguageCode;
            SelectedTranslationDefaultLanguage = DefaultLanguages.FirstOrDefault(s => s.Code == defaultLanguageCode);

            string applicationLanguageCode = _settings.ApplicationLanguageCode;

            if (string.IsNullOrEmpty(applicationLanguageCode))
                applicationLanguageCode = "en";

            InternationalizationLanguage selectedAppLanguage = ApplicationLanguages.FirstOrDefault(s => s.Code == applicationLanguageCode);

            if (selectedAppLanguage != null)
            {
                _selectedAppLanguageCode = selectedAppLanguage.Code;
                SelectedApplicationLanguage = selectedAppLanguage;
            }
            else
            {
                _selectedAppLanguageCode = "en";
                SelectedApplicationLanguage = new InternationalizationLanguage
                {
                    Code = "en",
                    Name = "English",
                    NativeName = "English"
                };
                if (!ApplicationLanguages.Any())
                {
                    ApplicationLanguages = new ObservableCollection<InternationalizationLanguage>();
                    ApplicationLanguages.Add(SelectedApplicationLanguage);
                }
            }

            string quickViewLanguages = _settings.QuickViewLanguages;

            if (string.IsNullOrEmpty(quickViewLanguages))
            {
                var quickViewList = new List<Language>()
                {
                    QuickViewLanguages.FirstOrDefault(l => l.Code == "fr-FR"),
                    QuickViewLanguages.FirstOrDefault(l => l.Code == "es-ES"),
                    QuickViewLanguages.FirstOrDefault(l => l.Code == "it-IT"),
                    QuickViewLanguages.FirstOrDefault(l => l.Code == "nl-NL"),
                    QuickViewLanguages.FirstOrDefault(l => l.Code == "sv-SE"),
                };

                quickViewList.RemoveAll(l => l == null);

                SelectedQuickViewLanguages = new ObservableCollection<Language>(quickViewList);
            }
            else
            {
                var quickLanguagesList = await JsonConverter.ReturnObjectFromJsonString<List<string>>(quickViewLanguages);
                GetQuickViewLanguages(quickLanguagesList);
            }
        }

        private void GetQuickViewLanguages(List<string> quickViewLanguagesList)
        {
            SelectedQuickViewLanguages = new ObservableCollection<Language>();

            foreach (var item in quickViewLanguagesList)
            {
                var language = _languages.Find(s => s.Code == item);

                if (language != null)
                    SelectedQuickViewLanguages.Add(language);
            }
        }

        private void QuickViewLanguageAdded()
        {
            IsQuickViewLanguagesUpdated = true;

            if (SelectedQuickViewLanguages.Any())
            {
                var existingLanguage = SelectedQuickViewLanguages.FirstOrDefault(c => c.Code == SelectedQuickViewLanguage.Code);

                if (existingLanguage == null)
                {
                    SelectedQuickViewLanguages.Insert(0, SelectedQuickViewLanguage);
                    if (SelectedQuickViewLanguages.Count > 5)
                        SelectedQuickViewLanguages.RemoveAt(SelectedQuickViewLanguages.Count - 1);
                   
                }
            }

            SelectedQuickViewLanguage = null;
        }

        private async Task SaveSettings()
        {
            var user = await _settings.GetUser();
            if (_settings.DefaultTranslationLanguageCode != SelectedTranslationDefaultLanguage.Code &&
              _settings.DefaultPlaybackLanguage != SelectedDefaultPlaybackLanguage &&
              _settings.ApplicationLanguageCode != SelectedApplicationLanguage.Code)
            {
                _settings.DefaultTranslationLanguageCode = SelectedTranslationDefaultLanguage.Code;
                _settings.DefaultPlaybackLanguage = SelectedDefaultPlaybackLanguage;
                _settings.ApplicationLanguageCode = SelectedApplicationLanguage.Code;
                ChangesSaved=true;
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                StrongReferenceMessenger.Default.Send(new InternationalizationMessage { LanguageCode = SelectedApplicationLanguage.Code });
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("LanguageSettingsUpdated") });
            }
            if (!ChangesSaved && (_settings.DefaultTranslationLanguageCode != SelectedTranslationDefaultLanguage.Code && _settings.DefaultPlaybackLanguage != SelectedDefaultPlaybackLanguage))
            {
                _settings.DefaultTranslationLanguageCode = SelectedTranslationDefaultLanguage.Code;
                _settings.DefaultPlaybackLanguage = SelectedDefaultPlaybackLanguage;
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("TranslationPlaybackUpdated") });
            }
            if (!ChangesSaved && (_settings.DefaultTranslationLanguageCode != SelectedTranslationDefaultLanguage.Code && _settings.DefaultPlaybackLanguage == SelectedDefaultPlaybackLanguage))
            {
                _settings.DefaultTranslationLanguageCode = SelectedTranslationDefaultLanguage.Code;
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("TranslationLanguageUpdated") });
            }
            if (!ChangesSaved && (_settings.DefaultPlaybackLanguage != SelectedDefaultPlaybackLanguage && _settings.DefaultTranslationLanguageCode == SelectedTranslationDefaultLanguage.Code))
            {
                _settings.DefaultPlaybackLanguage = SelectedDefaultPlaybackLanguage;
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("PlaybackLanguageUpdated") });

            }


            if (IsQuickViewLanguagesUpdated)
            {
                var quickViewLanguages = new List<string>();
                foreach (var item in SelectedQuickViewLanguages)
                {
                    quickViewLanguages.Add(item.Code);
                }
                var quickLanguages = await JsonConverter.ReturnJsonStringFromObject(quickViewLanguages);
                _settings.QuickViewLanguages = quickLanguages;
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("QuickViewLanguagesUpdated") });
            }

            if (_settings.ApplicationLanguageCode != SelectedApplicationLanguage.Code)
            {
                _settings.ApplicationLanguageCode = SelectedApplicationLanguage.Code;
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                StrongReferenceMessenger.Default.Send(new InternationalizationMessage { LanguageCode = SelectedApplicationLanguage.Code });
            }

            _appAnalytics.CaptureCustomEvent("Settings Changes",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Language settings updated" }
                    });
            ChangesSaved = false;
            StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadQuickViewLanguages = true });
        }

        private RelayCommand _saveCommand = null;
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(async () => { await SaveSettings(); }));
            }
        }
    }
}
