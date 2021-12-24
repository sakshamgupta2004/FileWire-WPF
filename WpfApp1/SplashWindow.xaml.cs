using FileWire;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        Dictionary<string, string> args;
        private static void AdminRelauncher(Dictionary<string, string> args)
        {
            if (!IsRunAsAdmin())
            {
                // relaunch the application with admin rights
                string fileName = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.CreateNoWindow = false;
                //processInfo.WindowStyle = ProcessWindowStyle.Maximized;
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runAs";
                processInfo.FileName = fileName;
                processInfo.Arguments = "";
                foreach (var arg in args)
                {
                    processInfo.Arguments += "--" + arg.Key + " " + "\"" + arg.Value + "\" ";
                }
                processInfo.Arguments += "--StartNumber Two";
                try
                {
                    Process.Start(processInfo);
                }
                catch (Win32Exception e)
                {
                    // This will be thrown if the user cancels the prompt
                }
                Environment.Exit(Environment.ExitCode);
                return;
            }
        }

        private static bool IsRunAsAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
            
        }




        public SplashWindow(Dictionary<String, String> arg)
        {
            args = arg;
            File.WriteAllBytes(new Preferences().settingsDirectory + "selectall.exe", Properties.Resources.singleinstance);
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\directory\shell", true))
                {
                    key.DeleteSubKeyTree("Send via FileWire");
                }
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\*\shell", true))
                {
                    key.DeleteSubKeyTree("Send via FileWire");
                }
            }
            catch
            {

            }
            try
            {
                var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\directory\shell");
                Properties.Resources.ic_logo_playstore.Save(File.Create(new Preferences().settingsDirectory + "appicon.ico"));
                var key1 = key.CreateSubKey("Send via FileWire");
                key1.SetValue("icon", new Preferences().settingsDirectory + "appicon.ico", RegistryValueKind.String);
                var key2 = key1.CreateSubKey("command");
                key2.SetValue("", new Preferences().settingsDirectory + "selectall.exe" + " \"%1\" \"" + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + "\" --SelectAllFiles $files --EndOfFiles");

                key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell");
                key1 = key.CreateSubKey("Send via FileWire");
                key1.SetValue("icon", new Preferences().settingsDirectory + "appicon.ico", RegistryValueKind.String);
                key2 = key1.CreateSubKey("command");
                key2.SetValue("", new Preferences().settingsDirectory + "selectall.exe" + " \"%1\" \"" + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + "\" --SelectAllFiles $files --EndOfFiles");

            }
            catch (Exception e)
            {
                new Preferences().printToLog(e.ToString());
            }

            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                if (key != null)
                {
                    foreach (var a in key.GetSubKeyNames())
                    {
                        var key1 = key.OpenSubKey(a, true);
                        if (key1.GetValue("DisplayName").ToString().StartsWith(AppDomain.CurrentDomain.FriendlyName) && a.ToLower().Equals("filewire-pc"))
                        {

                            /*key1.SetValue("EstimatedSize", (MainWindow.DirSize(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)) / 1024), RegistryValueKind.DWord);
                            key1.SetValue("DisplayIcon", new Preferences().settingsDirectory + "appicon.ico");*/
                            var originalUninstallStringFile = new Preferences().settingsDirectory + "originalUninstallString";
                            var uninstallString = key1.GetValue("UninstallString").ToString();
                            //rundll32.exe dfshim.dll,ShArpMaintain FileWire.application, Culture=neutral, PublicKeyToken=47b224adbda73958, processorArchitecture=x86

                            if (File.Exists(originalUninstallStringFile))
                            {
                                if (!uninstallString.EndsWith(AppDomain.CurrentDomain.FriendlyName + ".exe --Uninstall true"))
                                {
                                    var fs = File.Create(originalUninstallStringFile);
                                    var tw = Encoding.UTF8.GetBytes(uninstallString);
                                    fs.Write(tw, 0, tw.Length);
                                    fs.Close();
                                }
                                key1.SetValue("UninstallString", AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe --Uninstall true");

                            }
                            else
                            {
                                var fs = File.Create(originalUninstallStringFile);
                                var tw = Encoding.UTF8.GetBytes(uninstallString);
                                fs.Write(tw, 0, tw.Length);
                                fs.Close();
                                key1.SetValue("UninstallString", AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe --Uninstall true");
                            }
                        }
                    }
                }
            }

            if (!args.ContainsKey("StartNumber"))
            {
                AdminRelauncher(args);
            }
            else
            {
                if (!IsRunAsAdmin())
                {
                    Environment.Exit(Environment.ExitCode);
                }
            }



            


            





            InitializeComponent();
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("AppsUseLightTheme");
                        if (o != null)
                        {
                            if (o.Equals(0))
                            {
                                setDarkColors();
                            }
                            else
                            {
                                setLightColors();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }





            



            ProcessStartInfo processtartinfo = new ProcessStartInfo();
            processtartinfo.Arguments = "/C netsh firewall add allowedprogram \"" + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe\" FileWire ENABLE";
            processtartinfo.WindowStyle = ProcessWindowStyle.Hidden;
            processtartinfo.CreateNoWindow = true;
            processtartinfo.FileName = "CMD.exe";
            System.Diagnostics.Process.Start(processtartinfo);

            processtartinfo = new ProcessStartInfo();
            processtartinfo.Arguments = "/C netsh advfirewall firewall add rule name=\"FileWire\" dir=in action=allow program=\"" + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe\" enable=yes";
            processtartinfo.WindowStyle = ProcessWindowStyle.Hidden;
            processtartinfo.CreateNoWindow = true;
            processtartinfo.FileName = "CMD.exe";
            System.Diagnostics.Process.Start(processtartinfo);

            try
            {
                //FirewallHelper.Instance.GrantAuthorization(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName, "FileWire");
            }
            catch (Exception)
            {

            }

            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName);
            Console.WriteLine(System.Environment.MachineName);
            Console.WriteLine(Encoding.UTF8.GetBytes(System.Environment.MachineName).ToString());





            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(1500))
            };
            anim.Completed += OpenMainWindow;
            this.BeginAnimation(Window.OpacityProperty, anim);
            
        }

        private void setLightColors()
        {
            backgroundImage.Source = new BitmapImage(new Uri("Resources/background.jpeg", UriKind.Relative));
            logo.Source = new BitmapImage(new Uri("Resources/ic_logo_playstore_without_bg.png", UriKind.Relative));
        }

        private void setDarkColors()
        {
            backgroundImage.Source = new BitmapImage(new Uri("Resources/background_dark.jpg", UriKind.Relative));
            logo.Source = new BitmapImage(new Uri("Resources/ic_logo_dark_playstore_without_bg.png", UriKind.Relative));
        }

        Window window = null;
        private void OpenMainWindow(object sender, EventArgs e)
        {
            window = new MainWindow(args);
            window.Show();

            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            anim.Completed += closeSplash;
            window.BeginAnimation(Window.OpacityProperty, anim);

        }

        private void closeSplash(object sender, EventArgs e)
        {
            this.Closing += onAppClosing;
            this.Close();
        }

        Boolean hasCloseAnimationPlayed = false;
        private void onAppClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            if (!hasCloseAnimationPlayed)
            {
                DoubleAnimation anim = new DoubleAnimation()
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(500))
                };
                anim.Completed += fadeOutComplete;
                this.BeginAnimation(Window.OpacityProperty, anim);
            }
            else
            {
                Visibility = Visibility.Hidden;
                try
                {
                    window.Closed += exit;
                }
                catch
                {

                }
            }
        }

        private void exit(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void fadeOutComplete(object sender, EventArgs e)
        {
            hasCloseAnimationPlayed = true;
            this.Close();
            hasCloseAnimationPlayed = false;
        }

    }
}
