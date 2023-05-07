using SpeechlyTouch.Core.Services.AudioInput;
using SpeechlyTouch.Core.Services.AudioOutput;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using System;

namespace SpeechlyTouch.Core.Domain
{
    public class Participant
    {
        public Guid Guid { get; set; }

        public AudioProfile AudioProfile { get; set; }

        public Language SelectedLanguage { get; set; }

        public ITranslationProvider TranslationProvider { get; set; }

        public IAudioOutputService AudioOutputService { get; set; }

        public IAudioInputService AudioInputService { get; set; }
    }
}
