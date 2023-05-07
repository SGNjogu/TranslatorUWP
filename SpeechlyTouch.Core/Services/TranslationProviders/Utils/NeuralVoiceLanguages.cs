using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Linq;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Utils
{
    public static class NeuralVoiceLanguages
    {
        public static IEnumerable<Language> GetLanguages()
        {
            return new List<Language>()
            {
                new Language() { Name = "Arabic (Saudi Arabia)", Code = "ar-SA", Voice = new List<string> { "ar-SA-HamedNeural" } },
                new Language() { Name = "Catalan (Spain)", Code = "ca-ES", Voice = new List<string> { "ca-ES-AlbaNeural" } },
                new Language() { Name = "Danish (Denmark)", Code = "da-DK", Voice = new List<string> { "da-DK-ChristelNeural" } },
                new Language() { Name = "German (Germany)", Code = "de-DE", Voice = new List<string> { "de-DE-AmalaNeural" } },
                new Language() { Name = "English (Australia)", Code = "en-AU", Voice = new List<string> { "en-AU-AnnetteNeural" } },

                new Language() { Name = "English (Canada)", Code = "en-CA", Voice = new List<string> { "en-CA-ClaraNeural" } },
                new Language() { Name = "English (India)", Code = "en-IN", Voice = new List<string> { "en-IN-NeerjaNeural" } },

                new Language() { Name = "Spanish (Mexico)", Code = "es-MX", Voice = new List<string> { "es-MX-BeatrizNeural" } },
                new Language() { Name = "Finnish (Finland)", Code = "fi-FI", Voice = new List<string> { "fi-FI-HarriNeural" } },
                new Language() { Name = "French (Canada)", Code = "fr-CA", Voice = new List<string> { "fr-CA-AntoineNeural" } },
                new Language() { Name = "Hindi (India)", Code = "hi-IN", Voice = new List<string> { "hi-IN-MadhurNeural" } },

                new Language() { Name = "Italian (Italy)", Code = "it-IT", Voice = new List<string> { "it-IT-BenignoNeural" } },
                new Language() { Name = "Japanese (Japan)", Code = "ja-JP", Voice = new List<string> { "ja-JP-AoiNeural" } },
                new Language() { Name = "Korean (Korea)", Code = "ko-KR", Voice = new List<string> { "ko-KR-BongJinNeural" } },
                new Language() { Name = "Norwegian (Bokmål) (Norway)", Code = "nb-NO", Voice = new List<string> { "nb-NO-FinnNeural" } },
                new Language() { Name = "Dutch (Netherlands)", Code = "nl-NL", Voice = new List<string> { "nl-NL-ColetteNeural" } },

                new Language() { Name = "Polish (Poland)", Code = "pl-PL", Voice = new List<string> { "pl-PL-AgnieszkaNeural" } },
                new Language() { Name = "Portuguese (Brazil)", Code = "pt-BR", Voice = new List<string> { "pt-BR-AntonioNeural" } },
                new Language() { Name = "Portuguese (Portugal)", Code = "pt-PT", Voice = new List<string> { "pt-PT-DuarteNeural" } },
                new Language() { Name = "Russian (Russia)", Code = "ru-RU", Voice = new List<string> { "ru-RU-DariyaNeural" } },
                new Language() { Name = "Swedish (Sweden)", Code = "sv-SE", Voice = new List<string> { "sv-SE-HilleviNeural" } },

                new Language() { Name = "Tamil (India)", Code = "ta-IN", Voice = new List<string> { "ta-IN-PallaviNeural" } },
                new Language() { Name = "Telugu (India)", Code = "te-IN", Voice = new List<string> { "te-IN-MohanNeural" } },
                new Language() { Name = "Chinese (Mandarin, Simplified)", Code = "zh-CN", Voice = new List<string> { "zh-CN-XiaochenNeural" } },
                new Language() { Name = "Chinese (Cantonese, Traditional)", Code = "zh-HK", Voice = new List<string> { "yue-CN-XiaoMinNeural" } },
                new Language() { Name = "Chinese (Taiwanese Mandarin)", Code = "zh-TW", Voice = new List<string> { "zh-TW-HsiaoChenNeural" } },

                new Language() { Name = "Thai (Thailand)", Code = "th-TH", Voice = new List<string> { "th-TH-AcharaNeural" } },
                new Language() { Name = "Turkish (Turkey)", Code = "tr-TR", Voice = new List<string> { "tr-TR-AhmetNeural" } },

                new Language() { Name = "Bulgarian (Bulgaria)", Code = "bg-BG", Voice = new List<string> { "bg-BG-BorislavNeural" } },
                new Language() { Name = "Croatian (Croatia)", Code = "hr-HR", Voice = new List<string> { "hr-HR-GabrijelaNeural" } },
                new Language() { Name = "Czech (Czechia)", Code = "cs-CZ", Voice = new List<string> { "cs-CZ-AntoninNeural" } },
                new Language() { Name = "Greek (Greece)", Code = "el-GR", Voice = new List<string> { "el-GR-AthinaNeural" } },
                new Language() { Name = "Hungarian (Hungary)", Code = "hu-HU", Voice = new List<string> { "hu-HU-NoemiNeural" } },
                new Language() { Name = "Romanian (Romania)", Code = "ro-RO", Voice = new List<string> { "ro-RO-AlinaNeural" } },
                new Language() { Name = "Slovak (Slovakia)", Code = "sk-SK", Voice = new List<string> { "sk-SK-LukasNeural" } },
                new Language() { Name = "Slovenian (Slovenia)", Code = "sl-SI", Voice = new List<string> { "sl-SI-PetraNeural" } },

                new Language() { Name = "Hebrew (Israel)", Code = "he-IL", Voice = new List<string> { "he-IL-AvriNeural" } },
                new Language() { Name = "Indonesian (Indonesia)", Code = "id-ID", Voice = new List<string> { "id-ID-ArdiNeural" } },
                new Language() { Name = "Malay (Malaysia)", Code = "ms-MY", Voice = new List<string> { "ms-MY-OsmanNeural" } },
                new Language() { Name = "Vietnamese (Vietnam)", Code = "vi-VN", Voice = new List<string> { "vi-VN-HoaiMyNeural" } },

                new Language() { Name = "English (United Kingdom)", Code = "en-GB", Voice = new List<string> { "en-GB-AbbiNeural" } },
                new Language() { Name = "English (United States)", Code = "en-US", Voice = new List<string> { "en-US-JennyNeural" } },
                new Language() { Name = "French (France)", Code = "fr-FR", Voice = new List<string> { "fr-FR-DeniseNeural" } },
                new Language() { Name = "Spanish (Spain)", Code = "es-ES", Voice = new List<string> { "es-ES-ElviraNeural" } },
                new Language() { Name = "Swahili (Kenya)", Code = "sw-KE", Voice = new List<string> { "sw-KE-ZuriNeural" } },
                new Language() { Name = "Estonian (Estonia)", Code = "et-EE", Voice = new List<string> { "et-EE-AnuNeural" } },
                new Language() { Name = "Gujarati (India)", Code = "gu-IN", Voice = new List<string> { "gu-IN-DhwaniNeural" } },
                new Language() { Name = "Latvian (Latvia)", Code = "lv-LV", Voice = new List<string> { "lv-LV-EveritaNeural" } },
                new Language() { Name = "Lithuanian (Lithuania)", Code = "lt-LT", Voice = new List<string> { "lt-LT-OnaNeural" } },
                new Language() { Name = "Maltese (Malta)", Code = "mt-MT", Voice = new List<string> { "mt-MT-GraceNeural" } },
                new Language() { Name = "Marathi (India)", Code = "mr-IN", Voice = new List<string> { "mr-IN-AarohiNeural" } },
                new Language() { Name = "Gaelic (Ireland)", Code = "ga-IE", Voice = new List<string> { "ga-IE-ColmNeural" } },
                new Language() { Name = "Ukrainian (Ukraine)", Code = "uk-UA", Voice = new List<string> { "uk-UA-PolinaNeural" } },
                new Language() { Name = "Filipino (Philippines)", Code = "fil-PH", Voice = new List<string> { "fil-PH-BlessicaNeural" } },
                new Language() { Name = "Nepali (Nepal)", Code = "ne-NP", Voice = new List<string> { "ne-NP-HemkalaNeural" } },
                new Language() { Name = "Serbian (Serbia)", Code = "sr-RS", Voice = new List<string> { "sr-RS-SophieNeural" } },
                new Language() { Name = "Welsh (Wales)", Code = "cy-GB", Voice = new List<string> { "cy-GB-NiaNeural" } },
                new Language() { Name = "Bosnian (Bosnia)", Code = "bs-BA", Voice = new List<string> { "bs-BA-VesnaNeural" } },

                new Language() { Name = "Afrikaans (South Africa)", Code = "af-ZA", Voice = new List<string> { "af-ZA-AdriNeural" } },
                new Language() { Name = "Albanian (Albania)", Code = "sq-AL", Voice = new List<string> { "sq-AL-AnilaNeural" } },
                new Language() { Name = "Amharic (Ethiopia)", Code = "am-ET", Voice = new List<string> { "am-ET-AmehaNeural" } },
                new Language() { Name = "Armenian (Armenia)", Code = "hy-AM", Voice = new List<string> { "hy-AM-AnahitNeural" } },
                new Language() { Name = "বাংলা (Bangladesh)", Code = "bn-BD", Voice = new List<string> { "bn-BD-NabanitaNeural" } },
                new Language() { Name = "Icelandic (Iceland)", Code = "is-IS", Voice = new List<string> { "is-IS-GudrunNeural" } },
                new Language() { Name = "Kannada (India)", Code = "kn-IN", Voice = new List<string> { "kn-IN-GaganNeural" } },
                new Language() { Name = "Kazakh (Kazakhstan)", Code = "kk-KZ", Voice = new List<string> { "kk-KZ-AigulNeural" } },
                new Language() { Name = "Khmer (Cambodia)", Code = "km-KH", Voice = new List<string> { "km-KH-PisethNeural" } },
                new Language() { Name = "Lao (Laos)", Code = "lo-LA", Voice = new List<string> { "lo-LA-ChanthavongNeural" } },
                new Language() { Name = "Malayalam (India)", Code = "ml-IN", Voice = new List<string> { "ml-IN-MidhunNeural" } },
                new Language() { Name = "Burmese (Myanmar)", Code = "my-MM", Voice = new List<string> { "my-MM-NilarNeural" } },
                new Language() { Name = "Pashto (Afghanistan)", Code = "ps-AF", Voice = new List < string > { "ps-AF-GulNawazNeural" } },
                new Language() { Name = "Persian (Iran)", Code = "fa-IR", Voice = new List < string > { "fa-IR-DilaraNeural" } },

                new Language() { Name = "Arabic (United Arab Emirates)", Code = "ar-AE", Voice = new List<string> { "ar-AE-FatimaNeural" } },
                new Language() { Name = "Arabic (Bahrain)", Code = "ar-BH", Voice = new List<string> { "ar-BH-AliNeural" } },
                new Language() { Name = "Arabic (Algeria)", Code = "ar-DZ", Voice = new List<string> { "ar-DZ-AminaNeural" } },
                new Language() { Name = "Arabic (Egypt)", Code = "ar-EG", Voice = new List<string> { "ar-EG-SalmaNeural" } },
                new Language() { Name = "Arabic (Iraq)", Code = "ar-IQ", Voice = new List<string> { "ar-IQ-BasselNeural" } },
                new Language() { Name = "Arabic (Jordan)", Code = "ar-JO", Voice = new List<string> { "ar-JO-SanaNeural" } },
                new Language() { Name = "Arabic (Kuwait)", Code = "ar-KW", Voice = new List<string> { "ar-KW-FahedNeural" } },
                new Language() { Name = "Arabic (Lebanon)", Code = "ar-LB", Voice = new List<string> { "ar-LB-LaylaNeural" } },
                new Language() { Name = "Arabic (Libya)", Code = "ar-LY", Voice = new List<string> { "ar-LY-ImanNeural" } },
                new Language() { Name = "Arabic (Morocco)", Code = "ar-MA", Voice = new List<string> { "ar-MA-JamalNeural" } },
                new Language() { Name = "Arabic (Oman)", Code = "ar-OM", Voice = new List<string> { "ar-OM-AbdullahNeural" } },
                new Language() { Name = "Arabic (Qatar)", Code = "ar-QA", Voice = new List<string> { "ar-QA-AmalNeural" } },
                new Language() { Name = "Arabic (Syria)", Code = "ar-SY", Voice = new List<string> { "ar-SY-AmanyNeural" } },
                new Language() { Name = "Arabic (Tunisia)", Code = "ar-TN", Voice = new List<string> { "ar-TN-HediNeural" } },
                new Language() { Name = "Arabic (Yemen)", Code = "ar-YE", Voice = new List<string> { "ar-YE-MaryamNeural" } },
                new Language() { Name = "German (Austria)", Code = "de-AT", Voice = new List<string> { "de-AT-IngridNeural" } },
                new Language() { Name = "German (Switzerland)", Code = "de-CH", Voice = new List<string> { "de-CH-JanNeural" } },
                new Language() { Name = "English (Hong Kong)", Code = "en-HK", Voice = new List<string> { "en-HK-SamNeural" } },
                new Language() { Name = "English (Ireland)", Code = "en-IE", Voice = new List<string> { "en-IE-ConnorNeural" } },
                new Language() { Name = "English (Kenya)", Code = "en-KE", Voice = new List<string> { "en-KE-AsiliaNeural" } },
                new Language() { Name = "English (Nigeria)", Code = "en-NG", Voice = new List<string> { "en-NG-AbeoNeural" } },
                new Language() { Name = "English (New Zealand)", Code = "en-NZ", Voice = new List<string> { "en-NZ-MitchellNeural" } },
                new Language() { Name = "English (Philippines)", Code = "en-PH", Voice = new List<string> { "en-PH-JamesNeural" } },
                new Language() { Name = "English (Singapore)", Code = "en-SG", Voice = new List<string> { "en-SG-LunaNeural" } },
                new Language() { Name = "English (Tanzania)", Code = "en-TZ", Voice = new List<string> { "en-TZ-ElimuNeural" } },
                new Language() { Name = "English (South Africa)", Code = "en-ZA", Voice = new List<string> { "en-ZA-LeahNeural" } },
                new Language() { Name = "Spanish (Argentina)", Code = "es-AR", Voice = new List<string> { "es-AR-ElenaNeural" } },
                new Language() { Name = "Spanish (Bolivia)", Code = "es-BO", Voice = new List<string> { "es-BO-MarceloNeural" } },
                new Language() { Name = "Spanish (Chile)", Code = "es-CL", Voice = new List<string> { "es-CL-CatalinaNeural" } },
                new Language() { Name = "Spanish (Colombia)", Code = "es-CO", Voice = new List<string> { "es-CO-GonzaloNeural" } },
                new Language() { Name = "Spanish (Costa Rica)", Code = "es-CR", Voice = new List<string> { "es-CR-JuanNeural" } },
                new Language() { Name = "Spanish (Cuba)", Code = "es-CU", Voice = new List<string> { "es-CU-BelkysNeural" } },
                new Language() { Name = "Spanish (Dominican Republic)", Code = "es-DO", Voice = new List<string> { "es-DO-EmilioNeural" } },
                new Language() { Name = "Spanish (Ecuador)", Code = "es-EC", Voice = new List<string> { "es-EC-AndreaNeural" } },
                new Language() { Name = "Spanish (Equatorial Guinea)", Code = "es-GQ", Voice = new List<string> { "es-GQ-JavierNeural" } },
                new Language() { Name = "Spanish (Guatemala)", Code = "es-GT", Voice = new List<string> { "es-GT-AndresNeural" } },
                new Language() { Name = "Spanish (Honduras)", Code = "es-HN", Voice = new List<string> { "es-HN-CarlosNeural" } },
                new Language() { Name = "Spanish (Nicaragua)", Code = "es-NI", Voice = new List<string> { "es-NI-FedericoNeural" } },
                new Language() { Name = "Spanish (Panama)", Code = "es-PA", Voice = new List<string> { "es-PA-MargaritaNeural" } },
                new Language() { Name = "Spanish (Peru)", Code = "es-PE", Voice = new List<string> { "es-PE-AlexNeural" } },
                new Language() { Name = "Spanish (Puerto Rico)", Code = "es-PR", Voice = new List<string> { "es-PR-KarinaNeural" } },
                new Language() { Name = "Spanish (Paraguay)", Code = "es-PY", Voice = new List<string> { "es-PY-MarioNeural" } },
                new Language() { Name = "Spanish (El Salvador)", Code = "es-SV", Voice = new List<string> { "es-SV-LorenaNeural" } },
                new Language() { Name = "Spanish (United States)", Code = "es-US", Voice = new List<string> { "es-US-AlonsoNeural" } },
                new Language() { Name = "Spanish (Uruguay)", Code = "es-UY", Voice = new List<string> { "es-UY-MateoNeural" } },
                new Language() { Name = "Spanish (Venezuela)", Code = "es-VE", Voice = new List<string> { "es-VE-PaolaNeural" } },
                new Language() { Name = "French (Belgium)", Code = "fr-BE", Voice = new List<string> { "fr-BE-CharlineNeural" } },
                new Language() { Name = "French (Switzerland)", Code = "fr-CH", Voice = new List<string> { "fr-CH-ArianeNeural" } },
                new Language() { Name = "Dutch (Belgium)", Code = "nl-BE", Voice = new List<string> { "nl-BE-ArnaudNeural" } },
                new Language() { Name = "Swahili (Tanzania)", Code = "sw-TZ", Voice = new List<string> { "sw-TZ-DaudiNeural" } },
                new Language() { Name = "Tamil (Sri Lanka)", Code = "ta-LK", Voice = new List<string> { "ta-LK-KumarNeural" } },
                new Language() { Name = "Tamil (Malaysia)", Code = "ta-MY", Voice = new List<string> { "ta-MY-KaniNeural" } },
                new Language() { Name = "Tamil (Singapore)", Code = "ta-SG", Voice = new List<string> { "ta-SG-AnbuNeural" } },
                new Language() { Name = "Urdu (India)", Code = "ur-IN", Voice = new List<string> { "ur-IN-GulNeural" } },
                new Language() { Name = "Urdu (Pakistan)", Code = "ur-PK", Voice = new List<string> { "ur-PK-AsadNeural" } },
            };
        }

        public static bool IsNeuralVoiceLanguage(string languageCode)
        {
            var neuralLanguages = GetLanguages();
            return neuralLanguages.Select(l => l.Code).Contains(languageCode);
        }
    }
}
