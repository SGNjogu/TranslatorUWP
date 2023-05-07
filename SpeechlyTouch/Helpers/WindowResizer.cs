using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace SpeechlyTouch.Helpers
{
    public static class WindowResizer
    {
        private static float? DPI;
        private static Size? WindowSize;

        private static void GetLogicalDPI()
        {
            if (DPI == null)
            {
                DPI = DisplayInformation.GetForCurrentView().LogicalDpi;
            }

            GetMinimumWindowSize();
        }

        private static void GetMinimumWindowSize()
        {
            if (WindowSize == null)
            {
                int ouputWidth = ConvertDIPsToPixels(800);
                int ouputHeight = ConvertDIPsToPixels(600);
                WindowSize = new Size { Width = ouputWidth, Height = ouputHeight };
            }
        }

        private static float ConvertPixelsToDPIs(int pixels)
        {
            return (float)(pixels * 96f / DPI);
        }

        private static int ConvertDIPsToPixels(float dips)
        {
            return (int)(dips * DPI / 96f + 0.5f);
        }

        public static void TryResize()
        {
            GetLogicalDPI();
            var view = ApplicationView.GetForCurrentView();
            view.TryResizeView((Size)WindowSize);
        }

        public static void SetMinimumSize()
        {
            GetLogicalDPI();
            var view = ApplicationView.GetForCurrentView();
            // If this size is not permitted by the system, the nearest permitted value is used.
            view.SetPreferredMinSize((Size)WindowSize);
        }

        public static void SetPreferredLaunchSize()
        {
            GetLogicalDPI();
            // For best results, set the PreferredLaunchViewSize before setting
            // the PreferredLaunchWindowingMode.
            ApplicationView.PreferredLaunchViewSize = (Size)WindowSize;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        public static void ListenForWindowChanges()
        {
            Window.Current.SizeChanged += OnWindwSizeChanged;
        }

        private static void OnWindwSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width < WindowSize?.Width - 5 || e.Size.Height < WindowSize?.Height - 5)
            {
                TryResize();
            }
        }
    }
}
