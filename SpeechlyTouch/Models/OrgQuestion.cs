using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SpeechlyTouch.Models
{
    public class OrgQuestion : ObservableObject, ICloneable
    {
        private string _question;
        public string Question
        {
            get { return _question; }
            set { SetProperty(ref _question, value); }
        }

        public string LanguageCode { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private string _shortCut = string.Empty;
        public string ShortCut
        {
            get { return _shortCut; }
            set { SetProperty(ref _shortCut, value); }
        }

        private Visibility _isVisibleShortCut;
        public Visibility IsVisibleShortCut
        {
            get { return _isVisibleShortCut; }
            set { SetProperty(ref _isVisibleShortCut, value); }
        }

        private int _questionID;
        public int QuestionID
        {
            get { return _questionID; }
            set { SetProperty(ref _questionID, value); }
        }

        private int _questionStatus;
        public int QuestionStatus
        {
            get { return _questionStatus; }
            set { SetProperty(ref _questionStatus, value); }
        }

        private bool _synced;
        public bool Synced
        {
            get { return _synced; }
            set { SetProperty(ref _synced, value); }
        }

        private int _questionType;
        public int QuestionType
        {
            get { return _questionType; }
            set { SetProperty(ref _questionType, value); }

        }
    }
}
