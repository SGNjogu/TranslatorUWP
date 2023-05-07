using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Events;
using System;

namespace SpeechlyTouch.Core.Domain
{
    public class InputDevice
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public bool IsEnabled { get; set; }
        public string ContainerId { get; set; }

        /// <summary>
        /// Input device state
        /// </summary>
        public InputDeviceState State { get; private set; }

        /// <summary>
        /// Input device state change event
        /// </summary>
        public event InputDeviceStateChanged InputStateChanged;

        public bool IsJabra
        {
            get
            {
                try
                {
                    //Jabra Temp Fix for Lockdown
                    //BlueParrot Temp Fix for Lockdown
                    return Name.ToLower().Contains("jabra") ||
                           Name.ToLower().Contains("c300-xt");
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///  Set device state
        /// </summary>
        /// <param name="state">Input device state</param>
        public void SetState(InputDeviceState state)
        {
            if (State == state) return;

            State = state;
            var args = new InputDeviceStateChangedEventArgs
            {
                State = state
            };

            InputStateChanged?.Invoke(this, args);
        }
    }
}
