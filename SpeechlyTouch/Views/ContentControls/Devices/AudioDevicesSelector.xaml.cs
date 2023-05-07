using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SpeechlyTouch.Views.ContentControls.Devices
{
    public sealed partial class AudioDevicesSelector : UserControl
    {
        public AudioDevicesSelector()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StrongReferenceMessenger.Default.Send(new DevicesMessage { ReloadAudioInputDevices = true, ReloadAudioOuputDevices = true });
        }
    }
}
