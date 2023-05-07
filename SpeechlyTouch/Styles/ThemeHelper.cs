using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using ColorConverter = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

namespace SpeechlyTouch.Styles
{
    public static class ThemeHelper
    {
        public static DynamicColors _dynamicColors;
        public static SolidColorBrush _primaryTextColor = new SolidColorBrush(ColorConverter.ToColor("#000000"));
        public static SolidColorBrush _secondaryTextColor = new SolidColorBrush(ColorConverter.ToColor("#02175d"));
        public static SolidColorBrush _primaryBackgroundColor = new SolidColorBrush(ColorConverter.ToColor("#FFFFFF"));
        public static SolidColorBrush _secondaryBackgroundColor = new SolidColorBrush(ColorConverter.ToColor("#e7edfd"));
        public static Color _brandedPrimaryBackgroundColor = ColorConverter.ToColor("#ff93ff");
        public static Color _brandedSecondaryBackgroundColor = ColorConverter.ToColor("#FFFFFF");
        public static SolidColorBrush _accentColor = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        public static SolidColorBrush _btnAccentColor = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        public static SolidColorBrush _btnAccentTextColor = new SolidColorBrush(ColorConverter.ToColor("#FFFFFF"));
        public static SolidColorBrush _dashboardBtnAccentColor = new SolidColorBrush(ColorConverter.ToColor("#F5E4F6"));
        public static SolidColorBrush _dashboardBtnTextColor = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        public static SolidColorBrush _accentHover = new SolidColorBrush(ColorConverter.ToColor("#F5E3F7"));
        public static SolidColorBrush _accentPressed = new SolidColorBrush(ColorConverter.ToColor("#F0D4F1"));
        public static Color _borderColor = ColorConverter.ToColor("#DEE0EF");
        public static SolidColorBrush _borderBrush = new SolidColorBrush(ColorConverter.ToColor("#DEE0EF"));

        public static SolidColorBrush _menuBackgroundColor = new SolidColorBrush(ColorConverter.ToColor("#F3F4F9"));
        public static SolidColorBrush _selectedMenuItemColor = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        public static SolidColorBrush _selectedMenuItemBackgroundColor = new SolidColorBrush(Colors.White);
        public static Color _selectedMenuItemShadowColor = ColorConverter.ToColor("#7781BE");
        public static SolidColorBrush _unSelectedMenuItemColor = new SolidColorBrush(Colors.Gray);
        public static SolidColorBrush _unSelectedMenuItemBackgroundColor = new SolidColorBrush(Colors.Transparent);
        public static Color _unSelectedMenuItemShadowColor = Colors.Transparent;

        public static void ListenForRequestedTheme()
        {
            _dynamicColors = (DynamicColors)Application.Current.Resources["DynamicColors"];
            ApplyTheme();
            var Listener = new ThemeListener();
            Listener.ThemeChanged += Listener_ThemeChanged;
        }

        private static void Listener_ThemeChanged(ThemeListener sender)
        {
            ApplyTheme();
        }

