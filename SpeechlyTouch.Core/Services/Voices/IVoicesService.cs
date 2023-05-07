using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Services.Voices
{
    public interface IVoicesService
    {
        /// <summary>
        /// Gets supported voices for all translation providers
        /// </summary>
        /// <returns><see cref="IEnumerable{Voice}"/></returns>
        IEnumerable<Voice> GetSupportedVoices();

        /// <summary>
        /// Get supported voices for the specified service provider
        /// </summary>
        /// <param name="translationServiceProvider">Translation Service Provider</param>
        /// <returns><see cref="IEnumerable{Voice}"/></returns>
        IEnumerable<Voice> GetSupportedVoices(TranslationServiceProvider translationServiceProvider);
    }
}
