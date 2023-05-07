using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    public interface ITranslationHub
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
        /// Start translation
        /// </summary>
        /// <param name="participants">Collection of participants</param>
        /// <param name="apiKey">Cognitive endpoint access key</param>
        /// <param name="region">Cognitive endpoint region</param>
        /// <param name="waveFilePath">Path to store audio file created during translation session</param>
        /// <returns></returns>
        Task StartTranslationAsync(IEnumerable<Participant> participants, string apiKey, string region, bool isSingleDevice, string waveFilePath = null, string waveFilePath_Translated = null, List<string> candidateLanguages = null);

        /// <summary>
        /// Stop translation session
        /// </summary>
        /// <returns></returns>
        Task<bool> StopTranslationAsync();

        /// <summary>
        /// Swith active participant
        /// Use this method switch source and target languages
        /// </summary>
        /// <param name="to">Participant whose language will be set to target language</param>
        Task Switch(Participant to);

        /// <summary>
        /// Forces Ending of a Session by disposing objects
        /// </summary>
        /// <returns></returns>
        void ForceStop();

        /// <summary>
        /// Sets the profanity level for the translation provider
        /// </summary>
        bool AllowExplicitContent { get; set; }
        bool EnableAudioEnhancement { get; set; }
    }
}
