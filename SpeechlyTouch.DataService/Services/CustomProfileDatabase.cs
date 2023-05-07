using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpeechlyTouch.DataService.Models;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Method to get all CustomProfiles
        /// </summary>
        /// <returns>All CustomProfiles</returns>
        public async Task<List<CustomProfile>> GetCustomProfilesAsync()
        {
            return await Dataservice.Table<CustomProfile>().ToListAsync();
        }

        /// <summary>
        /// Method to get one custom profile using the name
        /// </summary>
        public async Task<CustomProfile> GetCustomProfile(string name)
        {
            return await Dataservice.Table<CustomProfile>().Where(s => s.Name == name).FirstOrDefaultAsync();
        }
    }
}
