using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Services.AudioInput;
using SpeechlyTouch.Core.Services.Languages;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.Voices;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public class GoogleProvider : IProvider
    {
        private readonly ILanguagesService _languageService;
        private readonly IVoicesService _voicesService;
        private readonly string _jsonCredentials;
        private IEnumerable<Language> _languages;
        private IEnumerable<Voice> _voices;

        private readonly string _apiKey;
        private readonly string _region;

        public TranslationServiceProvider Provider { get; set; } = TranslationServiceProvider.Microsoft;

        public GoogleProvider(
            ILanguagesService languageService,
            IVoicesService voicesService,
            string jsonCredentials,
            string apiKey,
            string region
            )
        {
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
            _voicesService = voicesService ?? throw new ArgumentNullException(nameof(voicesService));
            _jsonCredentials = jsonCredentials ?? throw new ArgumentNullException(nameof(jsonCredentials));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        public ITranslationProvider CreateTranslationProvider(Language sourceLanguage, Language targetLanguage, string voiceName, InputDevice inputDevice, IAudioInputService audioInputService)
        {
            try
            {
                if (sourceLanguage is null)
                    throw new ArgumentNullException(nameof(sourceLanguage));

                if (targetLanguage is null)
                    throw new ArgumentNullException(nameof(targetLanguage));

                if (voiceName is null)
                    throw new ArgumentNullException(nameof(voiceName));

                if (inputDevice is null)
                    throw new ArgumentNullException(nameof(inputDevice));

                if (audioInputService is null)
                    throw new ArgumentNullException(nameof(audioInputService));

                return (ITranslationProvider)new GoogleTranslationProvider(sourceLanguage, targetLanguage, voiceName, audioInputService, inputDevice, _jsonCredentials, _region, _apiKey);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Initialize()
        {
            try
            {
                _languages = _languageService.GetSupportedLanguages(Provider);
                _voices = _voicesService.GetSupportedVoices(Provider);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public IEnumerable<Language> ListSupportedLanguages()
        {
            if (_languages == null)
                throw new InvalidOperationException("Provide not initialized. Please call 'Initialize' method before calling this method");

            return _languages;
        }

        public IEnumerable<Voice> ListSupportedVoices()
        {
            if (_voices == null)
                throw new InvalidOperationException("Provide not initialized. Please call 'Initialize' method before calling this method");

            return _voices;
        }
    }
}
