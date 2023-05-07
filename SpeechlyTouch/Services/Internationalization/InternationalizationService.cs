using Microsoft.AppCenter.Crashes;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Services.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IInternationalization = SpeechlyTouch.Infrastructure.Services.Interfaces.IInternationalizationService;
using Language = SpeechlyTouch.Core.Domain.InternationalizationLanguage;

namespace SpeechlyTouch.Services.Internationalization
{
    public class InternationalizationService : IInternationalizationService
    {
        private readonly IInternationalization _internationalizationService;
        private readonly IDataService _dataServce;
        private readonly ISettingsService _settingsService;
        private readonly ICrashlytics _crashlytics;

        private object _lock = new object();

        public InternationalizationService
            (
                IInternationalization internationalizationService,
                IDataService dataServce,
                ISettingsService settingsService,
                ICrashlytics crashlytics
            )
        {
            _internationalizationService = internationalizationService;
            _dataServce = dataServce;
            _settingsService = settingsService;
            _crashlytics = crashlytics;
        }

        public async Task GetInternationalizationLanguages()
        {
            try
            {
                var languages = await _internationalizationService.GetInternationalizationLanguages();
                var localLanguages = await _dataServce.GetInternationalizationLanguages();

                if (languages != null && languages.Any())
                {
                    if (localLanguages != null && localLanguages.Any())
                    {
                        if (localLanguages.Count != languages.Count)
                        {
                            await AddInternationalizationLanguages(languages);
                        }
                    }
                    else
                    {
                        await AddInternationalizationLanguages(languages);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task AddInternationalizationLanguages(List<Language> languages)
        {
            languages = ModifyInternationalizationLanguages(languages);

            List<InternationalizationLanguage> internationalizationLanguages = new List<InternationalizationLanguage>();

            foreach (var item in languages)
            {
                InternationalizationLanguage internationalizationLanguage = new InternationalizationLanguage
                {
                    Name = item.Name,
                    NativeName = item.NativeName,
                    Code = item.Code
                };
                internationalizationLanguages.Add(internationalizationLanguage);
            }

            await _dataServce.CreateInternationalizationLanguages(internationalizationLanguages);
        }

        private List<Language> ModifyInternationalizationLanguages(List<Language> languages)
        {
            // add accent to az -> az-arab, ui -> ui-cans, ku -> ku-arab, pt -> pt-br
            var az = languages.FirstOrDefault(c => c.Code == "az");
            if (az != null)
                az.Code = "az-arab";

            var ui = languages.FirstOrDefault(c => c.Code == "ui");
            if (ui != null)
                ui.Code = "ui-cans";

            var ku = languages.FirstOrDefault(c => c.Code == "ku");
            if (ku != null)
                ku.Code = "ku-arab";

            var pt = languages.FirstOrDefault(c => c.Code == "pt");
            if (pt != null)
                pt.Code = "pt-br";

            // remove unsupported languages fj, ht, kmr, lhz, mg, mww, my, otq, ps, sm, tlh-Latn, tlh-Piqd, to, ty, yua, yue
            languages.RemoveAll(c =>
            c.Code == "fj" ||
            c.Code == "ht" ||
            c.Code == "kmr" ||
            c.Code == "lhz" ||
            c.Code == "mg" ||
            c.Code == "mww" ||
            c.Code == "my" ||
            c.Code == "otq" ||
            c.Code == "ps" ||
            c.Code == "sm" ||
            c.Code == "kmr" ||
            c.Code == "tlh-Latn" ||
            c.Code == "tlh-Piqd" ||
            c.Code == "to" ||
            c.Code == "ty" ||
            c.Code == "yua" ||
            c.Code == "yue"
            );

            return languages;
        }

        public async void LoadApplicationLanguage()
        {
            var languages = await _dataServce.GetInternationalizationLanguages();

            if (languages != null && languages.Any())
            {
                var applicationLanguageCode = _settingsService.ApplicationLanguageCode;

                if (!string.IsNullOrEmpty(applicationLanguageCode))
                {
                    var selectedLanguage = languages.FirstOrDefault(s => s.Code == applicationLanguageCode);

                    if (selectedLanguage != null)
                    {
                        SetAppLanguage(selectedLanguage.Code);
                    }
                    else
                    {
                        SetAppLanguage("en");
                    }
                }
                else
                {
                    SetAppLanguage("en");
                }
            }
            else
            {
                SetAppLanguage("en");
            }
        }

        public void SetAppLanguage(string languageCode)
        {
            lock (_lock)
            {
                if (languageCode == "zh")
                {
                    languageCode = "zh-Hans";
                }
                _settingsService.ApplicationLanguageCode = languageCode;
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;
                Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
                Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
            }
            
        }
    }
}
