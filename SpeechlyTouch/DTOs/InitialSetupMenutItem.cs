using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace SpeechlyTouch.DTOs
{
    public class InitialSetupMenutItem : ObservableObject
    {
        private double _opacity;
        public double Opacity
        {
            get { return _opacity; }
            set { SetProperty(ref _opacity, value); }
        }

        public string Title { get; set; }
        public int Number { get; set; }
        public string Ellipsis { get; set; }
    }
}
