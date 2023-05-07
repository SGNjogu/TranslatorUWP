using SpeechlyTouch.Core.DTO;
using System;

namespace SpeechlyTouch.Core.Events
{
    public class OutputDeviceStateChangedEventArgs : EventArgs
    {
        public OutputDeviceState State { get; set; }
    }

    public delegate void OutputDeviceStateChanged(object sender, OutputDeviceStateChangedEventArgs outputStateChangedEventArgs);
}
