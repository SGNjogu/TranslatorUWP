using System;

namespace SpeechlyTouch.Core.Domain
{
    public class AudioProfile
    {
        public InputDevice InputDevice { get; set; }

        public OutputDevice OutputDevice { get; set; }

        public string ContainerId { get; set; }

        public string Name { get; set; }

        public bool IsComplete
        {
            get
            {
                return InputDevice != null && OutputDevice != null;
            }
        }

        public bool IsJabra
        {
            get
            {
                try
                {
                    //Jabra Temp Fix for Lockdown
                    //BlueParrot Temp Fix for Lockdown

                    if(InputDevice != null && OutputDevice != null)
                    {
                        return InputDevice.IsJabra || OutputDevice.IsJabra;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
