using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.Core.Domain
{
    public class UserFeedback : BaseEntity
    {
        public string SessionNumber { get; set; }

        public int Rating { get; set; }

        public string ReasonForRating { get; set; }

        public string Comment { get; set; }

        public int FeedbackType { get; set; }
    }
}
