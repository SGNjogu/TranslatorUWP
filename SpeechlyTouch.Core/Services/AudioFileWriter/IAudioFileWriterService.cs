using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.AudioFileWriter
{
    public interface IAudioFileWriterService
    {
        void Initialize(string waveFilePath);

        void Write(byte[] bytes, int byteCount);

        Task SaveFile();
    }
}
