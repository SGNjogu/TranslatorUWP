using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SpeechlyTouch.Messages
{
    public class DownloadMessage
    {
        public Uri Uri { get; set; }
        public string SessionNumber { get; set; }
        public Visibility DownloadButtonVisibility { get; set; }
    }
}
