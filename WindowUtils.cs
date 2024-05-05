using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
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

        public static void CreateTrayIcon(Window window, System.Drawing.Icon? icon, string appName)
        {
            if (_notifyIcon != null || icon == null) return;
            _notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Text = $"{appName}: Click to show"
            };
            _notifyIcon.Click += (sender, eventArgs) => { Restore(window, true); };
            _notifyIcon.Visible = true;
        }

        /**
         * Run this on OnClosing(), override it on Window.
         */
        public static void DestroyTrayIcon()
        {
            if (_notifyIcon == null) return;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
        // endregion

        // region Mutex
        private static Mutex? _mutex = null;

        public static void CheckIfAlreadyRunning(string appName)
        {
            _mutex = new Mutex(true, appName, out var createdNew);
            if (createdNew) return;
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if(mainWindow != null) {
                System.Windows.MessageBox.Show(
                    mainWindow,
                    "This application is already running!",
                    appName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            System.Windows.Application.Current.Shutdown();
        }
        // endregion

        // region Window State
        public static void Minimize(Window window, bool onTaskbar)
        {
            window.Hide();
            window.WindowStyle = WindowStyle.ToolWindow;
            window.WindowState = WindowState.Minimized;
            window.ShowInTaskbar = onTaskbar;
        }

        public static void Restore(Window window, bool onTaskbar)
        {
            window.WindowStyle = WindowStyle.ThreeDBorderWindow;
            window.WindowState = WindowState.Normal;
            window.ShowInTaskbar = true;
            window.Show();
            window.Activate();
        }

        /**
         * Run this on OnStateChanged(), override it on Window.
         */
        public static void OnStateChange(Window window, bool onTaskbar)
        {
            switch (window.WindowState)
            {
                case WindowState.Minimized: // For tray icon
                    window.WindowStyle = WindowStyle.ToolWindow;
                    window.ShowInTaskbar = onTaskbar;
                    break;
                default:
                    window.WindowStyle = WindowStyle.ThreeDBorderWindow;
                    window.ShowInTaskbar = true;
                    break;
            }
        }

        // endregion
    }
}