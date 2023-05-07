using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class AudioDownloadViewModel : ObservableObject
    {

        public static string ResetDownloadCount = "0 / 0 kb";
        private int _downloadProgress;
        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                SetProperty(ref _downloadProgress, value);
            }
        }

        private string _downloadStatus;
        public string DownloadStatus
        {
            get { return _downloadStatus; }
            set
            {
                SetProperty(ref _downloadStatus, value);
            }
        }

        private string _downloadSize;
        public string DownloadSize
        {
            get { return _downloadSize; }
            set
            {
                SetProperty(ref _downloadSize, value);
            }
        }

        private string _downloadCount;
        public string DownloadCount
        {
            get { return _downloadCount; }
            set
            {
                SetProperty(ref _downloadCount, value);
            }
        }

        private Visibility _downloadControlVisibility;
        public Visibility DownloadControlVisibility
        {
            get { return _downloadControlVisibility; }
            set { SetProperty(ref _downloadControlVisibility, value); }
        }

        private Visibility _resumeButtonVisibility;
        public Visibility ResumeButtonVisibility
        {
            get { return _resumeButtonVisibility; }
            set { SetProperty(ref _resumeButtonVisibility, value); }
        }

        private Visibility _pauseButtonVisibility;
        public Visibility PauseButtonVisibility
        {
            get { return _pauseButtonVisibility; }
            set { SetProperty(ref _pauseButtonVisibility, value); }
        }

        private ResourceLoader _resourceLoader;
        private DownloadOperation downloadOperation;
        private CancellationTokenSource cancellationToken;
        private BackgroundDownloader backgroundDownloader;
        public AudioDownloadViewModel()
        {
            DownloadControlVisibility = Visibility.Collapsed;
            PauseButtonVisibility = Visibility.Collapsed;
            ResumeButtonVisibility = Visibility.Collapsed;
            backgroundDownloader = new BackgroundDownloader();
            _resourceLoader = ResourceLoader.GetForCurrentView();
            StrongReferenceMessenger.Default.Register<DownloadMessage>(this, (r, m) =>
            {
               _ = HandleMessage(m);
            });
        }

        private async Task HandleMessage(DownloadMessage message)
        {
            if(message.SessionNumber != null && message.SessionNumber != null)
            {
               DownloadControlVisibility = Visibility.Visible;
               await DownloadSessionAudio(message.Uri, message.SessionNumber);
            }
        }

        private async Task DownloadSessionAudio(Uri uri, string sessionNumber)
        {
            DownloadStatus = _resourceLoader.GetString("Downloading");
            DownloadProgress = 0;
            DownloadCount = ResetDownloadCount;
            if (uri == null)
                return;

            StorageFile file = await DownloadsFolder.CreateFileAsync(sessionNumber + ".wav", CreationCollisionOption.GenerateUniqueName);
            downloadOperation = backgroundDownloader.CreateDownload(uri, file);
            downloadOperation.Priority = BackgroundTransferPriority.High;
            Progress<DownloadOperation> progress = new Progress<DownloadOperation>(progressChanged);
            cancellationToken = new CancellationTokenSource();
            try
            {
                await downloadOperation.StartAsync().AsTask(cancellationToken.Token, progress);
            }
            catch (TaskCanceledException)
            {
                await downloadOperation.ResultFile.DeleteAsync();
                downloadOperation = null;
            }


        }

        private void progressChanged(DownloadOperation downloadOperation)
        {
            DownloadProgress = (int)(100 * ((double)downloadOperation.Progress.BytesReceived / (double)downloadOperation.Progress.TotalBytesToReceive));
            DownloadCount = String.Format("{0} / {1} kb ", downloadOperation.Progress.BytesReceived / 1024, downloadOperation.Progress.TotalBytesToReceive / 1024);
            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    {
                        DownloadStatus = _resourceLoader.GetString("Downloading");
                        PauseButtonVisibility = Visibility.Visible;
                        break;
                    }
                case BackgroundTransferStatus.PausedByApplication:
                    {
                        DownloadStatus = _resourceLoader.GetString("DownloadPaused");
                        break;
                    }
                case BackgroundTransferStatus.PausedCostedNetwork:
                    {
                        DownloadStatus = _resourceLoader.GetString("DownloadPaused");
                        break;
                    }
                case BackgroundTransferStatus.PausedNoNetwork:
                    {
                        DownloadStatus = _resourceLoader.GetString("DownloadPaused");
                        break;
                    }
                case BackgroundTransferStatus.Error:
                    {
                        DownloadStatus = _resourceLoader.GetString("DownloadError");
                        break;
                    }
            }
            if (DownloadProgress >= 100)
            {
                DownloadStatus = _resourceLoader.GetString("Downloaded");
                ResumeButtonVisibility = Visibility.Collapsed;
                PauseButtonVisibility = Visibility.Collapsed;
                StrongReferenceMessenger.Default.Send(new DownloadMessage { DownloadButtonVisibility = Visibility.Visible });
                
                downloadOperation = null;
            }
            
        }

        private void PauseDownload()
        {
            
                try
                {
                    ResumeButtonVisibility = Visibility.Visible;
                PauseButtonVisibility = Visibility.Collapsed;
                    downloadOperation.Pause();
                }
                catch (InvalidOperationException)
                {

                }
           
        }

        private void ResumeDownload()
        {
            if(downloadOperation != null)
            {
                try
                {
                    ResumeButtonVisibility = Visibility.Collapsed;
                    PauseButtonVisibility = Visibility.Visible;
                    downloadOperation.Resume();
                }
                catch (InvalidOperationException)
                {

                }
            }
        }

        private void CancelDownload()
        {
            if(DownloadProgress >= 100)
            {
                DownloadControlVisibility = Visibility.Collapsed;
                StrongReferenceMessenger.Default.Send(new DownloadMessage { DownloadButtonVisibility = Visibility.Visible });

            }
           
            if(DownloadProgress < 100 && cancellationToken.Token.CanBeCanceled)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
                DownloadControlVisibility = Visibility.Collapsed;
                DownloadCount = ResetDownloadCount;
                StrongReferenceMessenger.Default.Send(new DownloadMessage { DownloadButtonVisibility = Visibility.Visible });
                StrongReferenceMessenger.Default.Send(new NotificationMessage { DisplayMessage = _resourceLoader.GetString("DownloadCancel") });


            }
        }


        private RelayCommand _pauseDownloadCommand = null;
        public RelayCommand PauseDownloadCommand
        {
            get
            {
                return _pauseDownloadCommand ?? (_pauseDownloadCommand = new RelayCommand(() => { PauseDownload(); }));
            }
        }

        private RelayCommand _resumeDownloadCommand = null;
        public RelayCommand ResumeDownloadCommand
        {
            get
            {
                return _resumeDownloadCommand ?? (_resumeDownloadCommand = new RelayCommand(() => { ResumeDownload(); }));
            }
        }

        private RelayCommand _cancelHideDownloadCommand = null;
        public RelayCommand CancelHideDownloadCommand
        {
            get
            {
                return _cancelHideDownloadCommand ?? (_cancelHideDownloadCommand = new RelayCommand(() => { CancelDownload(); }));
            }
        }



    }

}
