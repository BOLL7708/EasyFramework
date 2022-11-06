using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BOLL7708
{
    public static class WindowUtils
    {
        // region Tray Icon
        /**
         * Import an icon as a resource in the resource editor, then import it with the below:
         * - Properties.Resources.Logo.Clone() as System.Drawing.Icon;
         */
        private static System.Windows.Forms.NotifyIcon? _notifyIcon;
        public static void CreateTrayIcon(Window window, System.Drawing.Icon icon, string appName) {
            if (_notifyIcon != null) return;
            _notifyIcon = new()
            {
                Icon = icon,
                Text = $"{appName}: Click to show"
            };
            _notifyIcon.Click += (sender, eventArgs) =>
            {
                window.WindowState = WindowState.Normal;
                window.ShowInTaskbar = true;
                window.Show();
                window.Activate();
            };
            _notifyIcon.Visible = true;
        }
        /**
         * Run this on OnClosing(), override it on Window.
         */
        public static void DestroyTrayIcon() {
            if (_notifyIcon != null) _notifyIcon.Dispose();
        }
        // endregion

        // region Mutex
        private static Mutex _mutex = null;
        public static void CheckIfAlreadyRunning(string appName) {
            _mutex = new Mutex(true, appName, out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show(
                Application.Current.MainWindow,
                "This application is already running!",
                appName,
                MessageBoxButton.OK,
                MessageBoxImage.Information
                );
                Application.Current.Shutdown();
            }
        }
        // endregion

        // region Minimize
        public static void Minimize(Window window, bool onTaskbar)
        {
            window.Hide();
            window.WindowState = WindowState.Minimized;
            window.ShowInTaskbar = onTaskbar;
            window.Visibility = onTaskbar ? Visibility.Visible : Visibility.Hidden;
        }

        /**
         * Run this on OnStateChanged(), override it on Window.
         */
        public static void OnStateChange(Window window, bool onTaskbar)
        {
            switch (window.WindowState)
            {
                case WindowState.Minimized: // For tray icon
                    window.ShowInTaskbar = onTaskbar;
                    window.Visibility = onTaskbar ? Visibility.Visible : Visibility.Hidden;
                    break; 
                default: 
                    window.ShowInTaskbar = true;
                    window.Visibility = Visibility.Visible;
                    window.Show(); 
                    break;
            }
        }

        // endregion
    }
}
