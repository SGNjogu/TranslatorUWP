using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Inserts a list of backend languages
        /// </summary>
        /// <param name="backendLanguages">Takes in a list of backend languages</param>
        public async Task InsertBackendLanguagesAsync(IEnumerable<BackendLanguage> languages)
        {
            await Dataservice.InsertAllAsync(languages);
        }

        /// <summary>
        /// Method to get all Backend Languages
        /// </summary>
        /// <returns>List of codes for backend languages</returns>
        public async Task<List<string>> GetBackendLanguagesAsync()
        {
            List<string> backendLanguageCodes = new List<string>();
            var languages = await Dataservice.Table<BackendLanguage>().ToListAsync();
            foreach (var language in languages)
            {
                backendLanguageCodes.Add(language.Code);
            }
            return backendLanguageCodes;
        }
    }
}
