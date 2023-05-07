using System.Collections.Generic;
using System.Threading.Tasks;
using SpeechlyTouch.DataService.Models;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService 
    {
        /// <summary>
        /// Method to get organization settings for the current user's organization
        /// </summary>
        /// <returns>All Settingss</returns>
        public async Task<List<OrganizationSettings>> GetOrganizationSettingsAsync()
        {
            return await Dataservice.Table<OrganizationSettings>().ToListAsync();
        }

        /// <summary>
        /// Method to get the first OrganizationSettings record
        /// </summary>
        /// <returns>The first record of OrganizationSettings</returns>
        public async Task<OrganizationSettings> GetOneOrganizationSettingsAsync()
        {
            return await Dataservice.Table<OrganizationSettings>().FirstOrDefaultAsync();
        }

        /// <summary>
        /// Method to get organization tags for the current user's organization
        /// </summary>
        /// <returns>All Tags</returns>
        public async Task<List<OrganizationTag>> GetOrganizationTagsAsync()
        {
            return await Dataservice.Table<OrganizationTag>().ToListAsync();
        }
    }
}
