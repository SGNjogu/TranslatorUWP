using SpeechlyTouch.DataService.Models;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        public async Task<UsageLimit> CreateUsageLimit(UsageLimit usageLimit)
        {
            var limit = await Dataservice.Table<UsageLimit>().FirstOrDefaultAsync();

            if (limit != null)
            {
                await Dataservice.DeleteAllAsync<UsageLimit>();
            }

            return await AddItemAsync<UsageLimit>(usageLimit);
        }

        public async Task<UsageLimit> GetUsageLimit()
        {
            return await Dataservice.Table<UsageLimit>().FirstOrDefaultAsync();
        }
    }
}