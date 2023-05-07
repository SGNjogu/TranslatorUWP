using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.Services.Popup
{
    public interface IDialogService
    {
        void HideDialog();
        Task ShowDialog(ContentDialog contentDialog);
    }
}
