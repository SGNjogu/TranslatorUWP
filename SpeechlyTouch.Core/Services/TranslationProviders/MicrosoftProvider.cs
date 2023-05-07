using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Services.AudioInput;
using SpeechlyTouch.Core.Services.Languages;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.Voices;
using System;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public class MicrosoftProvider : IProvider
    {
        private readonly ILanguagesService _languageService;
        private readonly IVoicesService _voicesService;
        private readonly string _apiKey;
        private readonly string _region;
        private IEnumerable<Language> _languages;
        private IEnumerable<Voice> _voices;

        public TranslationServiceProvider Provider { get; set; } = TranslationServiceProvider.Microsoft;

        public MicrosoftProvider(
            ILanguagesService languageService,
            IVoicesService voicesService,
            string apiKey,
            string region)
        {
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
            _voicesService = voicesService ?? throw new ArgumentNullException(nameof(voicesService));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        /// <summary>
        /// Create MicrosoftTranslationProvider
        /// </summary>
        /// <param name="sourceLanguage">Source Language</param>
        /// <param name="targetLanguage">Collection of Target languages</param>
        /// <param name="inputDevice">Instance of <see cref="InputDevice"/></param>
        /// <param name="audioInputService">Instance of <see cref="IAudioInputService"/></param>
        /// <exception cref="ArgumentNullException">Thrown when any of parameter provided is null</exception>
        /// <returns></returns>
        public ITranslationProvider CreateTranslationProvider(
            Language sourceLanguage,
            Language targetLanguage,
            string voiceName,
            InputDevice inputDevice,
            IAudioInputService audioInputService)
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

            return (ITranslationProvider)new MicrosoftTranslationProvider(sourceLanguage, targetLanguage, voiceName, audioInputService, inputDevice, _apiKey, _region);
        }

        /// <summary>
        /// Initialize this provider by setting the supported languages and voices
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            _languages = _languageService.GetSupportedLanguages(Provider);
            _voices = _voicesService.GetSupportedVoices(Provider);
        }

        /// <summary>
        /// Get list of languages supported by this provider
        /// </summary>
        /// <returns>Collection of languages</returns>
        public IEnumerable<Language> ListSupportedLanguages()
        {
            if (_languages == null)
                throw new InvalidOperationException("Provide not initialized. Please call 'Initialize' method before calling this method");

            return _languages;
        }

        /// <summary>
        /// Get list of voices supported by this provider
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Voice> ListSupportedVoices()
        {
            if (_voices == null)
                throw new InvalidOperationException("Provide not initialized. Please call 'Initialize' method before calling this method");

            return _voices;
        }
    }
}
