using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.Views.ContentControls.Chat
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PersonOne { get; set; }
        public DataTemplate PersonTwo { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var chat = item as Models.Chat;

            if (chat.IsPersonOne)
                return PersonOne;
            return PersonTwo;
        }
    }
}