        private static void ApplyTheme()
        {
            var currentTheme = Application.Current.RequestedTheme;

            if (currentTheme == ApplicationTheme.Dark)
            {
                App.Current.RequestedTheme = ApplicationTheme.Light;
            }

            var accessibilitySettings = new Windows.UI.ViewManagement.AccessibilitySettings();

            if (!accessibilitySettings.HighContrast)
            {
                _dynamicColors.PrimaryBackgroundColor = _primaryBackgroundColor;
                _dynamicColors.SecondaryBackgroundColor = _secondaryBackgroundColor;
                _dynamicColors.BrandedPrimaryBackgroundColor = _brandedPrimaryBackgroundColor;
                _dynamicColors.BrandedSecondaryBackgroundColor = _brandedSecondaryBackgroundColor;
                _dynamicColors.AccentColor = _accentColor;
                _dynamicColors.BtnAccentColor = _btnAccentColor;
                _dynamicColors.BtnAccentTextColor = _btnAccentTextColor;
                _dynamicColors.AccentPressed = _accentPressed;
                _dynamicColors.DashboardBtnAccentColor = _dashboardBtnAccentColor;
                _dynamicColors.DashboardBtnTextColor = _dashboardBtnTextColor;
                _dynamicColors.PrimaryTextColor = _primaryTextColor;
                _dynamicColors.SecondaryTextColor = _secondaryTextColor;
                _dynamicColors.BorderColor = _borderColor;
                _dynamicColors.BorderBrush = _borderBrush;
                _dynamicColors.MenuBackgroundColor = _menuBackgroundColor;
                _dynamicColors.SelectedMenuItemColor = _selectedMenuItemColor;
                _dynamicColors.SelectedMenuItemBackgroundColor = _selectedMenuItemBackgroundColor;
                _dynamicColors.SelectedMenuItemShadowColor = _selectedMenuItemShadowColor;
                _dynamicColors.UnSelectedMenuItemColor = _unSelectedMenuItemColor;
                _dynamicColors.UnSelectedMenuItemBackgroundColor = _unSelectedMenuItemBackgroundColor;
                _dynamicColors.UnSelectedMenuItemShadowColor = _unSelectedMenuItemShadowColor;
            }
            else if (accessibilitySettings.HighContrast)
            {
                _dynamicColors.PrimaryBackgroundColor = (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                _dynamicColors.SecondaryBackgroundColor = (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                _dynamicColors.BrandedPrimaryBackgroundColor = (Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush).Color;
                _dynamicColors.BrandedSecondaryBackgroundColor = (Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush).Color;
                _dynamicColors.AccentColor = (SolidColorBrush)Application.Current.Resources["SystemColorHighlightBrush"];
                _dynamicColors.BtnAccentColor = (SolidColorBrush)Application.Current.Resources["SystemColorHighlightBrush"];
                _dynamicColors.BtnAccentTextColor = new SolidColorBrush(ColorConverter.ToColor("#000000"));
                _dynamicColors.AccentHover = (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                _dynamicColors.AccentPressed = (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                _dynamicColors.DashboardBtnAccentColor = (SolidColorBrush)Application.Current.Resources["SystemColorHighlightBrush"];
                _dynamicColors.DashboardBtnTextColor = new SolidColorBrush(ColorConverter.ToColor("#FFFFFF"));
                _dynamicColors.PrimaryTextColor = new SolidColorBrush(ColorConverter.ToColor("#FFFFFF"));
                _dynamicColors.SecondaryTextColor = new SolidColorBrush(ColorConverter.ToColor("#FFFFFF"));
                _dynamicColors.BorderColor = ColorConverter.ToColor("#000000");
                _dynamicColors.BorderBrush = new SolidColorBrush(ColorConverter.ToColor("#FFFFFF"));
                _dynamicColors.MenuBackgroundColor = (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                _dynamicColors.SelectedMenuItemColor = (SolidColorBrush)Application.Current.Resources["SystemColorHighlightBrush"];
                _dynamicColors.SelectedMenuItemBackgroundColor = (SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                _dynamicColors.SelectedMenuItemShadowColor = ColorConverter.ToColor("#000000");
            }
            if (accessibilitySettings.HighContrast && accessibilitySettings.HighContrastScheme == "High Contrast White")
            {
                _dynamicColors.PrimaryTextColor = _primaryTextColor;
                _dynamicColors.SecondaryTextColor = _secondaryTextColor;
                _dynamicColors.AccentColor = (SolidColorBrush)Application.Current.Resources["SystemColorHighlightBrush"];
                _dynamicColors.BorderBrush = new SolidColorBrush(ColorConverter.ToColor("#000000"));
                _dynamicColors.BtnAccentTextColor = new SolidColorBrush(ColorConverter.ToColor("#000000"));
                _dynamicColors.SelectedMenuItemColor = new SolidColorBrush(ColorConverter.ToColor("#000000"));
                _dynamicColors.SelectedMenuItemShadowColor = ColorConverter.ToColor("#000000");
                _dynamicColors.DashboardBtnTextColor = new SolidColorBrush(ColorConverter.ToColor("#FFFFFF"));
            }
        }
    }
}
