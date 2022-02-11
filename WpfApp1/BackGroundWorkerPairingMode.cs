using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WpfApp1;
using static System.Environment;

namespace FileWire
{
    public class BackgroundVisibility
    {
        public static void run()
        {
            String productId = "";
            var hiddenwindow = new MainWindow(new Dictionary<string, string>(), true);
            hiddenwindow.Opacity = 1;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
            {
                productId = key.GetValue("ProductId", "55555-00000-99999-ZZZZZ").ToString();
            }
            var t = new Thread(new ThreadStart(() =>
            {
                int i = 0;
                while (true)
                {
                    if (i == 5)
                    {
                        i = 0;
                    }
                    try
                    {
                        Thread.Sleep(1000);
                        String add = "";
                        System.Net.IPAddress[] ad = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                        foreach (System.Net.IPAddress ip in ad)
                        {
                            add += ip.ToString() + ":" + hiddenwindow.getThisPCPort().ToString() + "\n";
                        }
                        bool connected;
                        if (add.Trim(' ').Length == 0)
                        {
                            connected = false;
                        }
                        else
                        {
                            connected = true;
                        }
                        if (connected)
                        {

                            try
                            {
                                var Client = new UdpClient();
                                Client.Client.ReceiveTimeout = 300;
                                Client.Client.SendTimeout = 300;
                                var RequestData = Encoding.ASCII.GetBytes("FileWire: PCID:-" + productId + "\n" + add);
                                var ServerEp = new IPEndPoint(IPAddress.Any, 0);

                                Client.EnableBroadcast = true;
                                Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, (42404 + i)));

                                Client.Close();
                                System.Net.IPAddress[] ad1 = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                                foreach (System.Net.IPAddress ip in ad1)
                                {
                                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                                    {

                                        IPEndPoint local = new IPEndPoint(IPAddress.Parse(ip.ToString()), 0);
                                        UdpClient udpc = new UdpClient(local);
                                        udpc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                                        udpc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);

                                        IPEndPoint target = new IPEndPoint(IPAddress.Broadcast, 42004 + i);
                                        udpc.Send(RequestData, RequestData.Length, target);
                                    }
                                }








                                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                                {
                                    if (ni.OperationalStatus == OperationalStatus.Up && ni.SupportsMulticast && ni.GetIPProperties().GetIPv4Properties() != null)
                                    {
                                        int id = ni.GetIPProperties().GetIPv4Properties().Index;
                                        if (NetworkInterface.LoopbackInterfaceIndex != id)
                                        {
                                            foreach (UnicastIPAddressInformation uip in ni.GetIPProperties().UnicastAddresses)
                                            {
                                                if (uip.Address.AddressFamily == AddressFamily.InterNetwork)
                                                {
                                                    IPEndPoint local = new IPEndPoint(uip.Address.Address, 0);
                                                    UdpClient udpc = new UdpClient(local);
                                                    udpc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                                                    udpc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
                                                    byte[] data = RequestData;
                                                    IPEndPoint target = new IPEndPoint(IPAddress.Broadcast, 42004 + i);
                                                    udpc.Send(data, data.Length, target);
                                                }
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
                    catch
                    {

                    }

                    i++;
                }
            }));
            t.IsBackground = true;
            t.Start();
            var t1 = new Thread(new ThreadStart(() =>
            {
                mobileDiscoveryServer(productId, hiddenwindow);
            }));
            t1.IsBackground = true;
            t1.Start();


        }



        private static void mobileDiscoveryServer(string prodId, MainWindow hiddenWin)
        {
            while (true)
            {
                String add = "";
                System.Net.IPAddress[] ad = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                foreach (System.Net.IPAddress ip in ad)
                {
                    add += ip.ToString() + "\n";
                }
                bool connected;
                if (add.Trim(' ').Length == 0)
                {
                    connected = false;
                }
                else
                {
                    connected = true;
                }
                if (connected)
                {


                    IPEndPoint ipep = new IPEndPoint(IPAddress.Any, MainWindow.GetAvailablePort(36000));
                    using (UdpClient newsock = new UdpClient(ipep))
                    {


                        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = newsock.Receive(ref sender);


                        new Thread(new ThreadStart(() =>
                        {

                            string receivedAdd = Encoding.ASCII.GetString(data);
                            if (receivedAdd.StartsWith("MobileFileWire"))
                            {
                                receivedAdd = receivedAdd.Substring(15);
                            }
                            string[] RemoveDuplicates(string[] s)
                            {
                                HashSet<string> set = new HashSet<string>(s);
                                string[] result = new string[set.Count];
                                set.CopyTo(result);
                                return result;
                            }
                            var adds = RemoveDuplicates(receivedAdd.Split("\n"));
                            int port = 0;

                            foreach (string s in adds)
                            {
                                if (!s.StartsWith("Port:"))
                                {
                                    try
                                    {
                                        String addtoSend = "";
                                        System.Net.IPAddress[] ad1 = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                                        foreach (System.Net.IPAddress ip in ad1)
                                        {
                                            addtoSend += ip.ToString() + ":" + hiddenWin.getThisPCPort().ToString() + ";";
                                        }

                                        //"http://" + s + ":" + port + "/pcavailable:" + Encoding.UTF8.GetString(Encoding.Default.GetBytes(prodId + ";" + addtoSend));
                                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                                        WebRequest webRequest = WebRequest.CreateHttp("http://" + s + ":" + port + "/compavailable:" + Encoding.UTF8.GetString(Encoding.Default.GetBytes(prodId + ";" + addtoSend)));
                                        webRequest.Timeout = 500;
                                        try
                                        {
                                            WebResponse response = webRequest.GetResponse();
                                            response.Close();
                                        }
                                        catch { }
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                else
                                {
                                    port = Int32.Parse(s.Substring(5));
                                }

                            }

                        })).Start();
                    }
                }
            }
        }

    }
    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 3 * 1000;
            return w;
        }
    }
}