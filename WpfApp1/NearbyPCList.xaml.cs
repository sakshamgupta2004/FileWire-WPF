using FileWire;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfApp1.MainWindow;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using Page = System.Windows.Controls.Page;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for NearbyPCList.xaml
    /// </summary>
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index = listView.ItemContainerGenerator.IndexFromContainer(item) + 1;
            return index.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Subtract30Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - 30;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class NearbyPCList : Page
    {
        private ContentDialog dialog;
        private int myPort = 1234;
        public NearbyPCList(ContentDialog p, int port)
        {
            InitializeComponent();
            dialog = p;
            NearbyPCListView.ItemsSource = MainWindow.NearbyPCList;
            myPort = port;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dialog.Hide();
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try {
                var item = sender as ListView;
                if (item != null)
                {
                    int index = item.SelectedIndex;
                    string fullAddress = MainWindow.NearbyPCList.ElementAt(index).address;
                    string ipaddress = fullAddress.Substring(7, fullAddress.IndexOf(":", 7) - 7);
                    string port = fullAddress.Substring(fullAddress.IndexOf(":", 7) + 1, 4);
                    Trace.WriteLine("Address: " + ipaddress + "    Port: " + port);
                    using (var client = new WebClient())
                    {
                        String add = "";
                        System.Net.IPAddress[] ad = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                        foreach (System.Net.IPAddress ip in ad)
                        {
                            Trace.WriteLine(ip.ToString());
                            Trace.WriteLine(ipaddress);

                            Trace.WriteLine("");
                            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                add += "http://" + ip.ToString() + ":" + myPort.ToString() + ";;";
                            }
                            /*if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                if (ip.ToString().Split(".").Length == 4)
                                {
                                    if (ipaddress.Split(".").Length == 4)
                                    {
                                        var a = ip.ToString().Split(".");
                                        var b = ipaddress.Split(".");
                                        if (a[0].Equals(b[0]))
                                        {
                                            if (a[1].Equals(b[1]))
                                            {
                                             //   if (a[2].Equals(b[2]))
                                             //   {
                                                    add += ip.ToString();
                                             //   }
                                            }
                                        }
                                    }
                                }
                            }*/
                        }

                        Trace.WriteLine(add);

                        if (add.Trim().Length != 0)
                        {
                            try
                            {
                                Trace.WriteLine(fullAddress + "STARTSERVERPC:" + add);
                                client.DownloadString(fullAddress + "STARTSERVERPC:" + add);
                                WebClient client1 = new WebClient();
                                new ServerListenerClass().connectedSuccessfully(ipaddress);
                                new ServerListenerClass().gotMobilePort(int.Parse(port));
                                client1.DownloadString(fullAddress + "verify?" + myPort.ToString() + "&&false");
                                new ServerListenerClass().autoSendFiles();
                                HTTPSERVER.isConnected = true;
                                dialog.Hide();
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
            catch
            {

            }
            }
    }
}
