using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace SpeechlyTouch.Models
{
    public class Chat : ObservableObject
    {
        public Guid Guid { get; set; }

        private string _person;
        public string Person
        {
            get { return _person; }
            set { SetProperty(ref _person, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public string OriginalMessage { get; set; }
        public string TranslatedMessage { get; set; }
        public string OriginalMessageISO { get; set; }
        public string TranslatedMessageISO { get; set; }
        public int UploadedSessionId { get; set; }
        public bool IsCertifiedDevice { get; set; }

        private DateTime? _date;
        public DateTime? Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value); }
        }

        public string Sentiment { get; set; }
        public string SentimentEmoji { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsPersonOne { get; set; }
        public bool IsCopyPasteEnabled { get; set; } = true;
        public long OffsetInTicks { get; set; }
        public bool IsComplete { get; set; }
    }
}
