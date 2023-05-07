using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Method to get all PlaybackUsage
        /// </summary>
        /// <returns>All PlaybackUsage</returns>
        public async Task<List<PlaybackUsage>> GetPlaybackUsageAsync()
        {
            return await Dataservice.Table<PlaybackUsage>().ToListAsync();
        }
    }
}
