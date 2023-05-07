using SpeechlyTouch.Core.DTO;
using System;

namespace SpeechlyTouch.Core.Events
{
    public class InputDeviceStateChangedEventArgs : EventArgs
    {
        public InputDeviceState State { get; set; }
    }

    public delegate void InputDeviceStateChanged(object sender, InputDeviceStateChangedEventArgs inputStateChangedEventArgs);
}
