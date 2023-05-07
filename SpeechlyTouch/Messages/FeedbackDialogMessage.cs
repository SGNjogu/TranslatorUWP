using SpeechlyTouch.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.Messages
{
    public class FeedbackDialogMessage
    {
        public bool ShowFeedbackSubmitted { get; set; }

        public bool ShowRatingFeedback { get; set; }

        public bool CloseRatingFeedback { get; set; }

        public bool CloseStarRatingFeedback { get; set; }

        public string SessionNumber { get; set; }

        public int Rating { get; set; }

        public string Feedbacktitle { get; set; }

        public string Feedbackdescription { get; set; }

        public FeedbackType FeedbackType { get; set; }

    }
}
