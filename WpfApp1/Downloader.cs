using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace FileWire
{
    public class Downloader : IDisposable
    {
        private Uri link;
        private string savePath;
        private Boolean isFolder;
        private static int bufferSize = 819200;
        private DownloadCompleteEventHandler downloadCompleteEventHandler = null;
        private DownloadProgressChangedEventHandler downloadProgressChangedEventHandler = null;


        public Downloader(Uri uri, string v, Boolean isFolder)
        {
            this.link = uri;
            this.savePath = v;
            this.isFolder = isFolder;

            new Thread(new ThreadStart(startDownload)).Start();
        }

        private void startDownload()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                WebRequest webRequest = WebRequest.CreateHttp(link);

                WebResponse response = webRequest.GetResponse();
                Stream input = response.GetResponseStream();

                if (isFolder)
                {
                    unzipWhileDownloading(input, savePath);
                }
                else
                {
                    FileStream fs = File.Create(savePath);
                    byte[] buffer = new byte[bufferSize];

                    long total = 0;
                    //long count = 0;
                    try
                    {
                        /*var awaiter = input.CopyToAsync(fs);
                        while (!awaiter.IsCompleted)
                        {
                            count = fs.Position;
                            if (downloadProgressChangedEventHandler != null)
                            {
                                downloadProgressChangedEventHandler.progressChange.Invoke(this, new DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs(total + count));
                            }
                            //Thread.Sleep(100);
                        }
                        total += fs.Length;*/


                        int count = 0;
                        while ((count = input.Read(buffer, 0, bufferSize)) != 0)
                        {
                            total += count;
                            if (downloadProgressChangedEventHandler != null)
                            {
                                downloadProgressChangedEventHandler.progressChange.Invoke(this, new DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs(count));
                            }
                            fs.Write(buffer, 0, count);
                            //fs.Flush();
                        }
                        /*if (downloadProgressChangedEventHandler != null)
                        {
                            downloadProgressChangedEventHandler.progressChange.Invoke(this, new DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs(count));
                        }*/
                        fs.Close();
                    }
                    catch (Exception)
                    {
                        fs.Close();
                        File.Delete(savePath);
                    }
                }
            }
            catch (Exception)
            {

            }
            if (downloadCompleteEventHandler != null)
            {
                downloadCompleteEventHandler.DownloadComplete.Invoke(this);
            }
        }

        private void unzipWhileDownloading(Stream zipIn, string destDirectory)
        {
            long total = 0;
            byte[] buffer = new byte[bufferSize];
            ZipInputStream zis = new ZipInputStream(zipIn);
            ZipEntry zipEntry = zis.GetNextEntry();
            Directory.CreateDirectory(destDirectory);
            destDirectory = destDirectory.Substring(0, destDirectory.LastIndexOf(@"\"));
            while (zipEntry != null)
            {
               
                if (zipEntry.IsDirectory)
                {
                    Console.WriteLine((destDirectory + @"\" + zipEntry.Name).Replace("/", @"\"));
                    Directory.CreateDirectory((destDirectory + @"\" + zipEntry.Name).Replace("/", @"\"));
                }
                else
                {
                    String file = (destDirectory + @"\" + zipEntry.Name.Replace("/", @"\").Replace(":", " "));
                    Directory.CreateDirectory(file.Substring(0, file.LastIndexOf(@"\")));
                    FileStream fs = File.Create(file);

                    try
                    {

                        
                        int count = 0;
                        while ((count = zis.Read(buffer, 0, bufferSize)) != 0)
                        {
                            total += count;
                            if (downloadProgressChangedEventHandler != null)
                            {
                                downloadProgressChangedEventHandler.progressChange.Invoke(this, new DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs(count));
                            }
                            fs.Write(buffer, 0, count);
                        }
                        /*long count = 0;
                        var awaiter = zis.CopyToAsync(fs);
                        while (!awaiter.IsCompleted)
                        {
                            count = fs.Position;
                            if (downloadProgressChangedEventHandler != null)
                            {
                                downloadProgressChangedEventHandler.progressChange.Invoke(this, new DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs(total + count));
                            }
                            //Thread.Sleep(100);
                        }
                        total += fs.Length;*/
                        fs.Close();
                        

                    }
                    catch (Exception)
                    {
                        fs.Close();
                        File.Delete(file);
                    }

                }
                try
                {
                    zipEntry = zis.GetNextEntry();
                }
                catch (Exception)
                {
                    zipEntry = null;
                }
            }
            try
            {
                zis.Close();
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
        }



        public class DownloadProgressChangedEventHandler
        {
            public Action<object, DownloadProgressChangedEventArgs> progressChange;

            public DownloadProgressChangedEventHandler(Action<object, DownloadProgressChangedEventArgs> progressChange)
            {
                this.progressChange = progressChange;
            }

            public class DownloadProgressChangedEventArgs
            {
                public DownloadProgressChangedEventArgs(long bytes)
                {
                    this.BytesReceived = bytes;
                }

                public long BytesReceived { get; set; }
            }
        }
        public class DownloadCompleteEventHandler
        {
            public Action<object> DownloadComplete;

            public DownloadCompleteEventHandler(Action<object> downloadComplete)
            {
                this.DownloadComplete = downloadComplete;
            }
        }
        public void setOnDownloadProgressEventHandler(DownloadProgressChangedEventHandler downloadProgressChangedEventHandler)
        {
            this.downloadProgressChangedEventHandler = downloadProgressChangedEventHandler;
        }
        public void setOnDownloadCompleteEventHandler(DownloadCompleteEventHandler downloadCompleteEventHandler)
        {
            this.downloadCompleteEventHandler = downloadCompleteEventHandler;
        }
    }
}