using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;

namespace SpeechlyTouch.Logging
{
    public interface ICrashlytics
    {
        Task<ErrorAttachmentLog[]> Attachments();
    }
}
