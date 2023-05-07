using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace SpeechlyTouch.Models
{
    public class Language : ObservableObject, ICloneable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _countryCode;
        public string CountryCode
        {
            get { return _countryCode; }
            set { SetProperty(ref _countryCode, value); }
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

        private string _englishName;
        public string EnglishName
        {
            get { return _englishName; }
            set { SetProperty(ref _englishName, value); }
        }

        private string _code;
        public string Code
        {
            get { return _code; }
            set { SetProperty(ref _code, value); }
        }

        private string _voice;
        public string Voice
        {
            get { return _voice; }
            set { SetProperty(ref _voice, value); }
        }

        private SvgImageSource _flag;
        public SvgImageSource Flag
        {
            get { return _flag; }
            set { SetProperty(ref _flag, value); }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private string _displayCode;
        public string DisplayCode
        {
            get { return _displayCode; }
            set { SetProperty(ref _displayCode, value); }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return $"Name: {Name} Code: {Code} Voice: {Voice}";
        }
    }
}
