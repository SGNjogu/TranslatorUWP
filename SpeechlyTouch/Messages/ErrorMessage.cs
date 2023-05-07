using SpeechlyTouch.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.Messages
{
    public class ErrorMessage
    {
        public string ErrorDisplayMessage { get; set; }

        public bool DisplayDialog { get; set; }

        public bool EnableNavigation { get; set; }

        public SettingsType SettingsType { get; set; }
    }
}
