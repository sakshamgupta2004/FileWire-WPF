using System;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfApp1
{

    public class NearbyPC
    {
        public NearbyPC()
        {
            try
            {
                new Thread(new ThreadStart(() => {
                    Thread.Sleep(20000);
                    MainWindow.window.Dispatcher.Invoke(() =>
                    {
                        MainWindow.NearbyPCList.Remove(this);
                    });
                })).Start();
            }
            catch
            {

            }
        }
        public string address { get; set; }
        public string PCName { get; set; }
    }
}