using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HelpPage : Page
    {
        private HelpViewModel _dataContext;
        private bool IsInitialized { get; set; } = false;

        public HelpPage()
        {
            this.InitializeComponent();
            _dataContext = DataContext as HelpViewModel;
            _dataContext.ContentFrame = contentFrame;
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
            if (!IsInitialized)
            {
                StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadAboutView = true });
                IsInitialized = true;
            }
        }
    }
}
