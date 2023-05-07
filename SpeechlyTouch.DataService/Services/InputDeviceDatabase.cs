
using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        public async Task<List<InputDevice>> GetInputDevicesAsync()
        {
            return await Dataservice.Table<InputDevice>().ToListAsync();
        }

        public async Task<InputDevice> AddInputDeviceAsync(InputDevice inputDevice)
        {
            var existingInputDevices = await GetInputDevicesAsync();
            var existingInputDevice = existingInputDevices.Find(d => d.Participant == inputDevice.Participant);

            if (existingInputDevices.Count == 0 || existingInputDevice == null)
            {
                return await AddItemAsync<InputDevice>(inputDevice);
            }
            else
            {
                existingInputDevice.Name = inputDevice.Name;
                existingInputDevice.DeviceId = inputDevice.DeviceId;
                existingInputDevice.IsJabra = inputDevice.IsJabra;
                existingInputDevice.ContainerId = inputDevice.ContainerId;
                existingInputDevice.Participant = inputDevice.Participant;
                return await UpdateItemAsync<InputDevice>(existingInputDevice);
            }
        }
    }
}
