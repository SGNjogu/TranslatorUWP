using System;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Events
{
    public class TranslationResult
    {
        public bool IsPersonOne { get; set; } = false;
        public Guid Guid { get; set; }

        public string TranslatedText { get; set; }

        public string OriginalText { get; set; }

        public string SourceLanguageCode { get; set; }

        public string TargetLanguageCode { get; set; }

        public byte[] AudioResult { get; set; }
        public TimeSpan Duration { get; set; }
        public double WordsPerMinute
        {
            get
            {
                return GetWordsPerMinute(OriginalText, Duration.TotalMinutes);
            }
        }

        private double GetWordsPerMinute(string text, double durationInMinutes)
        {
            double wordsPerMinute = 0.0;
            if (!string.IsNullOrEmpty(text))
            {
                string[] words = text.Split(' ');
                wordsPerMinute = words.Length / durationInMinutes;
            }
            return wordsPerMinute;
        }

        public long OffsetInTicks { get; set; }
        public bool IsComplete { get; set; }
    }
}
