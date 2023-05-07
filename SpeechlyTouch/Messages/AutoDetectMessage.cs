using SpeechlyTouch.Models;
using System.Collections.Generic;

namespace SpeechlyTouch.Messages
{
    public class AutoDetectMessage
    {
        public bool GoToLanguages { get; set; }
        public bool GoToFlags { get; set; }
        public LanguageFlag LanguageFlag { get; set; }
        public bool StartTranslation { get; set; }
        public List<string> candidateLanguages { get; set; }
        public bool CloseAutoDetectPopups { get; set; }
        public bool TranslationOpen { get; set; }
    }
}
