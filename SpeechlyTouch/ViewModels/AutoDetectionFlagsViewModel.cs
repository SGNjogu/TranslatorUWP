using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.FlagLanguage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using CoreLanguageService = SpeechlyTouch.Core.Services.Languages.ILanguagesService;

namespace SpeechlyTouch.ViewModels
{
    public class AutoDetectionFlagsViewModel : ObservableObject
    {
        private ObservableCollection<LanguageFlag> _languageFlags;
        public ObservableCollection<LanguageFlag> LanguageFlags
        {
            get { return _languageFlags; }
            set { SetProperty(ref _languageFlags, value); }
        }

        private LanguageFlag _selectedLanguageFlag;
        public LanguageFlag SelectedLanguageFlag
        {
            get { return _selectedLanguageFlag; }
            set
            {
                SetProperty(ref _selectedLanguageFlag, value);
                ValidateAutoDetection();
            }
        }

        private string _nextButtonText;
        public string NextButtonText
        {
            get { return _nextButtonText; }
            set { SetProperty(ref _nextButtonText, value); }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }

        private List<LanguageFlag> OriginalFlagsList;
        private List<Core.Domain.Language> OriginalAutoDetectLanguages;

        private readonly IFlagLanguageService _flagLanguageService;
        private readonly CoreLanguageService _coreLanguageService;
        private ResourceLoader _resourceLoader;

        public AutoDetectionFlagsViewModel(IFlagLanguageService flagLanguageService, CoreLanguageService coreLanguageService)
        {
            _flagLanguageService = flagLanguageService;
            _coreLanguageService = coreLanguageService;
            NextButtonText = "Next";
            OriginalFlagsList = new List<LanguageFlag>();
            OriginalAutoDetectLanguages = new List<Core.Domain.Language>();
            _resourceLoader = ResourceLoader.GetForCurrentView();

            StrongReferenceMessenger.Default.Register<LanguageMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            Initialize();
        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                Initialize();
            }
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
            OriginalFlagsList = await _flagLanguageService.GetFlagLanguages();
            LanguageFlags = new ObservableCollection<LanguageFlag>(OriginalFlagsList);
            SelectedLanguageFlag = LanguageFlags.Count > 0 ? LanguageFlags[0] : null;
        }

        private void ValidateAutoDetection()
        {
            var canDetect = EnableAutoDetect();
            NextButtonText = canDetect ? _resourceLoader.GetString("TranslateText") : _resourceLoader.GetString("NextText");
        }

        private bool EnableAutoDetect()
        {
            if (OriginalAutoDetectLanguages == null || !OriginalAutoDetectLanguages.Any())
            {
                OriginalAutoDetectLanguages = _coreLanguageService.GetAutoDetectSupportedLanguages().ToList();
            }

            if (SelectedLanguageFlag == null) return false;

            var selectedLanguages = SelectedLanguageFlag.Languages;
            List<bool> checksForDetection = new List<bool>();

            foreach (var language in selectedLanguages)
            {
                if (OriginalAutoDetectLanguages.Any(s => s.Code == language.Code))
                    checksForDetection.Add(true);
                else
                    checksForDetection.Add(false);
            }

            if (checksForDetection.Any(c => !c))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void GoToLanguages()
        {
            //Translate for auto-detectable languages
            var canDetect = EnableAutoDetect();
            if (canDetect)
            {
                List<string> candidateLanguages = new List<string>();
                foreach (var language in SelectedLanguageFlag.Languages)
                {
                    candidateLanguages.Add(language.Code);
                }
                StrongReferenceMessenger.Default.Send(new AutoDetectMessage { StartTranslation = true, candidateLanguages = candidateLanguages, CloseAutoDetectPopups = true });
            }
            else
            {
                //Send Message To Navigate
                StrongReferenceMessenger.Default.Send(new AutoDetectMessage { GoToLanguages = true, LanguageFlag = SelectedLanguageFlag });
            }
        }

        private void SearchFlag()
        {
            if (OriginalFlagsList == null || !OriginalFlagsList.Any()) return;

            if (string.IsNullOrEmpty(SearchText))
            {
                LanguageFlags = new ObservableCollection<LanguageFlag>(OriginalFlagsList);
                SelectedLanguageFlag = LanguageFlags.Count > 0 ? LanguageFlags[0] : null;
            }
            else
            {
                var filteredList = OriginalFlagsList.FindAll(l => l.CountryName.ToLower().Contains(SearchText.ToLower()));
                LanguageFlags = new ObservableCollection<LanguageFlag>(filteredList);
                SelectedLanguageFlag = LanguageFlags.Count > 0 ? LanguageFlags[0] : null;
            }
        }

        private void ResetSearch()
        {
            SearchText = string.Empty;

            if (OriginalFlagsList == null || !OriginalFlagsList.Any()) return;

            if (string.IsNullOrEmpty(SearchText))
            {
                LanguageFlags = new ObservableCollection<LanguageFlag>(OriginalFlagsList);
                SelectedLanguageFlag = LanguageFlags.Count > 0 ? LanguageFlags[0] : null;
            }
        }

        private void CloseWizard()
        {
            StrongReferenceMessenger.Default.Send(new AutoDetectMessage { CloseAutoDetectPopups = true });
        }

        private RelayCommand _goTolanguagesCommand = null;

        public RelayCommand GoTolanguagesCommand
        {
            get
            {
                return _goTolanguagesCommand ?? (_goTolanguagesCommand = new RelayCommand(() => { GoToLanguages(); }));
            }
        }

        private RelayCommand _closeWizardCommand = null;
        public RelayCommand CloseWizardCommand
        {
            get
            {
                return _closeWizardCommand ?? (_closeWizardCommand = new RelayCommand(() => { CloseWizard(); }));
            }
        }

        private RelayCommand _searchCommand = null;
        public RelayCommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(() => { SearchFlag(); }));
            }
        }

        private RelayCommand _resetSearchCommand = null;
        public RelayCommand ResetSearchCommand
        {
            get
            {
                return _resetSearchCommand ?? (_resetSearchCommand = new RelayCommand(() => { ResetSearch(); }));
            }
        }
    }
}
