using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Services.AudioInput;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    /// <summary>
    /// Create a translation provider to model a Cloud Provider e.g Microsoft, Google, Amazon
    /// </summary>
    public interface IProvider
    {
        TranslationServiceProvider Provider { get; set; }

        IEnumerable<Language> ListSupportedLanguages();

        IEnumerable<Voice> ListSupportedVoices();

        void Initialize();

        ITranslationProvider CreateTranslationProvider(Language sourceLanguage, Language targetLanguage, string voiceName, InputDevice inputDevice, IAudioInputService audioInputService);
    }
}
