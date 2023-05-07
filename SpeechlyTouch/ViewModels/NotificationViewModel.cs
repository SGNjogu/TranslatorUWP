using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class NotificationViewModel : ObservableObject
    {
        private DispatcherTimer dispatcherTimer = null;
        private Visibility _notificationVisibility = Visibility.Collapsed;
        public Visibility NotificationVisibility
        {
            get { return _notificationVisibility; }
            set { SetProperty(ref _notificationVisibility, value); }
        }
        private string _notificationMessage;
        public string NotificationMessage
        {
            get { return _notificationMessage; }
            set { SetProperty(ref _notificationMessage, value); }
        }
        public NotificationViewModel()
        {
            dispatcherTimer = new DispatcherTimer();
            StrongReferenceMessenger.Default.Register<NotificationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
        }
        private void HandleMessage(NotificationMessage message)
        {
            NotificationMessage = message.DisplayMessage;
            CreateTimer();

            NotificationVisibility = message.Visible;
        }
        private void CloseNotification()
        {
            dispatcherTimer.Stop();
            dispatcherTimer = null;
            StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Collapsed });
        }
        public void CreateTimer()
        {
            if(dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += DispatcherTimer_Tick; ;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 4);
                dispatcherTimer.Start();
            }
        
        }
        private void DispatcherTimer_Tick(object sender, object e)
        {
            NotificationVisibility = Visibility.Collapsed;
            dispatcherTimer.Stop();
            dispatcherTimer = null;
        }
        private RelayCommand _closeNotificationCommand = null;
        public RelayCommand CloseNotificationCommand
        {
            get
            {
                return _closeNotificationCommand ?? (_closeNotificationCommand = new RelayCommand(() => { CloseNotification(); }));
            }
        }
    }
}

