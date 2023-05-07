using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.Languages;
using SpeechlyTouch.Services.Settings;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpeechlyTouch.ViewModels
{
    public class InitialSetupLanguagesViewModel : ObservableObject
    {
        private ObservableCollection<Language> _defaultLanguages;
        public ObservableCollection<Language> DefaultLanguages
        {
            get { return _defaultLanguages; }
            set { SetProperty(ref _defaultLanguages, value); }
        }

        private Language _selectedDefaultLanguage;
        public Language SelectedDefaultLanguage
        {
            get { return _selectedDefaultLanguage; }
            set { SetProperty(ref _selectedDefaultLanguage, value); }
        }

        private readonly ISettingsService _settingsService;
        private readonly ILanguagesService _languagesService;
        private readonly IDataService _dataService;

        public InitialSetupLanguagesViewModel(ISettingsService settingsService, ILanguagesService languagesService, IDataService dataService)
        {
            _settingsService = settingsService;
            _languagesService = languagesService;
            _dataService = dataService;

            StrongReferenceMessenger.Default.Register<LanguageMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            Initialize();
        }

        private void HandleMessage(LanguageMessage message)
        {
            if (message != null && message.UpdateLanguages)
            {
                Initialize();
            }
        }

        private async void Initialize()
        {
            var languages = await _languagesService.GetSupportedLanguagesAsync().ConfigureAwait(true);

            if (languages.Any())
            {
                DefaultLanguages = new ObservableCollection<Language>(languages.OrderBy(c => c.DisplayName));
            }
            else
            {
                DefaultLanguages = new ObservableCollection<Language>();
            }

            var organizationSettings = await _dataService.GetOrganizationSettingsAsync();
            string setLanguageCode = string.Empty;
            if (organizationSettings.Any())
            {
                var orgCode = organizationSettings[0]?.LanguageCode;
                setLanguageCode = languages.Exists(c => c.Code.ToLower() == orgCode.ToLower()) ? organizationSettings[0]?.LanguageCode : DefaultLanguages.FirstOrDefault().Code;
            }

            string defaultLanguageCode = string.Empty;

            if (!string.IsNullOrEmpty(setLanguageCode) && setLanguageCode != "string")
                defaultLanguageCode = setLanguageCode;

            if (string.IsNullOrEmpty(defaultLanguageCode))
                defaultLanguageCode = languages.FirstOrDefault().Code;

            _settingsService.DefaultTranslationLanguageCode = defaultLanguageCode;
            SelectedDefaultLanguage = DefaultLanguages.FirstOrDefault(s => s.Code == defaultLanguageCode);

            string applicationLanguage = _settingsService.ApplicationLanguageCode;
        }

        private void SaveSettings()
        {
            _settingsService.DefaultTranslationLanguageCode = SelectedDefaultLanguage?.Code;
            StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadPasscodeInitialSetup = true });
        }

        private RelayCommand _moveToPasscodeSetupCommand = null;
        public RelayCommand MoveToPasscodeSetupCommand
        {
            get
            {
                return _moveToPasscodeSetupCommand ?? (_moveToPasscodeSetupCommand = new RelayCommand(() => { SaveSettings(); }));
            }
        }
    }
}
