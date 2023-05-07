using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellView : Page
    {
        private ShellViewModel _dataContext;

        public ShellView()
        {
            this.InitializeComponent();
            _dataContext = DataContext as ShellViewModel;
            _dataContext.MainFrame = mainFrame;
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadSettingsView = true });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StartIdleTimerCountDown();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _dataContext.AppIdleTimer.Stop();
            Window.Current.CoreWindow.PointerMoved -= OnCoreWindowPointerMoved;
        }

        public void StartIdleTimerCountDown()
        {
            _dataContext.StartAppIdleCountDown();
            Window.Current.CoreWindow.PointerMoved += OnCoreWindowPointerMoved;
        }

        private void OnCoreWindowPointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            _dataContext.AppIdleTimer.Stop();
            _dataContext.StartAppIdleCountDown();
        }
    }
}
