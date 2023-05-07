using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SpeechlyTouch.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        private int val;
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            
            val = (int)value;

            return (( val) == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
