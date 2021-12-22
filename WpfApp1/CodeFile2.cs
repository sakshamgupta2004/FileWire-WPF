using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using WpfApp1;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;
using static WpfApp1.MainWindow;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.Http;
using ModernWpf.Controls;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace FileWire
{
    public class HTTPSERVER
    {
        private bool isBackground;
        public static Boolean isConnected = false;
        private int port;
        private MainWindow.ServerListenerClass serverListener;
        private int mobilePort = 1234;
        private string filesJSON = "error";
        private string productId = "";
        public static string visibleName;
        private ObservableCollection<fileProgressClass> SendingListItems;

        public HTTPSERVER(string address, int port, ObservableCollection<fileProgressClass> sendingListItems, MainWindow.ServerListenerClass serverListenerClass, bool isBackground = false)
        {
            this.isBackground = isBackground;
            this.port = port;
            this.serverListener = serverListenerClass;
            this.SendingListItems = sendingListItems;
            String productId = "55555-00000-99999-ZZZZZ";
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
                {
                    productId = key.GetValue("ProductId", "55555-00000-99999-ZZZZZ").ToString();
                }
            }
            catch
            {

            }
            this.productId = productId;
            if (port > 1234 && !isBackground)
            {
                visibleName = System.Environment.MachineName + " - " + (port - 1234).ToString();
            }
            else
            {
                visibleName = System.Environment.MachineName;
            }
            //Console.WriteLine(address);






            if (!isBackground)
            {
                var t = new Thread(new ThreadStart(() =>
                {
                    int i = 0;
                    while (!isConnected)
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
                                add += "http://" + ip.ToString() + ":" + port.ToString() + "/\n";
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
                                    string contentUTF8 = "";
                                    if (!isConnected)
                                    {
                                        AvtarAndName av = new AvtarAndName();
                                        av.name = visibleName;
                                        av.avatar = 1.ToString();
                                        contentUTF8 = "<html><title>File Share</title><body>" + JsonConvert.SerializeObject(av) + "</body></html>";
                                    }
                                    else
                                    {
                                        contentUTF8 = "<html><title>File Share</title><body>Not Found</body></html>";
                                    }
                                    var RequestData = Encoding.ASCII.GetBytes(add);
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





                var t1 = new Thread(new ThreadStart(runPCDiscoveryServer));
                t1.IsBackground = true;
                t1.Start();
            }
        }
        
        public void startSimpleServer()
        {
            var route_config = new List<SimpleHttpServer.Models.Route>() {
        new Route {
          Name = "HTTPHandler",
            Method = "GET",
            Callable = (HttpRequest request) => {
              var headers = new Dictionary < string,
                string > ();
              string ip = "";
              request.Path = request.Path.Substring(1);
              string page = System.Web.HttpUtility.UrlDecode(request.Path);
              Stream st = null;
              String direc = null;
              string contentUTF8 = null;
              FileSendListener sendListener = null;
              Console.WriteLine(page);

              if (page.StartsWith("STARTSERVER:")) {
                ip = page.Substring(page.IndexOf("STARTSERVER:") + 12);
                headers.Add("Content-Type", "text/plain");
                contentUTF8 = "Ok";
                serverListener.connectedSuccessfully(ip);
                Console.WriteLine(ip);

              }
              if (page.StartsWith("STARTSERVERPC:")) {
                string hoststring = page.Substring(page.IndexOf("STARTSERVERPC:") + 14);
                headers.Add("Content-Type", "text/plain");
                contentUTF8 = "Ok";
                    var hosts = hoststring.Split(";;");
                    foreach (string host in hosts)
                    {
                        var a = canGetNameAndAvatar(host + "/");
                        Task.WaitAll(a);
                        if (a.Result != null)
                        {
                            ip = host.Substring(7, host.IndexOf(":", 7) - 7);
                            serverListener.connectedSuccessfully(ip);
                            Console.WriteLine(ip);
                            break;
                        }
                    }

              }
              else if (page.StartsWith("request"))
                {
                    headers.Add("Content-Type", "text/plain");
                contentUTF8 = filesJSON;
                }
                else if (page.StartsWith("verify"))
                {

                    int index = page.IndexOf("?") + 1;
                    int length = page.IndexOf("&&") - index;
                    mobilePort = int.Parse(page.Substring(index, length));
                    serverListener.gotMobilePort(mobilePort);
                    
                    headers.Add("Content-Type", "text/plain");
                contentUTF8 = "OK";
                    isConnected = true;
                    serverListener.autoSendFiles();
                }
                else if (page.ToLower().StartsWith("INCOMING:".ToLower())) {

                mobilePort = int.Parse(page.Substring(page.ToLower().IndexOf("INCOMING:".ToLower()) + 9));
                headers.Add("Content-Type", "text/plain");
                contentUTF8 = "Ok";
                serverListener.incomingFiles(mobilePort);

              } else if (page.StartsWith("file:")) {
                contentUTF8 = "OK";
                String filePath = page.Substring(5);
                Console.WriteLine("FileName: " + filePath);
                FileAttributes attr = File.GetAttributes(filePath);


                    fileProgressClass progress = null;
                foreach (fileProgressClass fpc in SendingListItems)
                    {
                        if (fpc.fileName.Equals(Path.GetFileName(filePath)))
                        {
                            Console.WriteLine("Equals");
                            if (fpc.progressPercent == 0)
                            {
                                if (!fpc.downloadFailed)
                                {
                                    progress = fpc;
                                    break;
                                }
                            }
                        }
                    }
                if (progress != null) {
                    sendListener = new FileSendListener(){
                    file = progress
                    };
                }else
                    {
                        sendListener = new FileSendListener(){
                    file = new fileProgressClass()
                    };
                    }






                    if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
                {
                headers.Add("Content-Type", MimeType.GetMimeType(filePath));
                headers.Add("Content-Disposition", "inline; filename=\"" + Path.GetFileName(filePath) + "\"");
                        try {
                st = File.OpenRead(filePath);
                        }
                        catch (Exception e)
                        {
                            sendListener.sendFailed(e);
                        }

                }
                else
                {
                headers.Add("Content-Disposition", "inline; filename=\"" + Path.GetFileName(filePath) + ".zip\"");
                direc = filePath;
                }
                
              }
              else if (page.StartsWith("getAvatarAndName"))
                {
                    
                    if (!isConnected || isBackground) {
                    headers.Add("Content-Type", "text/plain");
                    AvtarAndName av = new AvtarAndName();
                    av.name = visibleName;
                    av.avatar = 1.ToString();
                    contentUTF8 = "<html><title>File Share</title><body>" + JsonConvert.SerializeObject(av) + "</body></html>";
                        
                    }
                    else
                    {
                        headers.Add("Content-Type", "text/plain");
                        contentUTF8 = "<html><title>File Share</title><body>Not Found</body></html>";
                    }
                }
              else if (page.StartsWith("getid"))
                {
                    headers.Add("Content-Type", "text/plain");
                        contentUTF8 = "<html><title>File Share</title><body>" + productId + "</body></html>";
                }

              return new HttpResponse() {
                ReasonPhrase = "OK",
                  StatusCode = "200",
                  stream = st,
                  dir = direc,
                  ContentAsUTF8 = contentUTF8,
                  Headers = headers,
                  listener = sendListener
              };
            }
        },
      };

            HttpServer httpServer = new HttpServer(port, route_config);

            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.IsBackground = true;
            thread.Start();

        }

        public void setFiles(string json)
        {
            filesJSON = json;
        }











        public void runPCDiscoveryServer()
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
                    
                    IPEndPoint ipep = new IPEndPoint(IPAddress.Any, GetAvailablePort(42404));
                    using (UdpClient newsock = new UdpClient(ipep))
                    {


                        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = newsock.Receive(ref sender);
                        //Trace.WriteLine(Encoding.ASCII.GetString(data));


                        String add1 = "";
                        System.Net.IPAddress[] ad1 = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                        foreach (System.Net.IPAddress ip in ad)
                        {
                            add1 += "http://" + ip.ToString() + ":" + port.ToString() + "/\n";
                        }

                        if (Encoding.ASCII.GetString(data).Equals(add1))
                        {
                            Trace.WriteLine("Ping from Myself");
                        }
                        else
                        {
                            try
                            {
                                var addresses = Encoding.ASCII.GetString(data).Split("\n");
                                if (addresses.Length > 0)
                                {
                                    string PCName = "";
                                    string addr_main = "";
                                    foreach (var addr in addresses)
                                    {
                                        var task = canGetNameAndAvatar(addr);
                                        Task.WaitAll(task);
                                        if (task.Result != null)
                                        {
                                            PCName = task.Result;
                                            addr_main = addr;



                                            window.Dispatcher.Invoke(() =>
                                            {
                                                int c = 0;
                                                bool hasToRemove = false;
                                                foreach (NearbyPC np in MainWindow.NearbyPCList)
                                                {
                                                    if (np.address.Equals(addr_main) && np.PCName.Equals(PCName))
                                                    {
                                                        hasToRemove = true;
                                                        break;
                                                    }
                                                    c++;
                                                }
                                                if (hasToRemove)
                                                {
                                                    MainWindow.NearbyPCList.RemoveAt(c);
                                                    MainWindow.NearbyPCList.Insert(c, new NearbyPC()
                                                    {
                                                        address = addr_main,
                                                        PCName = PCName
                                                    });
                                                }
                                                else
                                                {
                                                    MainWindow.NearbyPCList.Add(new NearbyPC()
                                                    {
                                                        address = addr_main,
                                                        PCName = PCName
                                                    });
                                                }


                                            });
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
            }
        }

        private static async Task<String> canGetNameAndAvatar(String connection)
        {
            String link = connection + "getAvatarAndName";
            link = link.Replace(" ", "%20");
            try
            {

                var client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(500);
                string result;
                using (HttpResponseMessage response = await client.GetAsync(link))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
                result = result.Substring(result.IndexOf("<body>") + 6, result.IndexOf("</body>") - (result.IndexOf("<body>") + 6));
                AvtarAndName json = JsonConvert.DeserializeObject<AvtarAndName>(result);
                if (json != null)
                {
                    return json.name;
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

    }
    public class FileSendListener
    {
        public fileProgressClass file;
        public void progressUpdate(long total)
        {
            Trace.WriteLine(total.ToString());
            file.progressPercent = int.Parse(((total * 100) / long.Parse(file.fileSize)).ToString());
        }
        public void sendComplete(long downloaded)
        {
            Trace.WriteLine("Complete: " + downloaded.ToString());
            file.progressPercent = 100;
        }
        public void sendFailed(Exception e)
        {
            Trace.WriteLine("Send Failed");
            file.downloadFailed = true;
        }
        public void sendStarted()
        {
            Trace.WriteLine("Send Start");
        }
    }
    public class MyUdpClient : UdpClient
    {
        public MyUdpClient() : base()
        {
            //Calls the protected Client property belonging to the UdpClient base class.
            Socket s = this.Client;
            //Uses the Socket returned by Client to set an option that is not available using UdpClient.
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
        }

        public MyUdpClient(IPEndPoint ipLocalEndPoint) : base(ipLocalEndPoint)
        {
            //Calls the protected Client property belonging to the UdpClient base class.
            Socket s = this.Client;
            //Uses the Socket returned by Client to set an option that is not available using UdpClient.
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
        }

    }
    class AvtarAndName
    {
        public string name { get; set; }
        public string avatar { get; set; }
    }

}