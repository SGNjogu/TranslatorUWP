using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using System;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    public interface IGoogleSpeechToTextClient
    {
        event Action<GoogleSpeechToTextResponse> GoogleSpeechToTextOnTextAvailable;
        event AudioInputDataAvailable InputDataAvailable;
        Task StartSpeechToTextAsync();
        Task<bool> StopTranslationAsync();
        void PauseSpeechToTextClient();
    }
}
