﻿using Microsoft.Win32;
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
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
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
                 //   Directory.Delete(new Preferences().settingsDirectory, true);

                }
                catch
                {

                }
                Environment.Exit(Environment.ExitCode);
            }





            Window wnd = new SplashWindow(arguments);
            wnd.Show();


        }
    }
}
