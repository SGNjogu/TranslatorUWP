using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.DataSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechlyTouch.ViewModels
{
    public class FeedbackSubmittedViewModel
    {
        private readonly IPushDataService _pushDataService;

        public FeedbackSubmittedViewModel(IPushDataService pushDataService)
        {
            _pushDataService = pushDataService;

        }


        private RelayCommand _dismissFeedbackCommand = null;

        public RelayCommand DismissFeedbackCommand
        {
            get
            {
                return _dismissFeedbackCommand ?? (_dismissFeedbackCommand = new RelayCommand(() => { Dismiss(); }));
            }
        }

        private void Dismiss()
        {
            Thread pushThread = new Thread(_pushDataService.BeginDataSync);
            pushThread.Start();
            StrongReferenceMessenger.Default.Send(new FeedbackDialogMessage { ShowFeedbackSubmitted = false });
        }
    }
}
