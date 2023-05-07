using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SpeechlyTouch.Services.Popup
{
    public class DialogService : IDialogService
    {
        private ContentDialog _contentDialog;
        private Queue<ContentDialog> _dialogQueue;
        public DialogService()
        {
            _contentDialog = new ContentDialog();
            _dialogQueue = new Queue<ContentDialog>();
        }
        public async Task ShowDialog(ContentDialog contentDialog)
        {
            _dialogQueue.Enqueue(contentDialog); 
           await HandleQueuedDialogs();
           
        }
        private async Task HandleQueuedDialogs()
        {
            if (_dialogQueue.Count < 1) return;
            var firstQueuedDialog = _dialogQueue.Dequeue();
            firstQueuedDialog.Closed += _contentDialog_Closed;
            var openedPopups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in openedPopups)
            {
                if (popup.Child is ContentDialog)
                {
                    _contentDialog.Hide();
                }
            }
            _contentDialog = firstQueuedDialog;
            await _contentDialog.ShowAsync();
        }

        private async void _contentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            await HandleQueuedDialogs();
        }

        public void HideDialog()
        {
            if (_contentDialog != null)
            {
                _contentDialog.Hide();
            }
        }
    }
}
