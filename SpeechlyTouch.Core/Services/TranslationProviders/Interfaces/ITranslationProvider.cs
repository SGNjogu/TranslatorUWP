using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    public interface ITranslationProvider : IDisposable
    {
        /// <summary>
        /// Invoked when speech recognizition starts
        /// </summary>
        event Action<TranslationResult> PartialResultReady;

        /// <summary>
        /// Invoked when speech is recognized
        /// </summary>
        event Action<TranslationResult> TranscriptionResultReady;

        /// <summary>
        /// Invoked when speech translated text is sythesized
        /// </summary>
        event Action<TranslationResult> TranslationSpeechReady;

        /// <summary>
        /// Invoked when transcription and translation is complete
        /// </summary>
        event Action<TranslationResult> FinalResultReady;

        /// <summary>
        /// Invoked when translation session is cancelled
        /// </summary>
        event Action<TranslationCancelled> TranslationCancelled;

        /// <summary>
        /// Invoked when input device data is available
        /// </summary>
        event AudioInputDataAvailable InputDataAvailable;

        /// <summary>
        /// Unique identifier for this provider
        /// </summary>
        Guid Guid { get; set; }

        void PauseSpeechToTextClient();

        /// <summary>
        /// Start a translation session
        /// </summary>
        /// <returns>Returns true if session starts successfully or false if otherwise</returns>
        Task<bool> StartTranslationAsync(bool allowExplicitContent, bool enableAudioEnhancement, List<string> candidateLanguages = null);

        /// <summary>
        /// Stop translation session
        /// </summary>
        /// <returns>Returns true if session stops successfully or false if otherwise</returns>
        Task<bool> StopTranslationAsync();

        /// <summary>
        /// Forces Stop by disposing objects
        /// </summary>
        void ForceStop();
        Task<bool> StartAutoDetectTranslationAsync(bool allowExlicitContent, bool enableAudioEnhancement, List<string> candidateLanguages = null);

        Task<bool> StopAutoDetectTranslationAsync();
        void AutoDetectForceStop();
    }
}
