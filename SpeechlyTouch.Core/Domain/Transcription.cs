using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.Core.Domain
{
    public class Transcription : BaseEntity
    {
        public int SessionId { get; set; }
        public string ChatUser { get; set; }
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public DateTime ChatTime { get; set; }
        public string Sentiment { get; set; }
        public bool SyncedToServer { get; set; } = false;
        public Guid Guid { get; set; }
        public string Timestamp { get; set; }
        public double TranslationSeconds { get; set; }
    }
}
