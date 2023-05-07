using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace SpeechlyTouch.Styles
{
    public class DynamicColors : INotifyPropertyChanged
    {
        private SolidColorBrush _primaryTextColor = ThemeHelper._primaryTextColor;
        public SolidColorBrush PrimaryTextColor
        {
            get { return _primaryTextColor; }
            set { _primaryTextColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _secondaryTextColor = ThemeHelper._secondaryTextColor;
        public SolidColorBrush SecondaryTextColor
        {
            get { return _secondaryTextColor; }
            set { _secondaryTextColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _primaryBackgroundColor = ThemeHelper._primaryBackgroundColor;
        public SolidColorBrush PrimaryBackgroundColor
        {
            get { return _primaryBackgroundColor; }
            set { _primaryBackgroundColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _secondaryBackgroundColor = ThemeHelper._secondaryBackgroundColor;
        public SolidColorBrush SecondaryBackgroundColor
        {
            get { return _secondaryBackgroundColor; }
            set { _secondaryBackgroundColor = value; OnPropertyChanged(); }
        }

        private Color _brandedPrimaryBackgroundColor = ThemeHelper._brandedPrimaryBackgroundColor;
        public Color BrandedPrimaryBackgroundColor
        {
            get { return _brandedPrimaryBackgroundColor; }
            set { _brandedPrimaryBackgroundColor = value; OnPropertyChanged(); }
        }

        private Color _brandedSecondaryBackgroundColor = ThemeHelper._brandedSecondaryBackgroundColor;
        public Color BrandedSecondaryBackgroundColor
        {
            get { return _brandedSecondaryBackgroundColor; }
            set { _brandedSecondaryBackgroundColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _accentColor = ThemeHelper._accentColor;
        public SolidColorBrush AccentColor
        {
            get { return _accentColor; }
            set { _accentColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _btnAccentColor = ThemeHelper._btnAccentColor;
        public SolidColorBrush BtnAccentColor
        {
            get { return _btnAccentColor; }
            set { _btnAccentColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _btnAccentTextColor = ThemeHelper._btnAccentTextColor;
        public SolidColorBrush BtnAccentTextColor
        {
            get { return _btnAccentTextColor; }
            set { _btnAccentTextColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _accentHover = ThemeHelper._accentHover;
        public SolidColorBrush AccentHover
        {
            get { return _accentHover; }
            set { _accentHover = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _accentPressed = ThemeHelper._accentPressed;
        public SolidColorBrush AccentPressed
        {
            get { return _accentPressed; }
            set { _accentPressed = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _dashboardBtnAccentColor = ThemeHelper._dashboardBtnAccentColor;
        public SolidColorBrush DashboardBtnAccentColor
        {
            get { return _dashboardBtnAccentColor; }
            set { _dashboardBtnAccentColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _dashboardBtnTextColor = ThemeHelper._dashboardBtnTextColor;
        public SolidColorBrush DashboardBtnTextColor
        {
            get { return _dashboardBtnTextColor; }
            set { _dashboardBtnTextColor = value; OnPropertyChanged(); }
        }

        private Color _borderColor = ThemeHelper._borderColor;
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _borderBrush = ThemeHelper._borderBrush;
        public SolidColorBrush BorderBrush
        {
            get { return _borderBrush; }
            set { _borderBrush = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _menuBackgroundColor = ThemeHelper._menuBackgroundColor;
        public SolidColorBrush MenuBackgroundColor
        {
            get { return _menuBackgroundColor; }
            set { _menuBackgroundColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _selectedMenuItemColor = ThemeHelper._selectedMenuItemColor;
        public SolidColorBrush SelectedMenuItemColor
        {
            get { return _selectedMenuItemColor; }
            set { _selectedMenuItemColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _selectedMenuItemBackgroundColor = ThemeHelper._selectedMenuItemBackgroundColor;
        public SolidColorBrush SelectedMenuItemBackgroundColor
        {
            get { return _selectedMenuItemBackgroundColor; }
            set { _selectedMenuItemBackgroundColor = value; OnPropertyChanged(); }
        }

        private Color _selectedMenuItemShadowColor = ThemeHelper._selectedMenuItemShadowColor;
        public Color SelectedMenuItemShadowColor
        {
            get { return _selectedMenuItemShadowColor; }
            set { _selectedMenuItemShadowColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _unSelectedMenuItemColor = ThemeHelper._unSelectedMenuItemColor;
        public SolidColorBrush UnSelectedMenuItemColor
        {
            get { return _unSelectedMenuItemColor; }
            set { _unSelectedMenuItemColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _unSelectedMenuItemBackgroundColor = ThemeHelper._unSelectedMenuItemBackgroundColor;
        public SolidColorBrush UnSelectedMenuItemBackgroundColor
        {
            get { return _unSelectedMenuItemBackgroundColor; }
            set { _unSelectedMenuItemBackgroundColor = value; OnPropertyChanged(); }
        }

        private Color _unSelectedMenuItemShadowColor = ThemeHelper._unSelectedMenuItemShadowColor;
        public Color UnSelectedMenuItemShadowColor
        {
            get { return _unSelectedMenuItemShadowColor; }
            set { _unSelectedMenuItemShadowColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
