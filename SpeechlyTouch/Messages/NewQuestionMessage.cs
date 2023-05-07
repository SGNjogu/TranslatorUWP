using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.Messages
{
    public class NewQuestionMessage
    {
        public string Question { get; set; }
        public string LanguageCode { get; set; }
    }
}
