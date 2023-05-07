using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.Core.Domain
{
    public class UserQuestion : BaseEntity
    {
        public string Question { get; set; }
        public string LanguageCode { get; set; }
        public int QuestionType { get; set; }
    }
}
