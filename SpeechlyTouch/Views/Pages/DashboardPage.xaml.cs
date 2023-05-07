
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        private DashboardViewModel _dataContext;
        private ScrollViewer _scrollViewer;

        public DashboardPage()
        {
            this.InitializeComponent();
            StrongReferenceMessenger.Default.Register<ScrollToMessage>(this, (r, m) =>
            {
                ScrollIntoView(m.Index);
            });
            _dataContext = DataContext as DashboardViewModel;
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void gridView_Loaded(object sender, RoutedEventArgs e)
        {
            GetScrollViewer();

            if (_scrollViewer != null)
                _scrollViewer.DirectManipulationStarted += DirectManipulationStarted;
        }

        private void GetScrollViewer()
        {
            if (_scrollViewer == null)
                _scrollViewer = GetScrollViewer(gridView) as ScrollViewer;
        }

        private void RightScroll_Click(object sender, RoutedEventArgs e)
        {
            ScrollRight();
        }

        private void LeftScroll_Click(object sender, RoutedEventArgs e)
        {
            ScrollLeft();
        }

        private void LoadALlLanguages()
        {
            if (!_dataContext.IsSearching && _dataContext.QuickViewLanguages.Count <= 5)
            {
                _dataContext.QuickViewLanguages = new ObservableCollection<Models.Language>(_dataContext.Languages);
                gridView.ScrollIntoView(gridView.Items[1], ScrollIntoViewAlignment.Leading);
            }
        }

        private void ScrollIntoView(int index)
        {
            try
            {
                gridView.ScrollIntoView(gridView.Items[index], ScrollIntoViewAlignment.Default);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        private void ScrollLeft()
        {
            LoadALlLanguages();

            GetScrollViewer();

            if (_scrollViewer != null)
            {
                if (_scrollViewer.HorizontalOffset > 200)
                {
                    var horizontalOffset = _scrollViewer.HorizontalOffset - 200;
                    _scrollViewer.ChangeView(horizontalOffset, null, null);
                }
                else
                {
                    _scrollViewer.ChangeView(0, null, null);
                }
            }
        }

        private void ScrollRight()
        {
            LoadALlLanguages();

            GetScrollViewer();

            if (_scrollViewer != null)
            {
                var horizontalOffset = _scrollViewer.HorizontalOffset + 200;
                _scrollViewer.ChangeView(horizontalOffset, null, null);
            }
        }

        private void DirectManipulationStarted(object sender, object e)
        {
            LoadALlLanguages();
        }

        // Annimates scrolling
        //void ScrollPeriodically(ScrollViewer sv, double horizontalOffset)
        //{
        //    while (horizontalOffset != 0)
        //    {
        //        sv.ChangeView(sv.VerticalOffset + 1, null, null);
        //        horizontalOffset--;
        //    }
        //}
    }
}
