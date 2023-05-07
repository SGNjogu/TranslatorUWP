
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace SpeechlyTouch.DTOs
{
    public class MenuItem : ObservableObject
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _glyph;
        public string Glyph
        {
            get { return _glyph; }
            set { SetProperty(ref _glyph, value); }
        }

        private SolidColorBrush _foreground;
        public SolidColorBrush Foreground
        {
            get { return _foreground; }
            set { SetProperty(ref _foreground, value); }
        }

        private SolidColorBrush _background;
        public SolidColorBrush Background
        {
            get { return _background; }
            set { SetProperty(ref _background, value); }
        }

        private Color _shadowColor;
        public Color ShadowColor
        {
            get { return _shadowColor; }
            set { SetProperty(ref _shadowColor, value); }
        }
    }
}
