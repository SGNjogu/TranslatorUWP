using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Connectivity
{
    public class ConnectivityService : IConnectivityService
    {
        private BackgroundWorker BackgroundWorkerClient;
        public event ConnectionChangedEvent ConnectionChangedEvent;
        public bool IsInternetConnectionAvailable { get; set; }

        public ConnectivityService()
        {
            BackgroundWorkerClient = new BackgroundWorker();
            BackgroundWorkerClient.DoWork += BackgroundWorkerClient_DoWork;
            BackgroundWorkerClient.RunWorkerAsync();
        }

        private double CheckInternetSpeed()
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Reset();
                stopwatch.Start();

                WebClient wc = new WebClient();
                byte[] bytes = wc.DownloadData("https://www.google.com/");

                stopwatch.Stop();

                double seconds = stopwatch.Elapsed.TotalSeconds;

                if (seconds > 0)
                {
                    double speed = bytes.Count() / seconds;

                    var result = (speed / 1024) * 20;

                    return result;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }

        }

        private async void BackgroundWorkerClient_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                await DoWork();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public bool IsConnectionAvailable()
        {
            try
            {
                IsInternetConnectionAvailable = NetworkInterface.GetIsNetworkAvailable();
                return IsInternetConnectionAvailable;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsConnectionStable()
        {
            var speed = CheckInternetSpeed();

            if (Math.Truncate(Convert.ToDecimal(speed)) < 500)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task DoWork()
        {
            ConnectionChangedEventArgs eventArgs = new ConnectionChangedEventArgs();

            if (IsConnectionAvailable())
            {
                eventArgs.ConnectionState = ConnectionState.ConnectionPresent;
                if (IsConnectionStable())
                {
                    eventArgs.ConnectionState = ConnectionState.ConnectionStable;
                }
                else
                {
                    eventArgs.ConnectionState = ConnectionState.ConnectionSlow;
                }
            }
            else
            {
                eventArgs.ConnectionState = ConnectionState.ConnectionLost;
            }

            ConnectionChangedEvent?.Invoke(this, eventArgs);

            await Task.Delay(3000);
            await DoWork();
        }
    }
}
