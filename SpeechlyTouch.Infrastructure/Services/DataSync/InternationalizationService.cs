using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.DataSync
{
    public class InternationalizationService : IInternationalizationService
    {
        private const string InternationalizationLanguagesBaseUrl = "https://api.cognitive.microsofttranslator.com";
        private string IntenationalizationLanguagesUrl = $"{InternationalizationLanguagesBaseUrl}/languages?api-version=3.0";
        private HttpClient client;

        public async Task<List<InternationalizationLanguage>> GetInternationalizationLanguages()
        {
            try
            {
                if (client == null)
                    client = new HttpClient();

                var response = await client.GetAsync(IntenationalizationLanguagesUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject jsonContent = JObject.Parse(content);
                    var translations = jsonContent.Value<JObject>("translation").Properties();
                    var languages = new List<InternationalizationLanguage>();

                    foreach (var item in translations)
                    {
                        var key = item.Name; // e.g en
                        var value = item.Value; // e.g { "name": "English","nativeName": "English", "dir": "ltr"}
                        var language = JsonConvert.DeserializeObject<InternationalizationLanguage>(value.ToString());
                        language.Code = key;
                        languages.Add(language);
                    }

                    return languages;
                }

                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
