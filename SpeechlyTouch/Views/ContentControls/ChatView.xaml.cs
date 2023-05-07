using Microsoft.Toolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using SpeechlyTouch.Messages;
using SpeechlyTouch.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SpeechlyTouch.Views.ContentControls
{
    public sealed partial class ChatView : UserControl
    {
        private double ScrollViwerHeight;
        public static bool IsSingleChat { get; set; }
        private ScrollViewer _scrollViewer;
        private int CurrentSessionId { get; set; }
        private bool IsCopyPaseEnabled { get; set; }
        public static Models.Chat Chat;


        private ChatViewModel _dataContext;
        private bool IsShowingImmersiveReader { get; set; }

        public ChatView()
        {
            this.InitializeComponent();
            _dataContext = DataContext as ChatViewModel;
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void UserControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StrongReferenceMessenger.Default.Register<ChatScrollToMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<RecognizedChatMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
        }

        private void HandleMessage(NavigationMessage m)
        {
            if (m.CloseImmersiveReader)
                IsShowingImmersiveReader = false;
        }

        private void HandleMessage(RecognizedChatMessage m)
        {
            IsCopyPaseEnabled = m.IsCopyPasteEnabled;
        }

        private void HandleMessage(ChatScrollToMessage message)
        {
            if (!message.IsPercentage && message.ScrollTo != null)
            {
                listView.ScrollIntoView(message.ScrollTo);
            }
            else if (message.IsPercentage)
            {
                AutoScrollChats(message.Percentage);
            }
        }

        private void Chats_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _scrollViewer = GetScrollViewer(listView) as ScrollViewer;
            ScrollViwerHeight = _scrollViewer.ActualHeight;
        }

        void AutoScrollChats(double percentage)
        {
            if (_scrollViewer != null)
            {
                var verticalOffset = (percentage * ScrollViwerHeight) / 100;
                _scrollViewer.ChangeView(null, verticalOffset, null);
            }
        }

        private DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        private void ListView_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if (IsCopyPaseEnabled)
            {
                ListView listView = (ListView)sender;
                menuFlyout.ShowAt(listView, e.GetPosition(listView));
                Chat = ((FrameworkElement)e.OriginalSource).DataContext as Models.Chat;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (Chat != null)
            {
                string copyText = $"{Chat.Person}\n{Chat.Message}\n";

                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(copyText);
                Clipboard.SetContent(dataPackage);
            }

            Chat = null;
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            IsSingleChat = true;
            DataTransferManager.ShowShareUI();
        }

        private void ListView_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            if (IsCopyPaseEnabled)
            {
                ListView listView = (ListView)sender;
                menuFlyout.ShowAt(listView, e.GetPosition(listView));
                Chat = ((FrameworkElement)e.OriginalSource).DataContext as Models.Chat;
            }
        }

        private async void CollectTranscriptions()
        {
            List<ImmersiveReaderRequest> request = new List<ImmersiveReaderRequest>();

            if (listView.Items != null)
            {
                foreach (var item in listView.Items)
                {
                    Models.Chat chat = (Models.Chat)item;
                    request.AddRange(new List<ImmersiveReaderRequest>() {
                        new ImmersiveReaderRequest()
                            {
                                Content = chat.OriginalMessage,
                                LanguageCode = chat.OriginalMessageISO,
                            },
                            new ImmersiveReaderRequest()
                            {
                                Content = chat.TranslatedMessage,
                                LanguageCode = chat.TranslatedMessageISO,
                            }
                    });
                }
            }

            await ImmersiveRead(request);
        }

        private async Task ImmersiveRead(List<ImmersiveReaderRequest> request)
        {
            if (!IsShowingImmersiveReader)
            {
                string message = "";
                Uri uri = new Uri("about:blank");
                try
                {
                    IsShowingImmersiveReader = true;
                    string endpoint = $"{Constants.BackendAPiEndpoint}{Constants.ImmersiveReaderEndpoint}";
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _dataContext._authService.IdToken);
                    HttpResponseMessage response = await client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        message = "Loading, please wait...";
                        uri = new Uri($"{endpoint}/{long.Parse(content)}");
                    }
                    else
                    {
                        message = "Loading, please wait...";
                    }

                    _dataContext.immersiveReaderDialog = new Popups.ImmersiveReaderDialog();
                    StrongReferenceMessenger.Default.Send(new ImmersiveReaderNavigationMessage { Message = message, SourceUri = uri });
                    await _dataContext.ShowImmersiveReader();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                message = "Loading, please wait...";
            }
        }

        void ImmersiveRead(object sender, RoutedEventArgs e)
        {
            CollectTranscriptions();
        }

        public class ImmersiveReaderRequest
        {
            public string Content { get; set; } = "";
            public string LanguageCode { get; set; } = "en-us";
        }
    }
}
