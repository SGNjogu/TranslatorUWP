using SpeechlyTouch.Models;
using System;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.SignalR
{
    public interface ISignalRService
    {
        string ConnectionId { get; }

        event Action<SignalRTranslateMessage> SignalRMessageReceived;

        Task ConnectSignalR(string token);
        Task DisconnectSignalR();
        Task SendSignalRMessage(SignalRTranslateMessage signalRTranslateMessage);
    }
}