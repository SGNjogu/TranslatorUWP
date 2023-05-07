using System;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Events
{
    public class TranslationCancelled
    {
        public Guid Guid { get; set; }

        public string Reason { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDetails { get; set; }
    }
}
