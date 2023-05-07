using SpeechlyTouch.Helpers;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ColorConverter = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartupPage : Page
    {
        public StartupPage()
        {
            this.InitializeComponent();
            ChangeTitleBarColor();
        }

        private void ChangeTitleBarColor()
        {
            var hoverColor = ColorConverter.ToColor("#F5E3F7");
            var backgroundColor = ColorConverter.ToColor("#e7edfd");
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            applicationView.TitleBar.BackgroundColor = backgroundColor;
            applicationView.TitleBar.ButtonBackgroundColor = backgroundColor;
            applicationView.TitleBar.ButtonInactiveBackgroundColor = backgroundColor;
            applicationView.TitleBar.ButtonForegroundColor = Colors.Black;
            applicationView.TitleBar.InactiveForegroundColor = Colors.Black;
            applicationView.TitleBar.ButtonHoverBackgroundColor = hoverColor;
            applicationView.TitleBar.ButtonHoverForegroundColor = Colors.Black;
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Hide the default TitleBar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            TitleBar.LoadTitleBar();
        }
    }
}
