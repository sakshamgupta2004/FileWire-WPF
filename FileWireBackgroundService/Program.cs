using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace FileWireBackground { 
    public class BackgroundVisibility
    {
        public static void Main()
        {
            String productId = "";
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

                try
                {
                    var Client = new UdpClient();
                    Client.Client.ReceiveTimeout = 300;
                    Client.Client.SendTimeout = 300;
                    var RequestData = Encoding.ASCII.GetBytes("FileWire: PCID:-" + productId);
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

                            IPEndPoint target = new IPEndPoint(IPAddress.Broadcast, 42404 + i);
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
                                        IPEndPoint target = new IPEndPoint(IPAddress.Broadcast, 42404 + i);
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
        }
    }
}