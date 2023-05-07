using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Events;
using System;
using Windows.Devices.Enumeration;

namespace SpeechlyTouch.Core.Domain
{
    public class OutputDevice
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public bool IsEnabled { get; set; }
        public string ContainerId { get; set; }

        public DeviceInformation AudioDevice { get; set; }

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

        public OutputDeviceState State { get; set; } = OutputDeviceState.Idle;

        /// <summary>
        /// Output device state change event
        /// </summary>
        public event OutputDeviceStateChanged OutputStateChanged;

        /// <summary>
        /// Set new output device state
        /// </summary>
        /// <param name="state">New state</param>
        public void SetState(OutputDeviceState state)
        {
            if (State == state) return;

            State = state;

            var args = new OutputDeviceStateChangedEventArgs
            {
                State = State
            };

            OutputStateChanged?.Invoke(this, args);
        }
    }
}
