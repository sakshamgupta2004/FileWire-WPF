// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using FileWire;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using log4net;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SimpleHttpServer
{
    public class HttpProcessor
    {

        #region Fields

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        private List<Route> Routes = new List<Route>();

        private static readonly ILog log = LogManager.GetLogger(typeof(HttpProcessor));

        #endregion

        #region Constructors

        public HttpProcessor()
        {
        }

        #endregion

        #region Public Methods
        public void HandleClient(TcpClient tcpClient)
        {
                Stream inputStream = GetInputStream(tcpClient);
                Stream outputStream = GetOutputStream(tcpClient);
                HttpRequest request = GetRequest(inputStream, outputStream);

                // route and handle the request...
                HttpResponse response = RouteRequest(inputStream, outputStream, request);      
          
                Console.WriteLine("{0} {1}",response.StatusCode,request.Url);
                // build a default response for errors
                if (response.Content == null) {
                    if (response.StatusCode != "200") {
                        response.ContentAsUTF8 = string.Format("{0} {1} <p> {2}", response.StatusCode, request.Url, response.ReasonPhrase);
                    }
                }

                WriteResponse(outputStream, response);

                outputStream.Flush();
                outputStream.Close();
                outputStream = null;

                inputStream.Close();
                inputStream = null;

        }

        // this formats the HTTP response...
        private static void WriteResponse(Stream stream, HttpResponse response) {            
            if (response.Content == null) {           
                response.Content = new byte[]{};
            }

            // default to text/html content type
            if (response.dir == null)
            {
                if (!response.Headers.ContainsKey("Content-Type"))
                {
                    response.Headers["Content-Type"] = "text/html";
                }
            }
            else
            {
                response.Headers["Content-Type"] = "application/x-zip-compressed";
            }

            if (response.dir == null)
            {
                if (response.stream == null)
                    response.Headers["Content-Length"] = response.Content.Length.ToString();
                else
                    response.Headers["Content-Length"] = response.stream.Length.ToString();
            }
            else
            {
                response.Headers["Content-Length"] = "-1";
            }
            Write(stream, string.Format("HTTP/1.0 {0} {1}\r\n",response.StatusCode,response.ReasonPhrase));
            Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            Write(stream, "\r\n\r\n");

            long total = 0;
            try
            {
                if (response.stream != null)
                {
                    //response.stream.CopyTo(stream);

                    try
                    {

                        response.listener.sendStarted();
                        byte[] buffer = new byte[81920];
                        int count = 0;
                        while ((count = response.stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, count);
                            total += count;
                            response.listener.progressUpdate(total);
                        }
                        response.stream.Close();
                        response.listener.sendComplete(total);
                    }
                    catch (Exception e)
                    {
                        response.listener.sendFailed(e);
                    }
                }
                else if (response.dir != null)
                {
                    writeZipToStream(response.dir, stream, response.listener);
                }
                else
                    stream.Write(response.Content, 0, response.Content.Length);
            }
            catch (Exception e)
            {
                Console.Out.Write(e.ToString());
            }
        }

        

        public void AddRoute(Route route)
        {
            this.Routes.Add(route);
        }

        #endregion

        #region Private Methods

        private static string Readline(Stream stream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual HttpResponse RouteRequest(Stream inputStream, Stream outputStream, HttpRequest request)
        {

            //List<Route> routes = this.Routes.Where(x => Regex.Match(request.Url, x.UrlRegex).Success).ToList();
            List<Route> routes = this.Routes;
            if (!routes.Any())
                return HttpBuilder.NotFound();

            Route route = routes.SingleOrDefault(x => x.Method == request.Method);

            if (route == null)
                return new HttpResponse()
                {
                    ReasonPhrase = "Method Not Allowed",
                    StatusCode = "405",

                };

            // extract the path if there is one
            /*var match = Regex.Match(request.Url,route.UrlRegex);
            if (match.Groups.Count > 1) {
                request.Path = match.Groups[1].Value;
            } else {*/
                request.Path = request.Url;
            //}

            // trigger the route handler...
            request.Route = route;
            try {
                return route.Callable(request);
            } catch(Exception ex) {
                log.Error(ex);
                return HttpBuilder.InternalServerError();
            }

        }

        private HttpRequest GetRequest(Stream inputStream, Stream outputStream)
        {
            //Read Request Line
            string request = Readline(inputStream);

            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = Readline(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            string content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];
               
                while(bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }


            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = content
            };
        }

        #endregion
        
        private static void writeZipToStream(string dir, Stream stream, FileSendListener listener)
        {
            long total = 0;
            List<string> fileList;
            string SOURCE_FOLDER = dir;
            Console.WriteLine(SOURCE_FOLDER);
            fileList = new List<string>();
            generateFileList(SOURCE_FOLDER, SOURCE_FOLDER, fileList);
            byte[] buffer = new byte[81920];
            string source = new DirectoryInfo(SOURCE_FOLDER).Name;
            ZipOutputStream zos = null;

            listener.sendStarted();
            try
            {
                zos = new ZipOutputStream(stream);
                zos.SetLevel(Deflater.NO_COMPRESSION);
                FileStream input = null;
                foreach (string file in fileList)
                {
                    Console.WriteLine("Entry: " + SOURCE_FOLDER + @"\" + file);
                    FileAttributes attr = File.GetAttributes(SOURCE_FOLDER + @"\" + file);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        ZipEntry ze = new ZipEntry(source + @"\" + file + "/");
                        ze.Size = 0;
                        zos.PutNextEntry(ze);
                        //ze.Size = new FileInfo(SOURCE_FOLDER + @"\" + file).Length;
                    }
                    else
                    {
                        ZipEntry ze = new ZipEntry(source + @"\" + file);

                        ze.Size = new FileInfo(SOURCE_FOLDER + @"\" + file).Length;
                        zos.PutNextEntry(ze);
                        try
                        {
                            input = File.OpenRead(SOURCE_FOLDER + @"\" + file);
                            
                            int len;
                            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                zos.Write(buffer, 0, len);
                                total += len;
                                listener.progressUpdate(total);
                            }
                        }
                        finally
                        {
                            input.Close();
                        }
                    }
                }
                zos.CloseEntry();
                listener.sendComplete(total);
            }
            catch (Exception e)
            {
                listener.sendFailed(e);
            }
            finally
            {
                try
                {
                    zos.Close();
                }
                catch (Exception)
                {

                }
            }
            //stream.Write(Encoding.UTF8.GetBytes("OK"), 0, Encoding.UTF8.GetBytes("OK").Length);

        }

        private static void generateFileList(string node, string SOURCE_FOLDER, List<string> fileList)
        {
            FileAttributes attr = File.GetAttributes(node);
            //Console.WriteLine(node);

            //detect whether its a directory or file
            
            if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
            {
                fileList.Add(node.Substring(SOURCE_FOLDER.Length + 1));
            }
            else
            {
                int count = 0;
                try
                {
                    string[] subNode = Directory.GetFiles(node);

                    foreach (string fileName in subNode)
                    {
                        //Console.WriteLine(fileName);
                        count++;
                        generateFileList(fileName, SOURCE_FOLDER, fileList);
                    }
                }
                catch (Exception)
                {

                }

                try
                {
                    string[] subNode1 = Directory.GetDirectories(node);
                    foreach (string fileName in subNode1)
                    {
                        //Console.WriteLine(fileName);
                        count++;
                        generateFileList(fileName, SOURCE_FOLDER, fileList);
                    }
                }
                catch (Exception)
                {

                }
                if (count == 0)
                {
                    try
                    {
                        fileList.Add(node.Substring(SOURCE_FOLDER.Length + 1));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }

}
