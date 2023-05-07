using SpeechlyTouch.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Popups
{
    public sealed partial class AppUpdateDialog : ContentDialog
    {
        private AppUpdateViewModel _dataContext;
        public AppUpdateDialog()
        {
            this.InitializeComponent();
            _dataContext = DataContext as AppUpdateViewModel;
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            _dataContext.AssignContent();
        }
    }
}
