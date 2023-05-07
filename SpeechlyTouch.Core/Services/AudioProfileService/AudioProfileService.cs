using SpeechlyTouch.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace SpeechlyTouch.Core.Services.AudioProfileService
{
    public class AudioProfileService : IAudioProfileService
    {
        public async Task<List<InputDevice>> GetInputDevices()
        {
            try
            {
                List<InputDevice> devices = new List<InputDevice>();

                var inputDevices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);

                foreach (var item in inputDevices)
                {
                    var device = new InputDevice
                    {
                        Name = item.Name,
                        DeviceId = item.Id,
                        IsEnabled = item.IsEnabled
                    };
                    devices.Add(device);
                }

                return devices.Where(s => s.IsEnabled == true).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<OutputDevice>> GetOutputDevices()
        {
            try
            {
                List<OutputDevice> devices = new List<OutputDevice>();

                var outputDevices = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);

                foreach (var item in outputDevices)
                {
                    var device = new OutputDevice
                    {
                        Name = item.Name,
                        DeviceId = item.Id,
                        IsEnabled = item.IsEnabled,
                        AudioDevice = item
                    };
                    devices.Add(device);
                }

                return devices.Where(s => s.IsEnabled == true).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
