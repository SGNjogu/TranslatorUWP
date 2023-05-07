using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SpeechlyTouch.Views.ContentControls
{
    public sealed partial class TitleView : UserControl
    {
        public TitleView()
        {
            this.InitializeComponent();
        }

        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            titleView.Width = e.NewSize.Width;
        }
    }
}
