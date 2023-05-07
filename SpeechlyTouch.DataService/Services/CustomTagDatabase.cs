using System.Collections.Generic;
using System.Threading.Tasks;
using SpeechlyTouch.DataService.Models;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        public async Task<List<CustomTag>> GetCustomTagsAsync()
        {
            return await Dataservice.Table<CustomTag>().ToListAsync();
        }
    }
}
