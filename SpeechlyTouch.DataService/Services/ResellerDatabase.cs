using System.Collections.Generic;
using System.Threading.Tasks;
using SpeechlyTouch.DataService.Models;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Method to get all Reseller records
        /// </summary>
        /// <returns>All Reseller records</returns>
        public async Task<List<Reseller>> GetResellersAsync()
        {
            return await Dataservice.Table<Reseller>().ToListAsync();
        }

        /// <summary>
        /// Method to get the first reseller info record
        /// </summary>
        /// <returns>The first record of Reseller Info</returns>
        public async Task<Reseller> GetFirstResellerInfoAsync()
        {
            return await Dataservice.Table<Reseller>().FirstOrDefaultAsync();
        }
    }
}
