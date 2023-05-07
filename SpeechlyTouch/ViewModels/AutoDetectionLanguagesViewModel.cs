using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.Audio;
using SpeechlyTouch.Services.CognitiveService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OutputDevice = SpeechlyTouch.Core.Domain.OutputDevice;

namespace SpeechlyTouch.ViewModels
{
    public class AutoDetectionLanguagesViewModel : ObservableObject
    {
        private LanguageFlag _selectedLanguageFlag;
        public LanguageFlag SelectedLanguageFlag
        {
            get { return _selectedLanguageFlag; }
            set { SetProperty(ref _selectedLanguageFlag, value); }
        }

        private ObservableCollection<Language> _languages;
        public ObservableCollection<Language> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                SetProperty(ref _selectedLanguage, value);
                TranslateText();
            }
        }

        private string _languageIdQuestion;
        public string LanguageIdQuestion
        {
            get { return _languageIdQuestion; }
            set { SetProperty(ref _languageIdQuestion, value); }
        }

        private bool _isLanguageTextLoading = false;
        private CognitiveEndpoint _cognitiveEndpoint = null;
        public bool IsLanguageTextLoading
        {
            get { return _isLanguageTextLoading; }
            set { SetProperty(ref _isLanguageTextLoading, value); }
        }

        private readonly IMicrosoftTextToTextTranslator _microsoftTextToTextTranslator;
        private readonly IMicrosoftStandardVoiceSynthesizer _microsoftStandardVoiceSynthesizer;
        private readonly ICognitiveServicesHelper _cognitiveServicesHelper;
        private readonly IAudioService _audioService;

        public AutoDetectionLanguagesViewModel(
            IMicrosoftTextToTextTranslator microsoftTextToTextTranslator,
            ICognitiveServicesHelper cognitiveServicesHelper,
            IMicrosoftStandardVoiceSynthesizer microsoftStandardVoiceSynthesizer,
            IAudioService audioService)
        {
            _microsoftTextToTextTranslator = microsoftTextToTextTranslator;
            _cognitiveServicesHelper = cognitiveServicesHelper;
            _microsoftStandardVoiceSynthesizer = microsoftStandardVoiceSynthesizer;
            _audioService = audioService;
            StrongReferenceMessenger.Default.Register<AutoDetectMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<LanguageMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            Languages = new ObservableCollection<Language>();
            LanguageIdQuestion = "Is English your language?";
        }

        private async void HandleMessage(LanguageMessage message)
        {
            if (message != null && message.UpdateLanguages)
            {
                await Task.Delay(2000); // This delay allows the flag dialog to update approved languages before navigating
                GoToFlags();
            }
        }

        private async void TranslateText()
        {
            IsLanguageTextLoading = true;

            //Check if the endpoint from the previous session was deallocated. If true, deallocate it and delete from database
            await _cognitiveServicesHelper.DeleteCognitiveServicesEndpointId();
            //Allocate new endpoint for the session and save endpoint Id to database
            _cognitiveEndpoint = await _cognitiveServicesHelper.GetAccessKeyAndRegionAsync();

            var targetLanguageCode = SelectedLanguage.Code.Substring(0, 2);

            var languageQuestion = await _microsoftTextToTextTranslator.TranslateTextToText(_cognitiveEndpoint.AccessKey, _cognitiveEndpoint.Region, "en", $"Is {SelectedLanguage.DisplayName} your language?", targetLanguageCode);

            if (!string.IsNullOrEmpty(languageQuestion))
                LanguageIdQuestion = languageQuestion;

            IsLanguageTextLoading = false;
        }

        private async Task SyntheziseText()
        {
            OutputDevice outputDevice = null;
            var _participantOneOutputDevice = await _audioService.ParticipantOneOutputDevice();

            var _participantTwoOutputDevice = await _audioService.ParticipantTwoOutputDevice();

            if (_participantTwoOutputDevice == null)
            {
                outputDevice = _participantOneOutputDevice;
            }
            else
            {
                outputDevice = _participantTwoOutputDevice;
            }
            var audioResult = await _microsoftStandardVoiceSynthesizer.SynthesizeText(SelectedLanguage.Code, LanguageIdQuestion, _cognitiveEndpoint.AccessKey, _cognitiveEndpoint.Region, outputDevice);
        }

        private void HandleMessage(AutoDetectMessage message)
        {
            if (message.GoToLanguages && message.LanguageFlag != null)
            {
                SelectedLanguageFlag = message.LanguageFlag;
                Languages = message.LanguageFlag.Languages;
                SelectedLanguage = Languages.Count > 0 ? Languages[0] : null;
            }
        }

        private void GoToFlags()
        {
            //Send Message To Navigate
            StrongReferenceMessenger.Default.Send(new AutoDetectMessage { GoToFlags = true });

        }

        private void SelectNextLanguage()
        {
            var currentIndex = Languages.IndexOf(SelectedLanguage);
            var sizeOfList = Languages.Count();
            if (currentIndex == sizeOfList - 1)
            {
                SelectedLanguage = Languages.FirstOrDefault();
            }
            else if (currentIndex < sizeOfList - 1)
            {
                SelectedLanguage = Languages.ElementAtOrDefault(currentIndex + 1);
            }
        }

        private void StartTranslation()
        {
            List<string> candidateLanguage = new List<string>();
            candidateLanguage.Add(SelectedLanguage.Code);

            StrongReferenceMessenger.Default.Send(new AutoDetectMessage { StartTranslation = true, candidateLanguages = candidateLanguage, CloseAutoDetectPopups = true });
        }

        private void CloseWizard()
        {
            StrongReferenceMessenger.Default.Send(new AutoDetectMessage { CloseAutoDetectPopups = true });
        }

        private RelayCommand _goToFlagsCommand = null;
        public RelayCommand GoToFlagsCommand
        {
            get
            {
                return _goToFlagsCommand ?? (_goToFlagsCommand = new RelayCommand(() => { GoToFlags(); }));
            }
        }

        private RelayCommand _playbackCommand = null;
        public RelayCommand PlaybackCommand
        {
            get
            {
                return _playbackCommand ?? (_playbackCommand = new RelayCommand(async () => { await SyntheziseText(); }));
            }
        }

        private RelayCommand _selectNextLanguageCommand = null;
        public RelayCommand SelectNextLanguageCommand
        {
            get
            {
                return _selectNextLanguageCommand ?? (_selectNextLanguageCommand = new RelayCommand(() => { SelectNextLanguage(); }));
            }
        }

        private RelayCommand _startTranslationCommand = null;
        public RelayCommand StartTranslationCommand
        {
            get
            {
                return _startTranslationCommand ?? (_startTranslationCommand = new RelayCommand(() => { StartTranslation(); }));
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
    }
}
