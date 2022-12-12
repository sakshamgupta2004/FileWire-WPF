using DocumentFormat.OpenXml.InkML;
using FileWire;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ModernWpf.Controls;
using Newtonsoft.Json;
using SourceChord.FluentWPF;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XamlFlair;
using ZXing;
using ProgressBar = ModernWpf.Controls.ProgressBar;
using Console = System.Diagnostics.Trace;
using Path = System.IO.Path;
using System.Text.RegularExpressions;
using DataFormats = System.Windows.DataFormats;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.VisualBasic.Logging;
using RegistryUtils;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : AcrylicWindow
    {
        private const String APP_ID = "Sugarsnooper.FileWire";

        private delegate void mainDelegate();

        private static mainDelegate switchToLightTheme = new mainDelegate(setLightColors);
        private static mainDelegate switchToDarkTheme = new mainDelegate(setDarkColors);
        private static mainDelegate switchToOpaqueWindow = new mainDelegate(makeWindowOpaque);
        private static mainDelegate switchToTransparentWindow = new mainDelegate(makeWindowTransparent);
        private static mainDelegate refreshAllAccentedItems = new mainDelegate(RefreshAccentedItems);
        private static mainDelegate showQRCode = new mainDelegate(showQRConnectCode);
        private static mainDelegate enableButtons = new mainDelegate(enableAllButtons);
        private static mainDelegate disableButtons = new mainDelegate(disableAllButtons);
        private static mainDelegate update = new mainDelegate(changeProgress);
        private static mainDelegate disable = new mainDelegate(disableProgress);
        private static mainDelegate formClose = new mainDelegate(close);
        private static mainDelegate statusDownloading = new mainDelegate(setStatusTransferInProgress);
        private static mainDelegate displayFileName = new mainDelegate(changeDisplayedFileName);
        private static mainDelegate displayConnectedDeviceInfo = new mainDelegate(displayConnectedInfo);
        private static mainDelegate connected = new mainDelegate(hideQrCodeAndShowUI);
        private static mainDelegate incoming = new mainDelegate(incomingFile);

        private static System.Windows.Controls.Image backgroundImage = null;
        private static System.Windows.Controls.Image QRCodePictureBox;

        private static System.Windows.Controls.Button selectFileButton;
        private static System.Windows.Controls.Button selectFilesToSendButton;
        private static System.Windows.Controls.Button selectFolderToSendButton;
        private static System.Windows.Controls.Button openReceivedButton;
        private static System.Windows.Controls.Button ViewListButton;
        private static System.Windows.Controls.Button HideListViewButton;


        private static AnimatedScrollViewer.AnimatedScrollViewer QRCodeViewPanel;
        private static ScrollViewerEx ConnectedViewPanel;

        private static ProgressBar ProgressBar1;
        private static ProgressBar ProgressBar2;
        private static ProgressBar[] MultiThreadFileProgressBars;

        private static TextBlock label;
        private static TextBlock FileNameLabel;
        private static TextBlock StatusLabel;
        private static TextBlock overallProgressPercentageLabel;
        private static TextBlock fileProgressPercentageLabel;
        private static TextBlock fileRelativeProgress;
        private static TextBlock relativeOverallProgressLabel;
        private static TextBlock instructionLabel;
        private static TextBlock visibleAsLabel;
        private static TextBlock TotalProgressLabel;
        private static TextBlock NoFilesReceivedTextBlockOnListView;
        private static TextBlock NoFilesSentTextBlockOnListView;

        public static AcrylicWindow window;

        private static SolidColorBrush plusIconTint;
        private static SolidColorBrush gearIconTint;
        private static SolidColorBrush backIconTint;

        private static Grid settingsView;
        private static Grid mainProgressViews;

        private static System.Windows.Controls.ListView receivingListView;

        private static System.Windows.Controls.ListView sendingListView;
        //private static ModernWpf.Controls.ListView receivingListView;

        private static ContentDialog dialog;

        private static Grid multiThreadFileProgressBarsPanel;

        private static HTTPSERVER server;
        private static Dictionary<string, string> args;
        public static string DirectoryPhotos = "Photos";
        public static string DirectoryVideos = "Videos";
        public static string DirectoryApps = "Apps";
        public static string DirectoryArchives = "Archives";
        public static string DirectoryDocuments = "Documents";
        public static string DirectoryAudio = "Audio";
        public static string DirectoryDefault = "Other";
        public static string iPAddress = "";
        public static int mobilePort;
        private static string timeStamp;
        private static string currFileName = "";
        private static Boolean commandToDownload = true;
        private static bool isLightMode = false;
        private static List<MyClass> list;
        private static string host = "";
        private static long FilesReceivingTotalSize = 0;
        private static long FilesReceivingReceivingSize = 0;
        private static int overallProgress;
        private static int pos = 0;
        private static int port = 1234;
        private static int threads;
        private static int threadsMade = 0;
        private static AvtarAndName connectedInfo;
        private static Preferences preferences;
        private static ToastNotification progressToast;
        private static ToastContent progressToastContent;
        private static HiddenProgressOverlayWindow progressOverlay = null;

        public static ObservableCollection<NearbyPC> NearbyPCList
        {
            get { return _NearbyPCList; }
        }

        public static ObservableCollection<NearbyPC> _NearbyPCList = new ObservableCollection<NearbyPC>();

        private static ObservableCollection<fileProgressClass> ReceivingFilesListViewItems
        {
            get { return _ReceivingFilesListViewItems; }
        }

        private static ObservableCollection<fileProgressClass> _ReceivingFilesListViewItems =
            new ObservableCollection<fileProgressClass>();

        public static ObservableCollection<fileProgressClass> SendingFilesListViewItems
        {
            get { return _SendingFilesListViewItems; }
        }

        public static ObservableCollection<fileProgressClass> _SendingFilesListViewItems =
            new ObservableCollection<fileProgressClass>();


        private static List<ThreadTransactionInfo> receivingFilesList
        {
            get { return _receivingFilesList; }
        }

        private static List<ThreadTransactionInfo> _receivingFilesList = new List<ThreadTransactionInfo>();

        private class ThreadTransactionInfo
        {

            public ObservableCollection<MyClass> List { get; set; }
            public int PointerLocation { get; set; }
            public long FileCurrentReceivedSize { get; set; }
            public bool CommandToDownload { get; set; }
        }



        private static void hideQrCodeAndShowUI()
        {

            
            foreach (var item in ReceivingFilesListViewItems)
            {
                item.downloadFailed = false;
            }

            preferences = new Preferences();
            overallProgress = 0;
            FilesReceivingReceivingSize = 0;
            FilesReceivingTotalSize = 0;
            if (window.IsVisible)
            {
                if (dialog != null)
                {
                    if (dialog.IsVisible)
                    {
                        dialog.Hide();
                    }
                }

                settingsView.Visibility = Visibility.Hidden;
                ConnectedViewPanel.Visibility = Visibility.Visible;
                if (window.Width <= 750)
                    receivingListView.Visibility = Visibility.Collapsed;
                var e = new System.Windows.Size(window.ActualWidth, window.ActualHeight);
                windowSizeChange(e);
            }

            threads = preferences.getThreads();
            if (threads != 1)
            {
                if (window.IsVisible)
                {

                    FileNameLabel.Text = "File Progresses";
                    fileProgressPercentageLabel.Visibility = Visibility.Hidden;
                    fileRelativeProgress.Visibility = Visibility.Hidden;
                    ProgressBar1.Visibility = Visibility.Collapsed;

                    MultiThreadFileProgressBars = new ProgressBar[threads];
                    for (int i = 0; i < threads; i++)
                    {
                        multiThreadFileProgressBarsPanel.ColumnDefinitions.Add(new ColumnDefinition());
                        ProgressBar progressBar = new ProgressBar()
                        {
                            Height = 8,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Value = 0,
                            Foreground = AccentColors.ImmersiveSystemAccentBrush,
                            Margin = new Thickness
                            {
                                Bottom = 0,
                                Top = 0,
                                Right = 2,
                                Left = 2
                            }
                        };
                        Grid.SetColumn(progressBar, i);
                        MultiThreadFileProgressBars[i] = progressBar;
                        multiThreadFileProgressBarsPanel.Children.Add(progressBar);
                    }
                }
            }
            else
            {
                if (window.IsVisible)
                    multiThreadFileProgressBarsPanel.Visibility = Visibility.Collapsed;
            }

            for (int i = threadsMade; i < threads; i++)
            {
                var list = new ObservableCollection<MyClass>();
                var threadNum = i;
                list.CollectionChanged += (s, e) =>
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        if (receivingFilesList.ElementAt(threadNum).CommandToDownload)
                        {
                            receivingFilesList.ElementAt(threadNum).CommandToDownload = false;
                            downloadFile(null, threadNum);
                        }
                    }
                };

                receivingFilesList.Add(new ThreadTransactionInfo()
                {
                    List = list,
                    PointerLocation = 0,
                    FileCurrentReceivedSize = 0,
                    CommandToDownload = true
                });
                threadsMade++;
            }

            if (!window.IsVisible)
            {
                new Thread(() =>
                {
                    isConnectedCurrently = true;


                    long timeSinceDown = DateTime.Now.Ticks/10000000;
                    while (true)
                    {
                        var t = HTTPSERVER.canGetNameAndAvatar("http://" + iPAddress + ":" + mobilePort + "/");
                        Task.WaitAll(t);
                        if (t.Result == null)
                        {
                            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\*\shell", true))
                            {
                                var keys = key.GetSubKeyNames();
                                foreach (var s in keys)
                                {
                                    if (s.StartsWith("Send via FileWire to "))
                                    {
                                        key.DeleteSubKeyTree(s);
                                    }
                                }
                            }

                            isConnectedCurrently = false;
                            if (DateTime.Now.Ticks / 10000000 - timeSinceDown > 5)
                            {
                                break;
                            }
                        }
                        else
                        {
                            timeSinceDown = DateTime.Now.Ticks / 10000000;
                            try
                            {
                                var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell");
                                if (connectedInfo.name != null)
                                {
                                    var key1 = key.CreateSubKey("Send via FileWire to " + t.Result);
                                    key1.SetValue("icon", new Preferences().settingsDirectory + "appicon.ico",
                                        RegistryValueKind.String);
                                    var key2 = key1.CreateSubKey("command");
                                    key2.SetValue("",
                                        new Preferences().settingsDirectory + "selectall.exe" + " \"%1\" \"" +
                                        AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName +
                                        "\" --SendAllFiles $files --EndOfFiles");
                                    key1.Close();
                                    key2.Close();
                                }

                                key.Close();
                            }
                            catch (Exception e)
                            {
                                new Preferences().printToLog(e.ToString());
                            }

                            isConnectedCurrently = true;
                        }
                        
                    }
                }).Start();
            }

        }


        private static void incomingFile()
        {
            if (progressOverlay == null)
            {
                progressOverlay = new HiddenProgressOverlayWindow();
                if (isLightMode)
                    progressOverlay.setLightColors();
                else
                    progressOverlay.setDarkColors();
                progressOverlay.Show();
            }

            checkAndCreateDirectoryForApplication();
            System.Net.WebRequest.DefaultWebProxy = null;

            var t = new Thread(new ThreadStart(run));
            t.IsBackground = true;
            t.Start();
            //  progressToast = new ToastContentBuilder();

            /* progressToast.AddText("Receiving Files from " + connectedInfo.name);
             progressToast.AddVisualChild(new AdaptiveProgressBar()
             {
                 Title = "Transfer Progress",
                 Value = Double.Parse(overallProgress.ToString())/100.0,
                 ValueStringOverride = overallProgress + "%",
                 Status = "Receiving"
             });
             progressToast.Show();*/



            progressToastContent = new ToastContentBuilder()
                .AddText("Receiving Files from " + connectedInfo.name)
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = "Transfer Progress",
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = "Receiving Files"
                })
                .GetToastContent();
            progressToast = new ToastNotification(progressToastContent.GetXml());
            progressToast.Tag = "receiving-files";
            progressToast.Data = new NotificationData();
            progressToast.Data.Values["progressValue"] = "0.0";
            progressToast.Data.Values["progressValueString"] = "0%";
            progressToast.Data.SequenceNumber = 0;
            // ToastNotificationManager.CreateToastNotifier(APP_ID).Show(progressToast);
        }

        private static void close()
        {
            //form.Close();
        }

        private static void displayConnectedInfo()
        {
            if (window.IsVisible)
                label.Text = "Connected with " + connectedInfo.name;
        }

        private static void changeDisplayedFileName()
        {
            if (window.IsVisible)
                FileNameLabel.Text = currFileName;
        }

        private static void setStatusTransferInProgress()
        {
            if (window.IsVisible)
            {
                StatusLabel.Text = "Status: File Transfer in Progress";
                if (threads != 1)
                {
                    FileNameLabel.Text = "File Progresses";
                }
            }
        }

        private static void disableProgress()
        {
            progressOverlay.Close();
            progressOverlay = null;
            if (window.IsVisible)
            {
                ProgressBar1.Value = 100;
                ProgressBar2.Value = 100;
                if (MultiThreadFileProgressBars != null)
                {
                    foreach (var progressBar in MultiThreadFileProgressBars)
                    {
                        progressBar.Value = 100;
                    }
                }

                overallProgressPercentageLabel.Text = "100%";
                fileProgressPercentageLabel.Text = "100%";
                fileRelativeProgress.Text = "";
                StatusLabel.Text = "Status: File Transfer Complete";
            }

            bool hasErrors = false;

            foreach (var item in ReceivingFilesListViewItems)
            {
                hasErrors |= item.downloadFailed;
            }

            if (hasErrors)
            {
                if (window.IsVisible)
                    StatusLabel.Text = "Status: Files Transferred with Errors";
                new ToastContentBuilder()
                    .AddText(connectedInfo.name)
                    .AddText("Files Transferred with Errors")
                    .AddButton(new ToastButton()
                        .SetContent("Show Files")
                        .AddArgument("action", "showReceived").SetBackgroundActivation())
                    .Show();
            }
            else
            {
                new ToastContentBuilder()
                    .AddText(connectedInfo.name)
                    .AddText("File Transfer Complete")
                    .AddButton(new ToastButton()
                        .SetContent("Show Files")
                        .AddArgument("action", "showReceived").SetBackgroundActivation())
                    .Show();

            }
        }


        private static void changeProgress()
        {

        }

        private static void enableAllButtons()
        {
            if (window.IsVisible)
            {
                selectFileButton.IsEnabled = true;
                selectFilesToSendButton.IsEnabled = true;
                selectFolderToSendButton.IsEnabled = true;
                openReceivedButton.IsEnabled = true;
            }
        }

        private static void disableAllButtons()
        {
            if (window.IsVisible)
            {
                selectFileButton.IsEnabled = false;
                selectFilesToSendButton.IsEnabled = false;
                selectFolderToSendButton.IsEnabled = false;
                openReceivedButton.IsEnabled = false;
            }
        }


        public MainWindow(Dictionary<string, string> args, bool isBackground = false)
        {
            MainWindow.args = args;
            //ApplicationDeployment
            preferences = new Preferences();
            ToastNotificationManagerCompat.OnActivated += (args) =>
            {
                if (args.Argument.Equals("action=showReceived"))
                {
                    Process.Start("explorer.exe", preferences.getReceivingBaseLocation(timeStamp));
                }
            };
            XamlFlair.Animations.OverrideDefaultSettings(duration: 750, easing: EasingType.Quintic);
            InitializeComponent();
            DateTime time = DateTime.Now;
            timeStamp = time.ToString("yyyy-MM-dd HH-mm-ss");
            Console.WriteLine(timeStamp);
            checkAndCreateDirectoryForApplication();
            this.Opacity = 0;
            backgroundImage = background_img;
            QRCodePictureBox = QRCodeImageView;
            openReceivedButton = openReceived;
            selectFileButton = button1;
            selectFilesToSendButton = openSelectFilesDialog;
            selectFolderToSendButton = openSelectFolderDialog;
            ViewListButton = viewListButton;
            HideListViewButton = hideListViewButton;
            ProgressBar1 = progressBar1;
            ProgressBar2 = progressBar2;
            QRCodeViewPanel = QRCodeView;
            ConnectedViewPanel = ConnectedView;
            settingsView = SettingsView;
            mainProgressViews = MainProgressViews;
            multiThreadFileProgressBarsPanel = MultiThreadFileProgressBarsPanel;
            label = label1;
            FileNameLabel = currentFileNameLabel;
            StatusLabel = statusLabel;
            fileProgressPercentageLabel = fileProgressLabel;
            overallProgressPercentageLabel = overallProgressLabel;
            fileRelativeProgress = relativeFileProgressLabel;
            relativeOverallProgressLabel = relativeProgressLabel;
            visibleAsLabel = VisibileNameTextBlock;
            instructionLabel = instructionTextBlock;
            TotalProgressLabel = totalProgressLabel;
            NoFilesReceivedTextBlockOnListView = noFilesReceivedTextBlockOnListView;
            NoFilesSentTextBlockOnListView = noFilesSentTextBlockOnListView;
            window = this;
            plusIconTint = PlusIconTint;
            gearIconTint = GearIconTint;
            backIconTint = BackIconTint;
            receivingListView = currentTransfersListView;
            sendingListView = currentSendingListView;
            openReceivedButton.Click += openReceived_Click;
            selectFileButton.Click += minimizeToTrayClick;
            selectFilesToSendButton.Click += openFileSelectorClick;
            selectFolderToSendButton.Click += openFolderSelectorClick;

            try
            {
                using (RegistryKey key =
                       Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
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

            autoSetTransparency();
            this.SizeChanged += (s, e) => { windowSizeChange(e.NewSize); };

            this.SizeChanged += (s, e1) =>
            {

                var dimension = 0.0;
                if (e1.NewSize.Width < e1.NewSize.Height)
                {
                    dimension = e1.NewSize.Width;
                }
                else
                {
                    dimension = e1.NewSize.Height;
                }

                QRCodeImageView.Width = dimension / 1.8;
                QRCodeImageView.Height = QRCodeImageView.Width;
            };
            RefreshAccentedItems();
            receivingListView.SelectionMode = System.Windows.Controls.SelectionMode.Single;
            sendingListView.SelectionMode = System.Windows.Controls.SelectionMode.Single;
            int port = GetAvailablePort(1234);
            if (isBackground)
            {
                port = GetAvailablePort(42000);
            }

            runQRGenerator(port);
            server = new HTTPSERVER(null, port, SendingFilesListViewItems, new ServerListenerClass(), isBackground);
            VisibileNameTextBlock.Text = "Visible as \"" + HTTPSERVER.visibleName + "\"";
            server.startSimpleServer();
            list = new List<MyClass>();
            Thread t = new Thread(new ThreadStart(reg_watcher_ui_mode));
            t.IsBackground = true;
            t.Start();




            NearbyPCButton.Click += (_, __) =>
            {

                dialog = new ContentDialog()
                {

                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,

                };
                dialog.Content = new System.Windows.Controls.Frame()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    Content = new NearbyPCList(dialog, port)
                };
                dialog.ShowAsync();
            };



            if (isBackground)
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\*\shell", true))
                {
                    var keys = key.GetSubKeyNames();
                    foreach (var s in keys)
                    {
                        if (s.StartsWith("Send via FileWire to "))
                        {
                            key.DeleteSubKeyTree(s);
                        }
                    }
                }
                watchRegistryAndTryToSendFiles();
            }

            /*Tried To Measure Speed Here
            *
            *
            *
            *
            *
            *
            *
            *new Thread(new ThreadStart(() => {
                var FilesReceivingReceivingSizeOld = FilesReceivingReceivingSize;

                long ticksOld = 0;
                long[] oldDifferences = new long[10];
                while (true)
                {
                    long diffNow = (long)((double)((FilesReceivingReceivingSize - FilesReceivingReceivingSizeOld) / (DateTime.Now.Ticks - ticksOld)) * 10000.00 * 1000.00);
                    FilesReceivingReceivingSizeOld = FilesReceivingReceivingSize;
                    ticksOld = DateTime.Now.Ticks;
                    for (int i = 1; i < oldDifferences.Length; i++)
                    {
                        oldDifferences[i - 1] = oldDifferences[i];
                    }
                    oldDifferences[oldDifferences.Length - 1] = diffNow;
                    long totalDiff = 0;
                    foreach (long diff in oldDifferences)
                    {
                        totalDiff += diff;
                    }
                    totalDiff /= oldDifferences.Length;
                    System.Diagnostics.Trace.WriteLine(getFormatSize(totalDiff) + "/s");
                    Thread.Sleep(100);
                    
                }
            })).Start();*/
        }

        private void watchRegistryAndTryToSendFiles()
        {
            Registry.CurrentUser.CreateSubKey("SOFTWARE\\FileWire\\Files").Close();
            var monitor = new RegistryMonitor("HKEY_CURRENT_USER\\SOFTWARE\\FileWire\\Files");
            monitor.RegChangeNotifyFilter = RegChangeNotifyFilter.Key;
            monitor.RegChanged += (s, e) =>
            {
                try
                {
                    var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\FileWire\\Files");
                    key.DeleteSubKeyTree("Send");
                    var filePaths = new List<String>();
                    var names = key.GetValueNames();
                    foreach (var name in names)
                    {
                        filePaths.Add(key.GetValue(name) as string);
                        key.DeleteValue(name);
                    }

                    key.Close();


                    Logging.createNew("Num Files", filePaths.Count.ToString());


                    MyClass[] filesToSend = new MyClass[filePaths.Count()];
                    int count = 0;
                    bool hasToSend = true;
                    foreach (string name in filePaths)
                    {
                        string path = name;
                        if (name.EndsWith(@":\"))
                        {
                            hasToSend = false;
                            path = name.Substring(0, name.Length - 1);
                            System.Windows.Forms.MessageBox.Show(
                                "Can't send a complete volume, please select only a folder/folders.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            
                            FileAttributes attr = File.GetAttributes(path);
                            filesToSend[count] = new MyClass();
                            filesToSend[count].id = count.ToString();
                            filesToSend[count].fileName = path.Substring(path.LastIndexOf(@"\") + 1);
                            filesToSend[count].link = "file:" + System.Web.HttpUtility.UrlEncode(path);
                            filesToSend[count].fileSize = ((attr & FileAttributes.Directory) != FileAttributes.Directory)?new FileInfo(path).Length.ToString():DirSize(new DirectoryInfo(path)).ToString();
                            filesToSend[count].isFolder = ((attr & FileAttributes.Directory) != FileAttributes.Directory)?"false":"true";
                            Console.WriteLine(path);
                            count++;
                        }
                    }

                    if (hasToSend)
                    {
                        using (System.Net.WebClient wb = new System.Net.WebClient())
                        {
                            server.setFiles(
                                "<html><body>" + JsonConvert.SerializeObject(filesToSend) + "</body></html>");
                            Logging.createNew("1", iPAddress + ":" + mobilePort + "/" + port.ToString());
                            Console.WriteLine(wb.DownloadString("http://" + iPAddress + ":" + mobilePort +
                                                                "/incoming:" + port.ToString()));
                        }
                    }




                }
                catch
                {
                }
            };
            monitor.Start();
        }

        private void NOP(double durationSeconds = 100)
        {
            var durationTicks = Math.Round(durationSeconds * Stopwatch.Frequency);
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedTicks < durationTicks)
            {

            }
        }

        private static void windowSizeChange(System.Windows.Size e)
        {
            receivingListView.MaxHeight = e.Height - 140;
            sendingListView.MaxHeight = e.Height - 140;
            if (e.Width > 750)
            {
                if (e.Width <= 420)
                {
                    receivingListView.Width = window.Width - 45;
                    sendingListView.Width = window.Width - 45;
                }
                else
                {
                    receivingListView.Width = 400;
                    sendingListView.Width = 400;
                }

                if (ConnectedViewPanel.IsVisible)
                {
                    receivingListView.Visibility = Visibility.Visible;
                    sendingListView.Visibility = Visibility.Visible;
                    receivingListView.ItemsSource = ReceivingFilesListViewItems;
                    sendingListView.ItemsSource = SendingFilesListViewItems;
                    mainProgressViews.Visibility = Visibility.Visible;
                }

                ViewListButton.Visibility = Visibility.Collapsed;
                HideListViewButton.Visibility = Visibility.Hidden;
            }
            else
            {
                receivingListView.Width = window.Width - 45;
                sendingListView.Width = window.Width - 45;
                if (mainProgressViews.IsVisible)
                {
                    receivingListView.Visibility = Visibility.Collapsed;
                    sendingListView.Visibility = Visibility.Collapsed;
                }

                ViewListButton.Visibility = Visibility.Visible;
            }

        }



        private static void run()
        {
            using (System.Net.WebClient wb = new System.Net.WebClient())
            {
                try
                {


                    var fileList = wb.DownloadString(host + "request");
                    fileList = fileList.Substring(fileList.IndexOf("<body>") + 6,
                        fileList.IndexOf("</body>") - (fileList.IndexOf("<body>") + 6));

                    List<MyClass> newList = JsonConvert.DeserializeObject<List<MyClass>>(fileList);

                    window.Dispatcher.BeginInvoke((Action)(() => { NoFilesReceivedTextBlockOnListView.Text = ""; }));
                    foreach (MyClass file in newList)
                    {
                        file.fileName =
                            new FileInfo(GetUniqueFilePath(preferences.getReceivingLocation(file.fileName, timeStamp) +
                                                           "\\" + file.fileName)).Name;
                        window.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            ReceivingFilesListViewItems.Add(new fileProgressClass()
                            {
                                id = (ReceivingFilesListViewItems.Count + 1).ToString() + ". ",
                                fileName = file.fileName,
                                progressPercent = 0
                            });
                        }));

                        FilesReceivingTotalSize += long.Parse(file.fileSize);


                        int count = 0;
                        int smallestListPos = 0;
                        int smallestListSize = 0;
                        foreach (var thread in receivingFilesList)
                        {
                            if (smallestListSize == 0 && count == 0)
                            {
                                smallestListSize = thread.List.Count;
                            }

                            if (thread.List.Count < smallestListSize)
                            {
                                smallestListSize = thread.List.Count;
                                smallestListPos = count;
                            }

                            count++;
                        }

                        receivingFilesList.ElementAt(smallestListPos).List.Add(file);
                        list.Add(file);
                    }

                    foreach (var thread in receivingFilesList)
                    {
                        Console.WriteLine(thread.List.Count.ToString());
                    }

                }
                catch (Exception e)
                {
                }
            }
        }

        private static List<string> reservedNames = new List<string>();

        private static string GetUniqueFilePath(string filePath)
        {
            if (File.Exists(filePath) || reservedNames.Contains(filePath))
            {
                string folderPath = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);
                int number = 1;

                Match regex = Regex.Match(fileName, @"^(.+) \((\d+)\)$");

                if (regex.Success)
                {
                    fileName = regex.Groups[1].Value;
                    number = int.Parse(regex.Groups[2].Value);
                }

                do
                {
                    number++;
                    string newFileName = $"{fileName} ({number}){fileExtension}";
                    filePath = Path.Combine(folderPath, newFileName);
                } while (File.Exists(filePath) || reservedNames.Contains(filePath));
            }

            reservedNames.Add(filePath);
            return filePath;
        }


        private static void downloadFile(object e, int threadNum)
        {
            var thread = receivingFilesList.ElementAt(threadNum);
            if (thread.PointerLocation > 0)
            {
                if (thread.FileCurrentReceivedSize !=
                    long.Parse(thread.List.ElementAt(thread.PointerLocation - 1).fileSize))
                {

                    if (bool.Parse(thread.List.ElementAt(thread.PointerLocation - 1).isFolder))
                    {
                        try
                        {
                            Directory.Delete(
                                preferences.getReceivingLocation(
                                    thread.List.ElementAt(thread.PointerLocation - 1).fileName, timeStamp) +
                                System.IO.Path.DirectorySeparatorChar.ToString() +
                                thread.List.ElementAt(thread.PointerLocation - 1).fileName, true);
                        }
                        catch (DirectoryNotFoundException)
                        {

                        }
                    }
                    else
                    {
                        File.Delete(
                            preferences.getReceivingLocation(thread.List.ElementAt(thread.PointerLocation - 1).fileName,
                                timeStamp) + System.IO.Path.DirectorySeparatorChar.ToString() +
                            thread.List.ElementAt(thread.PointerLocation - 1).fileName);
                    }

                    FilesReceivingReceivingSize -= thread.FileCurrentReceivedSize;
                    thread.FileCurrentReceivedSize = 0;
                    window.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (ProgressBar2.Value != overallProgress)
                        {
                            ProgressBar2.Value = overallProgress;
                        }

                        overallProgressPercentageLabel.Text = overallProgress.ToString() + "%";
                        relativeOverallProgressLabel.Text = getFormatSize(FilesReceivingReceivingSize) + " of " +
                                                            getFormatSize(FilesReceivingTotalSize);
                    }));
                    window.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ProgressBar1.Foreground =
                            new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 173, 78));
                        ProgressBar2.Foreground =
                            new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 173, 78));
                        if (MultiThreadFileProgressBars != null)
                        {
                            foreach (var progressBar in MultiThreadFileProgressBars)
                            {
                                progressBar.Foreground =
                                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 173, 78));
                            }
                        }
                    }));

                    var globalLocation = ((thread.PointerLocation - 1) * threads) + threadNum;
                    ReceivingFilesListViewItems.ElementAt(globalLocation).downloadFailed = true;
                }
            }

            if (thread.PointerLocation < thread.List.Count)
            {
                thread.PointerLocation++;
                string fileURL = host + thread.List.ElementAt(thread.PointerLocation - 1).link;
                string fileName = thread.List.ElementAt(thread.PointerLocation - 1).fileName;
                Boolean isFolder = false;
                if (thread.List.ElementAt(thread.PointerLocation - 1).isFolder != null &&
                    thread.List.ElementAt(thread.PointerLocation - 1).isFolder.Length > 0)
                {
                    isFolder = Boolean.Parse(thread.List.ElementAt(thread.PointerLocation - 1).isFolder);
                }

                thread.FileCurrentReceivedSize = 0;
                ConnectedViewPanel.Dispatcher.BeginInvoke(statusDownloading);
                using (var dl = new Downloader(new Uri(fileURL),
                           preferences.getReceivingLocation(fileName, timeStamp) +
                           System.IO.Path.DirectorySeparatorChar.ToString() + fileName, isFolder))
                {
                    currFileName = fileName;
                    dl.setOnDownloadProgressEventHandler(new Downloader.DownloadProgressChangedEventHandler(
                        (Action<object, Downloader.DownloadProgressChangedEventHandler.
                            DownloadProgressChangedEventArgs>)((s, e) => { progressChange(s, e, threadNum); })));
                    dl.setOnDownloadCompleteEventHandler(
                        new Downloader.DownloadCompleteEventHandler((Action<object>)((e) =>
                        {
                            downloadFile(e, threadNum);
                        })));
                }
            }
            else
            {
                thread.CommandToDownload = true;
                bool isReceiveComplete = true;
                foreach (var t in receivingFilesList)
                {
                    isReceiveComplete &= t.CommandToDownload;
                }

                if (isReceiveComplete)
                {
                    Console.WriteLine("Complete");
                    ConnectedViewPanel.Dispatcher.BeginInvoke(disable);
                    ConnectedViewPanel.Dispatcher.BeginInvoke(formClose);
                    Console.WriteLine("Complete");
                    currFileName = "Transfer Complete";
                    bool hasErrors = false;

                    foreach (var item in ReceivingFilesListViewItems)
                    {
                        hasErrors |= item.downloadFailed;
                    }

                    if (hasErrors)
                    {
                        currFileName += "d with Errors";
                    }

                    ConnectedViewPanel.Dispatcher.BeginInvoke(displayFileName);
                }

                /*ConnectedViewPanel.Dispatcher.BeginInvoke(disable);
                ConnectedViewPanel.Dispatcher.BeginInvoke(formClose);
                Console.WriteLine("Complete");
                currFileName = "Transfer Complete";
                bool hasErrors = false;
                
                foreach (var item in ReceivingFilesListViewItems)
                {
                    hasErrors |= item.downloadFailed;
                }
                if (hasErrors)
                {
                    currFileName += "d with Errors";
                }*/
                //this.Close();
            }

            if (threads == 1)
            {
                ConnectedViewPanel.Dispatcher.BeginInvoke(displayFileName);
            }
        }

        private static void progressChange(object sender,
            Downloader.DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs e, int threadNum)
        {
            var thread = receivingFilesList.ElementAt(threadNum);
            var globalLocation = ((thread.PointerLocation - 1) * threads) + threadNum;
            thread.FileCurrentReceivedSize += e.BytesReceived;
            var fileProgress = (int)((thread.FileCurrentReceivedSize * 100) /
                                     long.Parse(thread.List.ElementAt(thread.PointerLocation - 1).fileSize));
            FilesReceivingReceivingSize += e.BytesReceived;
            overallProgress = (int)((FilesReceivingReceivingSize * 100) / FilesReceivingTotalSize);
            progressOverlay.setProgress(overallProgress,
                getFormatSize(FilesReceivingReceivingSize) + " of " + getFormatSize(FilesReceivingTotalSize));
            //var data = new NotificationData();
            //data.Values["progressValue"] = (((Double)overallProgress) / 100.0).ToString();
            //data.Values["progressValueString"] = overallProgress.ToString() + "%";
            //ToastNotificationManager.CreateToastNotifier(APP_ID).Update(data, "receiving-files");
            try
            {
                ConnectedViewPanel.Dispatcher.BeginInvoke((Action)(() =>
                {

                    if (ProgressBar1.Value != fileProgress)
                    {
                        ProgressBar1.Value = fileProgress;
                    }

                    if (MultiThreadFileProgressBars != null && MultiThreadFileProgressBars.Length > threadNum &&
                        MultiThreadFileProgressBars[threadNum] != null)
                    {
                        MultiThreadFileProgressBars[threadNum].Value = fileProgress;
                    }

                    if (ProgressBar2.Value != overallProgress)
                    {
                        ProgressBar2.Value = overallProgress;
                    }

                    overallProgressPercentageLabel.Text = overallProgress.ToString() + "%";
                    relativeOverallProgressLabel.Text = getFormatSize(FilesReceivingReceivingSize) + " of " +
                                                        getFormatSize(FilesReceivingTotalSize);

                    if (threads == 1)
                    {
                        fileProgressPercentageLabel.Text = fileProgress.ToString() + "%";
                        fileRelativeProgress.Text = getFormatSize(thread.FileCurrentReceivedSize) + " of " +
                                                    getFormatSize(long.Parse(thread.List
                                                        .ElementAt(thread.PointerLocation - 1).fileSize));
                    }


                    ReceivingFilesListViewItems.ElementAt(globalLocation).progressPercent = fileProgress;
                }));
            }
            catch
            {
            }
        }

        public int getThisPCPort()
        {
            return MainWindow.port;
        }

        private void runQRGenerator(int port)
        {
            MainWindow.port = port;
            QRCodePictureBox.Visibility = Visibility.Visible;
            Thread runner = new Thread(new ThreadStart(GenerateQRCode));
            runner.IsBackground = true;
            runner.Start();
        }


        private void GenerateQRCode()
        {
            while (true)
            {

                System.Net.IPAddress[] addresses =
                    System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                string add = "";
                foreach (System.Net.IPAddress ip in addresses)
                {
                    add += "http://" + ip.ToString() + ":" + port.ToString() + "/\n";
                }

                GenerateMyQCCode(add);
                Thread.Sleep(500);
            }
        }

        private static Bitmap qrCode = null;

        private void GenerateMyQCCode(string QCText)
        {
            var QCwriter = new BarcodeWriter();
            QCwriter.Format = BarcodeFormat.QR_CODE;
            QCwriter.Options.Width = 256;
            QCwriter.Options.Height = 256;
            var result = QCwriter.Write(QCText);
            qrCode = result;

            try
            {
                Dispatcher.BeginInvoke(showQRCode);
            }
            catch (Exception)
            {

            }
        }

        private static void showQRConnectCode()
        {
            QRCodePictureBox.Source = Bitmap2BitmapImage(qrCode);
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }


        public static int GetAvailablePort(int startingPort)
        {
            System.Net.IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                where n.LocalEndPoint.Port >= startingPort
                select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                where n.Port >= startingPort
                select n.Port);

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                where n.Port >= startingPort
                select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }

        private static void checkAndCreateDirectoryForApplication(string subFolder = null)
        {
            if (subFolder == null)
            {
                checkAndCreateDirectoryForApplication(".jpg");
                checkAndCreateDirectoryForApplication(".mp4");
                checkAndCreateDirectoryForApplication(".txt");
                checkAndCreateDirectoryForApplication(".zip");
                checkAndCreateDirectoryForApplication(".apk");
                checkAndCreateDirectoryForApplication(".mp3");
                checkAndCreateDirectoryForApplication(DirectoryDefault);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(preferences.getReceivingLocation(subFolder, timeStamp));
                if (!Directory.Exists(preferences.getReceivingLocation(subFolder, timeStamp)))
                {
                    Directory.CreateDirectory(preferences.getReceivingLocation(subFolder, timeStamp));
                }
            }
        }

        private static string getFormatSize(long size)
        {
            double len = size;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private void reg_watcher_ui_mode()
        {
            bool lightTheme = true;
            int themeSetting = 0;
            int transparencySetting = 0;
            bool TransparencyEnabled = true;
            System.Windows.Media.Color accentColor = SystemParameters.WindowGlassColor;
            while (true)
            {
                try
                {
                    if (themeSetting == 0)
                    {
                        using (RegistryKey key =
                               Registry.CurrentUser.OpenSubKey(
                                   @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                        {
                            if (key != null)
                            {
                                Object o = key.GetValue("AppsUseLightTheme");
                                if (o != null)
                                {
                                    bool isLighttheme = true;
                                    if (o.Equals(0))
                                    {
                                        isLighttheme = false;
                                    }

                                    if (isLighttheme != lightTheme)
                                    {
                                        lightTheme = isLighttheme;
                                        if (isLighttheme)
                                        {

                                            Dispatcher.BeginInvoke(switchToLightTheme);
                                        }
                                        else
                                        {
                                            Dispatcher.BeginInvoke(switchToDarkTheme);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) //just for demonstration...it's always best to handle specific exceptions
                {
                    Console.WriteLine(ex.ToString());
                    //react appropriately
                }

                try
                {
                    if (transparencySetting == 0)
                    {
                        using (RegistryKey key =
                               Registry.CurrentUser.OpenSubKey(
                                   @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                        {
                            if (key != null)
                            {
                                Object o = key.GetValue("EnableTransparency");
                                if (o != null)
                                {
                                    bool isTransparencyEnabled = true;
                                    if (o.Equals(0))
                                    {
                                        isTransparencyEnabled = false;
                                    }

                                    if (isTransparencyEnabled != TransparencyEnabled)
                                    {
                                        TransparencyEnabled = isTransparencyEnabled;
                                        if (isTransparencyEnabled)
                                        {

                                            autoSetTransparency();
                                        }
                                        else
                                        {
                                            autoSetTransparency();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                if (!SystemParameters.WindowGlassColor.Equals(accentColor))
                {
                    accentColor = SystemParameters.WindowGlassColor;
                    Dispatcher.BeginInvoke(refreshAllAccentedItems);
                }

                //   System.Diagnostics.Trace.WriteLine
                //     (System.Diagnostics.Process.GetProcessesByName("FileWire").Length);
                Thread.Sleep(200);
            }
        }



        private static void setLightColors()
        {
            isLightMode = true;
            if (progressOverlay != null)
            {
                progressOverlay.setLightColors();
            }

            MaterialDesignThemes.Wpf.ThemeAssist.SetTheme(window, MaterialDesignThemes.Wpf.BaseTheme.Light);
            AcrylicWindow.SetTintColor(window, System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            backgroundImage.Source = new BitmapImage(new Uri("Resources/background.jpeg", UriKind.Relative));
            System.Windows.Controls.Button button = openReceivedButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 150, 150, 150));
            button.Foreground = System.Windows.Media.Brushes.DimGray;
            button = selectFileButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 150, 150, 150));
            button.Foreground = System.Windows.Media.Brushes.DimGray;
            button = selectFilesToSendButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 150, 150, 150));
            button.Foreground = System.Windows.Media.Brushes.DimGray;
            button = selectFolderToSendButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 150, 150, 150));
            button.Foreground = System.Windows.Media.Brushes.DimGray;

            System.Windows.Media.Brush TextColor = System.Windows.Media.Brushes.Black;
            label.Foreground = TextColor;
            FileNameLabel.Foreground = TextColor;
            fileProgressPercentageLabel.Foreground = TextColor;
            overallProgressPercentageLabel.Foreground = TextColor;
            relativeOverallProgressLabel.Foreground = TextColor;
            fileRelativeProgress.Foreground = TextColor;
            StatusLabel.Foreground = TextColor;
            instructionLabel.Foreground = TextColor;
            visibleAsLabel.Foreground = TextColor;
            TotalProgressLabel.Foreground = TextColor;
            //backIconTint.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
        }

        private static void setDarkColors()
        {
            isLightMode = false;
            if (progressOverlay != null)
            {
                progressOverlay.setDarkColors();
            }

            MaterialDesignThemes.Wpf.ThemeAssist.SetTheme(window, MaterialDesignThemes.Wpf.BaseTheme.Dark);
            AcrylicWindow.SetTintColor(window, System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
            backgroundImage.Source = new BitmapImage(new Uri("Resources/background_dark.jpg", UriKind.Relative));
            System.Windows.Controls.Button button = openReceivedButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 50, 50, 50));
            button.Foreground = System.Windows.Media.Brushes.LightGray;
            button = selectFileButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 50, 50, 50));
            button.Foreground = System.Windows.Media.Brushes.LightGray;
            button = selectFilesToSendButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 50, 50, 50));
            button.Foreground = System.Windows.Media.Brushes.LightGray;
            button = selectFolderToSendButton;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 50, 50, 50));
            button.Foreground = System.Windows.Media.Brushes.LightGray;

            System.Windows.Media.Brush TextColor = System.Windows.Media.Brushes.White;
            label.Foreground = TextColor;
            FileNameLabel.Foreground = TextColor;
            fileProgressPercentageLabel.Foreground = TextColor;
            overallProgressPercentageLabel.Foreground = TextColor;
            relativeOverallProgressLabel.Foreground = TextColor;
            fileRelativeProgress.Foreground = TextColor;
            StatusLabel.Foreground = TextColor;
            instructionLabel.Foreground = TextColor;
            visibleAsLabel.Foreground = TextColor;
            TotalProgressLabel.Foreground = TextColor;
            // backIconTint.Color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }


        private static void makeWindowOpaque()
        {
            AcrylicWindow.SetTintOpacity(window, 1);
            backgroundImage.Opacity = 1;
        }

        private static void makeWindowTransparent()
        {
            AcrylicWindow.SetTintOpacity(window, 0.5);
            backgroundImage.Opacity = 0.51;
        }

        private static void RefreshAccentedItems()
        {
            System.Windows.Media.Color accentColor = AccentColors.ImmersiveSystemAccent;
            plusIconTint.Color = accentColor;
            gearIconTint.Color = accentColor;
            bool hasErrors = false;

            foreach (var item in ReceivingFilesListViewItems)
            {
                hasErrors |= item.downloadFailed;
            }

            if (!hasErrors)
            {
                ProgressBar1.Foreground = AccentColors.ImmersiveSystemAccentBrush;
                ProgressBar2.Foreground = AccentColors.ImmersiveSystemAccentBrush;
                if (MultiThreadFileProgressBars != null)
                {
                    foreach (var progressBar in MultiThreadFileProgressBars)
                    {
                        progressBar.Foreground = AccentColors.ImmersiveSystemAccentBrush;
                    }
                }
            }
        }

        private static void autoSetTransparency()
        {
            if (preferences.IsTransparencyAutomatic())
            {
                try
                {
                    using (RegistryKey key =
                           Registry.CurrentUser.OpenSubKey(
                               @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    {
                        if (key != null)
                        {
                            Object o = key.GetValue("EnableTransparency");
                            if (o != null)
                            {
                                if (o.Equals(1))
                                {
                                    window.Dispatcher.BeginInvoke(switchToTransparentWindow);
                                }
                                else
                                {
                                    window.Dispatcher.BeginInvoke(switchToOpaqueWindow);
                                }
                            }
                        }
                    }

                }
                catch (Exception)
                {
                }
            }
            else if (preferences.IsTransparencyEnabled().HasValue && preferences.IsTransparencyEnabled() == true)
            {
                window.Dispatcher.BeginInvoke(switchToTransparentWindow);
            }
            else
            {
                window.Dispatcher.BeginInvoke(switchToOpaqueWindow);
            }
        }

        Boolean hasCloseAnimationPlayed = false;

        private void onAppClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !hasCloseAnimationPlayed;

            if (e.Cancel)
            {
                DoubleAnimation anim = new DoubleAnimation()
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200))
                };
                anim.Completed += fadeOutComplete;
                this.BeginAnimation(Window.OpacityProperty, anim);
            }
        }

        private void fadeOutComplete(object sender, EventArgs e)
        {
            hasCloseAnimationPlayed = true;
            this.Close();
            hasCloseAnimationPlayed = false;
        }

        public class ServerListenerClass
        {

            public void autoSendFiles()
            {
                if (args.ContainsKey("Files"))
                {
                    string[] fileNames = args.GetValueOrDefault("Files").Split("\n");
                    MyClass[] filesToSend = new MyClass[fileNames.Length - 1];
                    int count = 0;
                    bool hasToSend = true;

                    foreach (string name in fileNames)
                    {
                        try
                        {
                            string path = name;
                            if (name.EndsWith(@":\"))
                            {
                                hasToSend = false;
                                path = name.Substring(0, name.Length - 1);
                                System.Windows.Forms.MessageBox.Show(
                                    "Can't send a complete volume, please select only a folder/folders.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                filesToSend[count] = new MyClass();
                                filesToSend[count].id = count.ToString();
                                filesToSend[count].fileName = path.Substring(path.LastIndexOf(@"\") + 1);
                                filesToSend[count].link = "file:" + System.Web.HttpUtility.UrlEncode(path);
                                FileAttributes attr = File.GetAttributes(path);

                                if (attr.HasFlag(FileAttributes.Directory))
                                {
                                    filesToSend[count].fileSize = DirSize(new DirectoryInfo(path)).ToString();
                                    filesToSend[count].isFolder = "true";
                                }
                                else
                                {
                                    filesToSend[count].fileSize = new FileInfo(path).Length.ToString();
                                    filesToSend[count].isFolder = "false";
                                }

                                Console.WriteLine(path);
                                count++;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }

                    if (hasToSend)
                    {
                        try
                        {
                            window.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                for (int j = 0; j < filesToSend.Length; j++)
                                {
                                    SendingFilesListViewItems.Add(new fileProgressClass()
                                    {
                                        fileName = filesToSend.ElementAt(j).fileName,
                                        fileSize = filesToSend.ElementAt(j).fileSize.ToString(),
                                        isFolder = "true",
                                        id = (SendingFilesListViewItems.Count + 1).ToString() + ". ",
                                        progressPercent = 0
                                    });
                                }

                                if (SendingFilesListViewItems.Count >= 1)
                                    NoFilesSentTextBlockOnListView.Text = "";

                            }));
                            using (System.Net.WebClient wb = new System.Net.WebClient())
                            {
                                //wb.DownloadString(iPAddress);
                                server.setFiles("<html><body>" + JsonConvert.SerializeObject(filesToSend) +
                                                "</body></html>");
                                Console.WriteLine(wb.DownloadString("http://" + iPAddress + ":" + mobilePort +
                                                                    "/incoming:" + port.ToString()));
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }

            public void printToLabel(string s)
            {
                //label.Visible = true;
                //label.Text += s;
                //label.Text += "\n";
            }

            public void connectedSuccessfully(string ip)
            {
                iPAddress = ip;
                ConnectedViewPanel.Dispatcher.BeginInvoke(connected);

            }

            public void gotMobilePort(int mobilePort)
            {
                MainWindow.mobilePort = mobilePort;
                Console.WriteLine(mobilePort.ToString());
                using (WebClient wb = new WebClient())
                {
                    //wb.DownloadString(iPAddress)
                    //    ;
                    try
                    {
                        String s = wb.DownloadString("http://" + iPAddress + ":" + mobilePort.ToString() +
                                                     "/getAvatarAndName");
                        AvtarAndName na =
                            JsonConvert.DeserializeObject<AvtarAndName>(s.Substring(s.IndexOf("<body>") + 6)
                                .Replace("</body></html>", ""));
                        MainWindow.connectedInfo = na;
                        new ToastContentBuilder().AddText("Connected Succesfully")
                            .AddText("Connected with " + connectedInfo.name).Show();
                        label.Dispatcher.BeginInvoke(displayConnectedDeviceInfo);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            public void incomingFiles(int mobilePort)
            {
                MainWindow.mobilePort = mobilePort;
                MainWindow.host = "http://" + iPAddress + ":" + mobilePort.ToString() + "/";
                MainWindow.ConnectedViewPanel.Dispatcher.BeginInvoke(incoming);
            }
        }



        private void maximiseFromTray(object sender, EventArgs e)
        {
            Show(this);
            ((NotifyIcon)sender).Visible = false;
            ((NotifyIcon)sender).Dispose();
        }

        private void minimizeToTrayClick(object sender, RoutedEventArgs e)
        {
            Hide(this);
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
            notifyIcon.DoubleClick += maximiseFromTray;
        }

        public static void Hide(MainWindow window)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 1.0f,
                To = 0.0f,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            anim.Completed += window.Hide;
            window.BeginAnimation(Window.OpacityProperty, anim);
        }

        public static void Show(MainWindow window)
        {
            window.Show();
            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 0.0f,
                To = 1.0f,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            window.BeginAnimation(Window.OpacityProperty, anim);
        }

        public void Hide(object send, EventArgs e)
        {
            this.Hide();
        }

        private void openReceived_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", preferences.getReceivingBaseLocation(timeStamp));
        }

        private class MyClass
        {
            public string id { get; set; }

            public string link { get; set; }

            public string fileSize { get; set; }

            public string fileName { get; set; }

            public string isFolder { get; set; }
        }

        public class NotificationProcess : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void RaisePropertyChanged(string PropertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
                }
            }
        }

        public class fileProgressClass : NotificationProcess
        {
            public string id { get; set; }

            public string fileSize { get; set; }

            public bool downloadFailed
            {
                set
                {
                    isFailed = value;
                    _progressPercent = 100;
                    this.RaisePropertyChanged("progressPercent");
                    this.RaisePropertyChanged("progressColor");
                    this.RaisePropertyChanged("showDownloadPercent");
                    this.RaisePropertyChanged("showFailIcon");
                    this.RaisePropertyChanged("showCheckIcon");
                }
                get { return isFailed; }
            }

            private bool isFailed = false;

            public string fileName { get; set; }

            public string isFolder { get; set; }

            public System.Windows.Media.Brush progressColor
            {
                get
                {
                    if (!isFailed)
                    {
                        if (progressPercent != 100)
                        {
                            return AccentColors.ImmersiveSystemAccentBrush;
                        }
                        else
                        {
                            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 92, 184, 92));
                        }
                    }
                    else
                    {
                        return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 217, 83, 79));
                    }
                }
            }

            private int _progressPercent = 0;

            public int progressPercent
            {
                get { return _progressPercent; }
                set
                {
                    _progressPercent = value;

                    this.RaisePropertyChanged("progressPercent");
                    this.RaisePropertyChanged("progressColor");
                    this.RaisePropertyChanged("showDownloadPercent");
                    this.RaisePropertyChanged("showFailIcon");
                    this.RaisePropertyChanged("showCheckIcon");
                }
            }

            public Visibility showDownloadPercent
            {
                get
                {
                    if (_progressPercent == 100)
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }

            public Visibility showFailIcon
            {
                get
                {
                    if (downloadFailed)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }

            public Visibility showCheckIcon
            {
                get
                {
                    if (_progressPercent == 100 && !downloadFailed)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
        }

        private MyClass[] filesToSend;
        private static bool isConnectedCurrently = false;

        private void openFileSelectorClick(object sender, EventArgs e)
        {

            Dispatcher.BeginInvoke(disableButtons);

            CommonOpenFileDialog saveFileDialog1 = new CommonOpenFileDialog();

            saveFileDialog1.IsFolderPicker = false;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Select Files to Send";
            saveFileDialog1.Multiselect = true;
            if (saveFileDialog1.ShowDialog() == CommonFileDialogResult.Ok)
            {
                IEnumerable<string> fileNames = saveFileDialog1.FileNames;
                MyClass[] filesToSend = new MyClass[fileNames.Count()];
                int count = 0;
                bool hasToSend = true;
                foreach (string name in fileNames)
                {
                    string path = name;
                    if (name.EndsWith(@":\"))
                    {
                        hasToSend = false;
                        path = name.Substring(0, name.Length - 1);
                        System.Windows.Forms.MessageBox.Show(
                            "Can't send a complete volume, please select only a folder/folders.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        filesToSend[count] = new MyClass();
                        filesToSend[count].id = count.ToString();
                        filesToSend[count].fileName = path.Substring(path.LastIndexOf(@"\") + 1);
                        filesToSend[count].link = "file:" + System.Web.HttpUtility.UrlEncode(path);
                        filesToSend[count].fileSize = new FileInfo(path).Length.ToString();
                        filesToSend[count].isFolder = "false";
                        Console.WriteLine(path);
                        count++;
                    }
                }

                if (hasToSend)
                {
                    window.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        for (int j = 0; j < filesToSend.Length; j++)
                        {
                            SendingFilesListViewItems.Add(new fileProgressClass()
                            {
                                fileName = filesToSend.ElementAt(j).fileName,
                                fileSize = filesToSend.ElementAt(j).fileSize.ToString(),
                                isFolder = "true",
                                id = (SendingFilesListViewItems.Count + 1).ToString() + ". ",
                                progressPercent = 0
                            });
                        }

                        if (SendingFilesListViewItems.Count >= 1)
                            noFilesSentTextBlockOnListView.Text = "";
                        using (System.Net.WebClient wb = new System.Net.WebClient())
                        {
                            //wb.DownloadString(iPAddress);
                            server.setFiles(
                                "<html><body>" + JsonConvert.SerializeObject(filesToSend) + "</body></html>");
                            Console.WriteLine(wb.DownloadString("http://" + iPAddress + ":" + mobilePort +
                                                                "/incoming:" + port.ToString()));

                        }
                    }));
                }

            }

            Dispatcher.BeginInvoke(enableButtons);

        }

        private void openFolderSelectorClick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(disableButtons);

            CommonOpenFileDialog saveFileDialog1 = new CommonOpenFileDialog();

            saveFileDialog1.IsFolderPicker = true;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Select Folders to Send";
            saveFileDialog1.Multiselect = true;
            if (saveFileDialog1.ShowDialog() == CommonFileDialogResult.Ok)
            {
                IEnumerable<string> fileNames = saveFileDialog1.FileNames;
                MyClass[] filesToSend = new MyClass[fileNames.Count()];
                int count = 0;
                bool hasToSend = true;
                foreach (string name in fileNames)
                {
                    string path = name;
                    if (name.EndsWith(@":\"))
                    {
                        hasToSend = false;
                        path = name.Substring(0, name.Length - 1);
                        System.Windows.Forms.MessageBox.Show(
                            "Can't send a complete volume, please select only a folder/folders.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        filesToSend[count] = new MyClass();
                        filesToSend[count].id = count.ToString();
                        filesToSend[count].fileName = path.Substring(path.LastIndexOf(@"\") + 1);
                        filesToSend[count].link = "file:" + System.Web.HttpUtility.UrlEncode(path);
                        filesToSend[count].fileSize = DirSize(new DirectoryInfo(path)).ToString();
                        filesToSend[count].isFolder = "true";
                        Console.WriteLine(path);
                        count++;
                    }
                }

                if (hasToSend)
                {
                    window.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        for (int j = 0; j < filesToSend.Length; j++)
                        {
                            SendingFilesListViewItems.Add(new fileProgressClass()
                            {
                                fileName = filesToSend.ElementAt(j).fileName,
                                fileSize = filesToSend.ElementAt(j).fileSize.ToString(),
                                isFolder = "true",
                                id = (SendingFilesListViewItems.Count + 1).ToString() + ". ",
                                progressPercent = 0
                            });
                        }

                        if (SendingFilesListViewItems.Count >= 1)
                            noFilesSentTextBlockOnListView.Text = "";
                        using (System.Net.WebClient wb = new System.Net.WebClient())
                        {
                            //wb.DownloadString(iPAddress);
                            server.setFiles(
                                "<html><body>" + JsonConvert.SerializeObject(filesToSend) + "</body></html>");
                            Console.WriteLine(wb.DownloadString("http://" + iPAddress + ":" + mobilePort +
                                                                "/incoming:" + port.ToString()));

                        }
                    }));
                }

            }

            Dispatcher.BeginInvoke(enableButtons);

        }




        private void openNewWindowClick(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.FriendlyName);
        }

        private void viewFileListClick(object sender, EventArgs e)
        {
            receivingListView.Visibility = Visibility.Visible;
            sendingListView.Visibility = Visibility.Visible;
            MainProgressViews.Visibility = Visibility.Collapsed;
            hideListViewButton.Visibility = Visibility.Visible;
            receivingListView.ItemsSource = ReceivingFilesListViewItems;
            sendingListView.ItemsSource = SendingFilesListViewItems;
        }

        private void hideListViewClick(object sender, EventArgs e)
        {
            receivingListView.Visibility = Visibility.Collapsed;
            sendingListView.Visibility = Visibility.Collapsed;
            MainProgressViews.Visibility = Visibility.Visible;
            hideListViewButton.Visibility = Visibility.Hidden;
            receivingListView.ItemsSource = ReceivingFilesListViewItems;
            sendingListView.ItemsSource = SendingFilesListViewItems;
        }



        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            try
            {
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }

                // Add subdirectory sizes.
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di);
                }

            }
            catch (UnauthorizedAccessException)
            {

            }

            return size;
        }

        private void showReceivedFile(object sender, EventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            var a = button.DataContext as fileProgressClass;
            int id = int.Parse(a.id.Substring(0, a.id.IndexOf(".")));
            id -= 1;
            string fileName = list.ElementAt(id).fileName;
            String filePath = preferences.getReceivingLocation(fileName, timeStamp) +
                              System.IO.Path.DirectorySeparatorChar.ToString() + fileName;
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        }





        private void openReceivedFile(object sender, EventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            var a = button.DataContext as fileProgressClass;
            int id = int.Parse(a.id.Substring(0, a.id.IndexOf(".")));
            string fileName = list.ElementAt(id - 1).fileName;
            String filePath = preferences.getReceivingLocation(fileName, timeStamp) +
                              System.IO.Path.DirectorySeparatorChar.ToString() + fileName;
            System.Diagnostics.Trace.WriteLine(filePath);
            if (!bool.Parse(list.ElementAt(id - 1).isFolder))
            {
                Process p = new Process();
                ProcessStartInfo ps = new ProcessStartInfo();
                ps.FileName = "CMD.exe";
                ps.Arguments = "/C \"" + filePath + "\"";
                ps.CreateNoWindow = true;
                p.StartInfo = ps;
                p.Start();
            }
            else
            {
                Process.Start("explorer.exe", filePath);
            }
            //explorer.exe /select,"C:\Folder\subfolder\file.txt"
        }

        private void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //gifImage.StartAnimation();
        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }




    }
}
