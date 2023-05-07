using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeechlyTouch.Core.Services.Languages
{
    public class MicrosoftLanguagesService : ILanguagesService, IDisposable
    {
        public IEnumerable<Language> GetSupportedLanguages()
        {
            return AzureSpeechLanguages.languages;
        }

        public IEnumerable<Language> GetAutoDetectSupportedLanguages()
        {
            return AzureAutoDetectLanguages.languages;
        }

        public IEnumerable<Language> GetSupportedLanguages(TranslationServiceProvider translationServiceProvider)
        {
            switch (translationServiceProvider)
            {
                case TranslationServiceProvider.Microsoft:
                    return AzureSpeechLanguages.languages;
                default:
                    return new List<Language>();
            }
        }

        /// <exception cref="System.NullReferenceException">Thrown when an item in the languages collection is null</exception>
        /// <returns>Collection of languages with flag property set</returns>
        public IEnumerable<Language> MapFlagsToLanguages(IEnumerable<Language> languages, IEnumerable<LanguageFlag> languageFlags)
        {
            var mappedLanguages = new List<Language>();

            foreach (var language in languages)
            {
                if (language == null)
                    throw new NullReferenceException($"{nameof(language)} is null");

                var languageFlag = languageFlags.FirstOrDefault(lf => lf.LanguageCode == language.Code);

                if (languageFlag != null)
                    language.Flag = languageFlag.FlagUri;

                mappedLanguages.Add(language);
            }

            return mappedLanguages;
        }

        public void Dispose()
        {
        }
    }
}
