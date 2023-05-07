using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.Messages
{
    public class OrgQuestionsMessage
    {
        public bool ReloadQuestions { get; set; }
        public string TranslateQuestion { get; set; }
        public string LanguageCode { get; set; }
        public bool ResetSelection { get; set; }
    }
}
