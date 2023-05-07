using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SpeechlyTouch.Views.ContentControls.InitialSetup
{
    public sealed partial class Network : Page
    {
        public Network()
        {
            this.InitializeComponent();
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void ScrollToEndButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.ChangeView(null, 100, null);

            if (NetworksList.Items.Count == 0)
                return;

            var lastItem = NetworksList.Items[NetworksList.Items.Count - 1];
            NetworksList.ScrollIntoView(lastItem);
        }
    }
}
