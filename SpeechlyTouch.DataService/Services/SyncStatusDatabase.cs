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
        /// Method to get all Sync Status
        /// </summary>
        /// <returns>All Stauts</returns>
        public async Task<List<SyncStatus>> GetSyncStatusAsync()
        {
            return await Dataservice.Table<SyncStatus>().ToListAsync();
        }

        /// <summary>
        /// Method to get one Sync Status
        /// </summary>
        /// <returns>Single Sync Status</returns>
        public async Task<SyncStatus> GetOneSyncStatusAsync(int syncStatusId)
        {
            return await Dataservice.Table<SyncStatus>().Where(s => s.ID == syncStatusId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets Sync Status Count
        /// </summary>
        /// <returns>Number of SyncStatus</returns>
        public async Task<int> GetSyncStatusCountAsync()
        {
            return await Dataservice.Table<SyncStatus>().CountAsync();
        }
    }
}
