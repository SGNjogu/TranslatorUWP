using System.Collections.Generic;
using System.Threading.Tasks;
using SpeechlyTouch.DataService.Models;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        public async Task<List<ReleaseNote>> CreateReleaseNotes(IEnumerable<ReleaseNote> releaseNotes)
        {
            var notes = await Dataservice.Table<ReleaseNote>().CountAsync();

            if (notes > 1)
            {
                await Dataservice.DeleteAllAsync<ReleaseNote>();
            }

            foreach (var releaseNote in releaseNotes)
            {
                await AddItemAsync<ReleaseNote>(releaseNote);
            }

            var newNotes = await GetReleaseNotes();

            return newNotes;
        }

        public async Task<List<ReleaseNote>> GetReleaseNotes()
        {
            var releaseNotes = await Dataservice.Table<ReleaseNote>().ToListAsync();
            return releaseNotes;
        }
    }
}
