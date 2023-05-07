using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using System.Collections.Generic;

namespace SpeechlyTouch.Core.Services.Voices
{
    public class MicrosoftVoicesService : IVoicesService
    {
        public IEnumerable<Voice> GetSupportedVoices()
        {
            return new List<Voice>();
        }

        public IEnumerable<Voice> GetSupportedVoices(TranslationServiceProvider translationServiceProvider)
        {
            return new List<Voice>();
        }
    }
}
