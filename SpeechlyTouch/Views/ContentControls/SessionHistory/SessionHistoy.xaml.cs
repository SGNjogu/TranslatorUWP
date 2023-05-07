using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SpeechlyTouch.Views.ContentControls.SessionHistory
{
    public sealed partial class SessionHistoy : Page
    {
        public SessionHistoy()
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

        private void searchBox_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            searchBox.BorderThickness = new Thickness(0);
        }

        private async void HistoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(1000);
            HistoriesList.SelectedItem = null;
        }
    }
}
