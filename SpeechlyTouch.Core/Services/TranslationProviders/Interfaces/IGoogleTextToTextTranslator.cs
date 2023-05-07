using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using System;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    public interface IGoogleTextToTextTranslator
    {
        event Action<GoogleTextTranslationResponse> GoogleTextTranslationOnTextAvailable;
        Task Initialize();
        Task TranslateAsync
            (
            string originalText,
            string sourceLanguge,
            string targetLanguage,
            long OffsetInTicks,
            TimeSpan duration
            );
    }
}
