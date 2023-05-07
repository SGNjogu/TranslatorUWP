using Microsoft.AppCenter.Crashes;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.Languages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.FlagLanguage
{
    public class FlagLanguageService : IFlagLanguageService
    {
        private List<LanguageFlag> _languageFlags { get; set; }

        private readonly ILanguagesService _languagesServie;
        private readonly ICrashlytics _crashlytics;

        public FlagLanguageService(ILanguagesService languagesServie, ICrashlytics crashlytics)
        {
            _languagesServie = languagesServie;
            _crashlytics = crashlytics;
        }

        public async Task<List<LanguageFlag>> GetFlagLanguages()
        {
            try
            {
                if (_languageFlags == null || !_languageFlags.Any())
                {
                    _languageFlags = new List<LanguageFlag>();

                    var languages = await _languagesServie.GetSupportedLanguagesAsync();

                    foreach (var language in languages)
                    {
                        var code = language.Code.Substring(3);

                        if (_languageFlags.Any(s => s.CountryCode == code))
                        {
                            var languageFlag = _languageFlags.FirstOrDefault(s => s.CountryCode == code);
                            languageFlag.Languages.Add(language);
                        }
                        else
                        {
                            _languageFlags.Add(new LanguageFlag
                            {
                                CountryCode = code,
                                Flag = language.Flag,
                                CountryName = language.CountryName,
                                CountryNativeName = language.CountryNativeName,
                                Languages = new ObservableCollection<Language> { language }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                Debug.WriteLine(ex.Message);
            }

            return _languageFlags;
        }
    }
}
