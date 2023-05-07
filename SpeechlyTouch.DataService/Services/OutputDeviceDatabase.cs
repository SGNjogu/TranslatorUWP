
using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        public async Task<List<OutputDevice>> GetOuputDevicesAsync()
        {
            return await Dataservice.Table<OutputDevice>().ToListAsync();
        }

        public async Task<OutputDevice> AddOutputDeviceAsync(OutputDevice outputDevice)
        {
            var existingOutputDevices = await GetOuputDevicesAsync();
            var existingOutputDevice = existingOutputDevices.Find(d => d.Participant == outputDevice.Participant);

            if (existingOutputDevices.Count == 0 || existingOutputDevice == null)
            {
                return await AddItemAsync<OutputDevice>(outputDevice);
            }
            else
            {
                existingOutputDevice.Name = outputDevice.Name;
                existingOutputDevice.DeviceId = outputDevice.DeviceId;
                existingOutputDevice.IsJabra = outputDevice.IsJabra;
                existingOutputDevice.ContainerId = outputDevice.ContainerId;
                existingOutputDevice.Participant = outputDevice.Participant;
                return await UpdateItemAsync<OutputDevice>(existingOutputDevice);
            }
        }
    }
}
