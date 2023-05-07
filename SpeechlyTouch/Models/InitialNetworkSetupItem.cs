using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace SpeechlyTouch.Models
{
    public class InitialNetworkSetupItem : ObservableObject
    {
        private Brush _foreground;
        public Brush Foreground
        {
            get { return _foreground; }
            set { SetProperty(ref _foreground, value); }
        }

        private WiFiNetwork _wiFiNetwork;
        public WiFiNetwork WiFiNetwork
        {
            get { return _wiFiNetwork; }
            set { SetProperty(ref _wiFiNetwork, value); }
        }

        private Visibility _isVisibleLockIcon;
        public Visibility IsVisibleLockIcon
        {
            get { return _isVisibleLockIcon; }
            set { SetProperty(ref _isVisibleLockIcon, value); }
        }
    }
}
