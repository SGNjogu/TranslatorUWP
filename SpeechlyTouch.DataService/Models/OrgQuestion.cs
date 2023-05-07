using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.DataService.Models
{
    public class OrgQuestions : BaseModel
    {
        public string Question { get; set; }
        public string LanguageCode { get; set; }
        public string Shortcut { get; set; } = string.Empty;
        public bool SyncedToServer { get; set; }
        public int QuestionStatus { get; set; }
        public int QuestionID { get; set; }
        public int QuestionType { get; set; }
    }
}
