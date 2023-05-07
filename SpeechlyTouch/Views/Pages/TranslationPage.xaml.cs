using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TranslationPage : Page
    {
        private TranslationViewModel _dataContext;
        private object _lockObject = new object();

        public TranslationPage()
        {
            this.InitializeComponent();
            _dataContext = DataContext as TranslationViewModel;
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            StrongReferenceMessenger.Default.Send(new NavigationMessage { RegisterAppCloseEvent = true });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            StrongReferenceMessenger.Default.Send(new NavigationMessage { UnRegisterAppCloseEvent = true });
        }

        private void KeyboardAccelerator_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            lock (_lockObject)
            {
                if (sender.Key == Windows.System.VirtualKey.Number1) _dataContext.AskQuestion(1); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number2) _dataContext.AskQuestion(2); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number3) _dataContext.AskQuestion(3); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number4) _dataContext.AskQuestion(4); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number5) _dataContext.AskQuestion(5); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number6) _dataContext.AskQuestion(6); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number7) _dataContext.AskQuestion(7); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number8) _dataContext.AskQuestion(8); args.Handled = true;
                if (sender.Key == Windows.System.VirtualKey.Number9) _dataContext.AskQuestion(9); args.Handled = true;
            }
        }
    }
}
