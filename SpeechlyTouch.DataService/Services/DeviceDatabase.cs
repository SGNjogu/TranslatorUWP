using SpeechlyTouch.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Method to get all Devices
        /// </summary>
        /// <returns>All Devices</returns>
        public async Task<List<Device>> GetDevicesAsync()
        {
            return await Dataservice.Table<Device>().ToListAsync();
        }

        /// <summary>
        /// Method to get devices of a Session
        /// </summary>
        public async Task<List<Device>> GetSessionDevices(int sessionId)
        {
            return await Dataservice.Table<Device>().Where(s => s.SessionId == sessionId).ToListAsync();
        }

        /// <summary>
        /// Inserts a list of devices
        /// </summary>
        /// <param name="devices">Takes in a list of devices</param>
        public async Task InsertDevicesAsync(IEnumerable<Device> devices)
        {
            await Dataservice.InsertAllAsync(devices);
        }
    }
}
