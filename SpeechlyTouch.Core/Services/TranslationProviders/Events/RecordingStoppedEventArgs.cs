using System;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Events
{
    public class RecordingStoppedEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        /// <summary>
        /// Initialize a new instance of RecordingStoppedEventArgs
        /// </summary>
        public RecordingStoppedEventArgs() { }

        /// <summary>
        /// Initialize a new instance of RecordingStoppedEventArgs with an exception
        /// </summary>
        /// <param name="exception">Associated Exception</param>
        public RecordingStoppedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    public delegate void RecordingStopped(object sender, RecordingStoppedEventArgs recordingStoppedEventArgs);
}
