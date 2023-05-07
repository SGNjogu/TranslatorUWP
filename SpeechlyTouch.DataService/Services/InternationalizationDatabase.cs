using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        public async Task<List<InternationalizationLanguage>> GetInternationalizationLanguages()
        {
            var languages = await Dataservice.Table<InternationalizationLanguage>().ToListAsync();
            return languages;
        }

        public async Task CreateInternationalizationLanguages(IEnumerable<InternationalizationLanguage> internationalizationLanguages)
        {
            var languages = await GetInternationalizationLanguages();

            if (languages != null && languages.Any())
            {
                await Dataservice.DeleteAllAsync<InternationalizationLanguage>();
            }

            foreach (var language in internationalizationLanguages)
            {
                await AddItemAsync<InternationalizationLanguage>(language);
            }
        }
    }
}
