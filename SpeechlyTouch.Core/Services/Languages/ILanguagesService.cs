using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Services.Languages
{
    public interface ILanguagesService
    {
        /// <summary>
        /// Gets all supported languages
        /// </summary>
        /// <returns></returns>
        IEnumerable<Language> GetSupportedLanguages();

        /// <summary>
        /// Gets all supported languages for the specified provider
        /// </summary>
        /// <param name="translationServiceProvider">Provider</param>
        /// <returns><see cref="IEnumerable{Language}"/></returns>
        IEnumerable<Language> GetSupportedLanguages(TranslationServiceProvider translationServiceProvider);

        /// <summary>
        /// Maps flags Uri to Languages
        /// </summary>
        /// <param name="languages">Collection of Languages to map flags to</param>
        /// <param name="languageFlags">Collection of LanguageFlags</param>
        /// <returns><see cref="IEnumerable{Language}"/></returns>
        IEnumerable<Language> MapFlagsToLanguages(IEnumerable<Language> languages, IEnumerable<LanguageFlag> languageFlags);

        /// <summary>
        /// Gets all languages supported for Auto-detection
        /// </summary>
        /// <returns></returns>
        IEnumerable<Language> GetAutoDetectSupportedLanguages();
    }
}
