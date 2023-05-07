using SpeechlyTouch.Views.ContentControls;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using ColorConverter = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

namespace SpeechlyTouch.Helpers
{
    public static class TitleBar
    {
        public static void LoadTitleBar()
        {
            var titleBar = new TitleView();
            var hoverColor = ColorConverter.ToColor("#F5E3F7");
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            applicationView.TitleBar.BackgroundColor = Colors.White;
            applicationView.TitleBar.ButtonBackgroundColor = Colors.White;
            applicationView.TitleBar.ButtonInactiveBackgroundColor = Colors.White;
            applicationView.TitleBar.ButtonForegroundColor = Colors.Black;
            applicationView.TitleBar.InactiveForegroundColor = Colors.Black;
            applicationView.TitleBar.ButtonHoverBackgroundColor = hoverColor;
            applicationView.TitleBar.ButtonHoverForegroundColor = Colors.Black;
            Window.Current.SetTitleBar(titleBar);
        }
    }
}
