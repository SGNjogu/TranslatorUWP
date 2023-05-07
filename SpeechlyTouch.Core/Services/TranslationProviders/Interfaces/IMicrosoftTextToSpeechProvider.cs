using System;
using System.Threading.Tasks;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    public interface IMicrosoftTextToSpeechProvider
    {
        event Action<TranslationResult> TranscriptionResultReady;
        event Action<TranslationResult> TranslationSpeechReady;

        Task Translate(string apiKey, string apiRegion, string sourceLanguageCode, string textToTranslate, string targetLanguageCode, string participantOneLanguageCode, Participant participant);
    }
}