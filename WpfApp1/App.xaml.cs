using FileWire;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics.Tracing;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        int GetInstanceCount(string ExeName)
        {
            Process[] processlist = Process.GetProcessesByName(ExeName);
            int NoOfInstances = processlist.Count();
            return NoOfInstances;
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey.SetValue(AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe" + " --windowsStartup true");
            
            
            string[] args = Environment.GetCommandLineArgs();
            var arguments = new Dictionary<string, string>();

            for (int i = 1; i < args.Length; i += 2)
            {
                
                try
                {
                    string arg = args[i].Substring(args[i].IndexOf("--")+2);
                    arguments.Add(arg, args[i + 1]);
                }
                catch
                {

                }
            }

            if (args.Length >= 1)
            {
                var tw = "";
                var fs = File.Create(new Preferences().settingsDirectory + "Args.txt");
                foreach (var s in args)
                {
                    tw += s;
                    tw += "\n";
                }
                var toWrite = Encoding.ASCII.GetBytes(tw);
                fs.Write(toWrite, 0, toWrite.Length);
                fs.Close();
            }
            else
            {

                var fs = File.Create(new Preferences().settingsDirectory + "Args.txt");
                var toWrite = Encoding.ASCII.GetBytes(args.Length.ToString());
                fs.Write(toWrite, 0, toWrite.Length);
                fs.Close();
            }


            string filesString = "";

            bool hasSelectedFiles = false;
            int index = 0;
            foreach (var s in args)
            {
                if (s.Equals("--SelectAllFiles"))
                {
                    hasSelectedFiles = true;
                }
                if (hasSelectedFiles && (!s.Equals("--SelectAllFiles")))
                {
                    if (s.Equals("--EndOfFiles"))
                    {
                        break;
                    }
                    else
                    {
                        filesString += s;
                        filesString += "\n";
                    }
                }
                index++;
            }

            if (hasSelectedFiles)
            {
                arguments = new Dictionary<string, string>();
                arguments.Add("Files", filesString);
            }
            if (arguments.ContainsKey("Uninstall"))
            {
                var originalUninstallStringFile = new Preferences().settingsDirectory + "originalUninstallString";
                var fs = File.OpenRead(originalUninstallStringFile);
                byte[] fileContent = new byte[fs.Length];
                fs.Read(fileContent);
                fs.Close();
                var processInfo = new ProcessStartInfo();
                processInfo.Arguments = "/C " + Encoding.UTF8.GetString(fileContent);
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.CreateNoWindow = true;
                processInfo.FileName = "CMD.exe";
                Process.Start(processInfo);
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
                    registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    registryKey.DeleteValue(AppDomain.CurrentDomain.FriendlyName);

                    using (var key = Registry.CurrentUser.OpenSubKey("software", true))
                    {
                        key.DeleteSubKeyTree("Filewire");
                    }
                    //   Directory.Delete(new Preferences().settingsDirectory, true);

                }
                catch
                {

                }
                Environment.Exit(Environment.ExitCode);
            }
            if (arguments.ContainsKey("windowsStartup"))
            {
                BackgroundVisibility.run();
            }
            else
            {


                /*string currVersion = "null";
                using (var key1 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true))
                {
                    if (key1 != null)
                    {
                        foreach (var a in key1.GetSubKeyNames())
                        {
                            var key2 = key1.OpenSubKey(a);
                            if (key2.GetValue("DisplayName").ToString().StartsWith(AppDomain.CurrentDomain.FriendlyName) && a.ToLower().Equals("filewire-pc"))
                            {
                                currVersion = key2.GetValue("DisplayVersion", currVersion).ToString();
                            }
                        }
                    }
                }

                RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\FileWire");
                if (!key.GetValue("CurrentVersion", "1.0.0.0").Equals(currVersion))
                {
                    key.SetValue("CurrentVersion", currVersion);
                    foreach (var process in Process.GetProcessesByName(AppDomain.CurrentDomain.FriendlyName))
                    {
                        if (!process.MainModule.FileName.Equals(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".exe"))
                        {
                            process.Kill();
                        }
                    }
                }*/
                if (!Debugger.IsAttached)
                {
                    if (GetInstanceCount(AppDomain.CurrentDomain.FriendlyName) == 1)
                        Process.Start(AppDomain.CurrentDomain.FriendlyName, "--windowsStartup true");
                }
                Window wnd = new SplashWindow(arguments);
                wnd.Show();
            }







        }
    }
}
