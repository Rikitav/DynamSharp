using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SharpIdeMini.ApplicationInterface.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //Dispatcher.Invoke(() => SetWindowDarkMode(this, true));
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Exception exception = (Exception)e.ExceptionObject;
                string MessageContents = string.Format("Program faulted by unhandled exception :\n{0} : {1}\nSee detailed information in \"latest.log\"", exception.GetType().Name, exception.Message);
                MessageBox.Show(MessageContents);
                File.WriteAllText("latest.log", exception.ToString());
                Environment.Exit(exception.HResult);
            }
        }

        public static bool SetWindowDarkMode(Window appWindow, bool enabled)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                return false;

            int attribute = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985)
                ? NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE
                : NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;

            int useImmersiveDarkMode = enabled ? 1 : 0;
            HwndSource appHwnd = (HwndSource)PresentationSource.FromVisual(appWindow);
            return NativeMethods.DwmSetWindowAttribute(appHwnd.Handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;

        }

        private static class NativeMethods
        {
            public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
            public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

            [DllImport("dwmapi.dll")]
            public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        }
    }
}
