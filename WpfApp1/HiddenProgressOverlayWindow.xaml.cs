using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class HiddenProgressOverlayWindow : Window
    {
        public bool isHidden = false;
        public HiddenProgressOverlayWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion
    }
    public partial class HiddenProgressOverlayWindow : Window
    {
        public void setLightColors()
        {
            Background.Background = Brushes.White;
            ProgressBar.Foreground = SourceChord.FluentWPF.AccentColors.ImmersiveSystemAccentBrush;
            ProgressBar.Background = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128));
            ProgressPercent.Foreground = Brushes.Black;
            ProgressText.Foreground = Brushes.Black;
            ProgressTitle.Foreground = Brushes.Black;
        }
        public void setDarkColors()
        {
            Background.Background = Brushes.Black;
            ProgressBar.Foreground = SourceChord.FluentWPF.AccentColors.ImmersiveSystemAccentBrush;
            ProgressBar.Background = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128));
            ProgressPercent.Foreground = Brushes.White;
            ProgressText.Foreground = Brushes.White;
            ProgressTitle.Foreground = Brushes.White;
        }
    }

    public partial class HiddenProgressOverlayWindow : Window
    {
        public void setProgress(int progressPercent, string progressString)
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (progressString != null)
                        ProgressText.Text = progressString;
                    ProgressBar.Value = progressPercent;
                    ProgressPercent.Text = progressPercent.ToString() + "%";
                }
                catch
                {
                    Close();
                }
            }));
        }
    }
}
