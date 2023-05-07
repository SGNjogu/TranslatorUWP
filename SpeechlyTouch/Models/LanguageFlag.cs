using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

namespace SpeechlyTouch.Models
{
    public class LanguageFlag : ObservableObject
    {
        private SvgImageSource _flag;
        public SvgImageSource Flag
        {
            get { return _flag; }
            set { SetProperty(ref _flag, value); }
        }

        private string _countryCode;
        public string CountryCode
        {
            get { return _countryCode; }
            set { SetProperty(ref _countryCode, value); }
        }

        private ObservableCollection<Language> _languages;
        public ObservableCollection<Language> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        private string _countryName;
        public string CountryName
        {
            get { return _countryName; }
            set { SetProperty(ref _countryName, value); }
        }

        private string _countryNativeName;
        public string CountryNativeName
        {
            get { return _countryNativeName; }
            set { SetProperty(ref _countryNativeName, value); }
        }
    }
}
