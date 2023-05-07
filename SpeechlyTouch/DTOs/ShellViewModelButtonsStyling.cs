
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml.Media;
using ColorConverter = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

namespace SpeechlyTouch.DTOs
{
    public class ShellViewModelButtonsStyling : ObservableObject
    {
        public bool IsHistoryActive { get; set; } = false;
        public bool IsSettingsActive { get; set; } = false;
        public bool IsHelpActive { get; set; }

        private Brush _historyTextBtnColor;
        public Brush HistoryTextBtnColor
        {
            get { return _historyTextBtnColor; }
            set { SetProperty(ref _historyTextBtnColor, value); }
        }

        private Brush _historyFontIconBtnColor;
        public Brush HistoryFontIconBtnColor
        {
            get { return _historyFontIconBtnColor; }
            set { SetProperty(ref _historyFontIconBtnColor, value); }
        }

        private Brush _historyEllipseBtnColor;
        public Brush HistoryEllipseBtnColor
        {
            get { return _historyEllipseBtnColor; }
            set { SetProperty(ref _historyEllipseBtnColor, value); }
        }


        private Brush _settingsTextBtnColor;
        public Brush SettingsTextBtnColor
        {
            get { return _settingsTextBtnColor; }
            set { SetProperty(ref _settingsTextBtnColor, value); }
        }

        private Brush _settingsFontIconBtnColor;
        public Brush SettingsFontIconBtnColor
        {
            get { return _settingsFontIconBtnColor; }
            set { SetProperty(ref _settingsFontIconBtnColor, value); }
        }

        private Brush _settingsEllipseBtnColor;
        public Brush SettingsEllipseBtnColor
        {
            get { return _settingsEllipseBtnColor; }
            set { SetProperty(ref _settingsEllipseBtnColor, value); }
        }


        private Brush _helpTextBtnColor;
        public Brush HelpTextBtnColor
        {
            get { return _helpTextBtnColor; }
            set { SetProperty(ref _helpTextBtnColor, value); }
        }

        private Brush _helpFontIconBtnColor;
        public Brush HelpFontIconBtnColor
        {
            get { return _helpFontIconBtnColor; }
            set { SetProperty(ref _helpFontIconBtnColor, value); }
        }

        private Brush _helpEllipseBtnColor;
        public Brush HelpEllipseBtnColor
        {
            get { return _helpEllipseBtnColor; }
            set { SetProperty(ref _helpEllipseBtnColor, value); }
        }

        private Brush UnSelectedMenuItemFontIconColor = new SolidColorBrush(ColorConverter.ToColor("#02175d"));
        private Brush UnSelectedMenuItemTextColor = new SolidColorBrush(ColorConverter.ToColor("#7781BE"));
        private Brush UnSelectedMenuItemEllipseColor = new SolidColorBrush(ColorConverter.ToColor("#DEE0EF"));

        private Brush SelectedMenuItemFontIconColor = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        private Brush SelectedMenuItemTextColor = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        private Brush SelectedMenuItemEllipseColor = new SolidColorBrush(ColorConverter.ToColor("#F0D4F1"));

        public ShellViewModelButtonsStyling()
        {
            Reset();
        }

        private void Reset()
        {
            HistoryEllipseBtnColor = UnSelectedMenuItemEllipseColor;
            HistoryFontIconBtnColor = UnSelectedMenuItemFontIconColor;
            HistoryTextBtnColor = UnSelectedMenuItemTextColor;
            IsHistoryActive = false;

            SettingsEllipseBtnColor = UnSelectedMenuItemEllipseColor;
            SettingsFontIconBtnColor = UnSelectedMenuItemFontIconColor;
            SettingsTextBtnColor = UnSelectedMenuItemTextColor;
            IsSettingsActive = false;

            HelpEllipseBtnColor = UnSelectedMenuItemEllipseColor;
            HelpFontIconBtnColor = UnSelectedMenuItemFontIconColor;
            HelpTextBtnColor = UnSelectedMenuItemTextColor;
            IsHelpActive = false;
        }

        public void SelectHistory()
        {
            Reset();
            IsHistoryActive = true;
            HistoryEllipseBtnColor = SelectedMenuItemEllipseColor;
            HistoryFontIconBtnColor = SelectedMenuItemFontIconColor;
            HistoryTextBtnColor = SelectedMenuItemTextColor;
        }

        public void SelectSettings()
        {
            Reset();
            IsSettingsActive = true;
            SettingsEllipseBtnColor = SelectedMenuItemEllipseColor;
            SettingsFontIconBtnColor = SelectedMenuItemFontIconColor;
            SettingsTextBtnColor = SelectedMenuItemTextColor;
        }

        public void SelectHelp()
        {
            Reset();
            IsHelpActive = true;
            HelpEllipseBtnColor = SelectedMenuItemEllipseColor;
            HelpFontIconBtnColor = SelectedMenuItemFontIconColor;
            HelpTextBtnColor = SelectedMenuItemTextColor;
        }
    }
}
