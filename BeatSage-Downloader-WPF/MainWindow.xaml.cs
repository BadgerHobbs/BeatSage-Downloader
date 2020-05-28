using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace BeatSage_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static DownloadManager downloadManager;

        public MainWindow()
        {
            InitializeComponent();
            downloadManager = new DownloadManager(dataGrid);

            dataGrid.ItemsSource = DownloadManager.Downloads;

            if (Directory.Exists("Downloads") == false)
            {
                Directory.CreateDirectory("Downloads");
            }
        }

        public void OpenAddDownloadWindow(object sender, RoutedEventArgs e)
        {
            AddDownloadWindow addDownloadWindow = new AddDownloadWindow();
            addDownloadWindow.Owner = this;
            addDownloadWindow.ShowDialog();
        }
        public void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }
        private void OnExit(object sender, ExitEventArgs e)
        {
            Properties.Settings.Default.Save();

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        public void MoveSelectedDownloadUp(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                selectedDownloads.Add(download);
            }

            selectedDownloads = selectedDownloads.OrderBy(o => DownloadManager.downloads.IndexOf(o)).ToList();

            foreach (Download download in selectedDownloads)
            {
                int selectedIndex = DownloadManager.downloads.IndexOf(download);

                if (selectedIndex - 1 >= 0)
                {
                    Download selectedDownloadItem = download;
                    Download downloadItemAbove = (Download)dataGrid.Items[selectedIndex - 1];
                    DownloadManager.downloads.Remove(selectedDownloadItem);
                    DownloadManager.downloads.Insert(selectedIndex - 1, selectedDownloadItem);
                    dataGrid.SelectedIndex = selectedIndex - 1;
                }
                else
                {
                    return;
                }
            }

            foreach (Download download in selectedDownloads)
            {
                dataGrid.SelectedItems.Add((Download)dataGrid.Items[DownloadManager.downloads.IndexOf(download)]);
            }

        }
        public void MoveSelectedDownloadDown(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                selectedDownloads.Add(download);
            }

            selectedDownloads = selectedDownloads.OrderBy(o => DownloadManager.downloads.IndexOf(o)).Reverse().ToList();

            foreach (Download download in selectedDownloads)
            {
                int selectedIndex = DownloadManager.downloads.IndexOf(download);

                if (selectedIndex + 1 < dataGrid.Items.Count)
                {
                    Download selectedDownloadItem = download;
                    Download downloadItemBelow = (Download)dataGrid.Items[selectedIndex + 1];
                    DownloadManager.downloads.Remove(selectedDownloadItem);
                    DownloadManager.downloads.Insert(selectedIndex + 1, selectedDownloadItem);
                    dataGrid.SelectedIndex = selectedIndex + 1;
                }
                else
                {
                    return;
                }
            }

            foreach (Download download in selectedDownloads)
            {
                dataGrid.SelectedItems.Add((Download)dataGrid.Items[DownloadManager.downloads.IndexOf(download)]);
            }
        }

        public void RetrySelectedDownload(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                selectedDownloads.Add(download);
            }

            foreach (Download download in selectedDownloads)
            {
                int selectedIndex = DownloadManager.downloads.IndexOf(download);

                download.Status = "Queued";
            }

            foreach (Download download in selectedDownloads)
            {
                dataGrid.SelectedItems.Add((Download)dataGrid.Items[DownloadManager.downloads.IndexOf(download)]);
            }
        }

        public void RemoveSelectedDownload(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                if (download.IsAlive == true)
                {
                    DownloadManager.cts.Cancel();
                    DownloadManager.cts.Dispose();
                }

                selectedDownloads.Add(download);
            }

            foreach (Download download in selectedDownloads)
            {
                DownloadManager.downloads.Remove(download);
            }

        }
    }

    public class Download : INotifyPropertyChanged
    {
        private int number;
        private string youtubeID;
        private string title;
        private string artist;
        private string status;
        private string difficulties;
        private string gameModes;
        private string songEvents;
        private string filePath;
        private string fileName;
        private string identifier;
        private string environment;
        private string modelVersion;
        private bool isAlive;

        public int Number
        {
            get
            {
                return number;
            }
            set
            {
                number = value;
                RaiseProperChanged();
            }
        }
        public string YoutubeID
        {
            get
            {
                return youtubeID;
            }
            set
            {
                youtubeID = value;
                if ((FileName == "") || (FileName == null))
                {
                    Identifier = value;
                }
                RaiseProperChanged();
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                RaiseProperChanged();
            }
        }

        public string Artist
        {
            get
            {
                return artist;
            }
            set
            {
                artist = value;
                RaiseProperChanged();
            }
        }

        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                RaiseProperChanged();
            }
        }

        public string Difficulties
        {
            get
            {
                return difficulties;
            }
            set
            {
                difficulties = value;
                RaiseProperChanged();
            }
        }

        public string GameModes
        {
            get
            {
                return gameModes;
            }
            set
            {
                gameModes = value;
                RaiseProperChanged();
            }
        }

        public string SongEvents
        {
            get
            {
                return songEvents;
            }
            set
            {
                songEvents = value;
                RaiseProperChanged();
            }
        }

        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
                RaiseProperChanged();
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                if ((YoutubeID == "") || (YoutubeID == null))
                {
                    Identifier = fileName;
                }
                RaiseProperChanged();
            }
        }

        public string Identifier
        {
            get
            {
                return identifier;
            }
            set
            {
                identifier = value;
                RaiseProperChanged();
            }
        }

        public string Environment
        {
            get
            {
                return environment;
            }
            set
            {
                environment = value;
                RaiseProperChanged();
            }
        }

        public string ModelVersion
        {
            get
            {
                return modelVersion;
            }
            set
            {
                modelVersion = value;
                RaiseProperChanged();
            }
        }

        public bool IsAlive
        {
            get
            {
                return isAlive;
            }
            set
            {
                isAlive = value;
                RaiseProperChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperChanged([CallerMemberName] string caller = "")
        {

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }

    public class DownloadManager
    {
        public static ObservableCollection<Download> downloads = new ObservableCollection<Download>();

        public static readonly HttpClient httpClient = new HttpClient();

        private static DataGrid dataGrid;

        public static CancellationTokenSource cts;

        public DownloadManager(DataGrid newDataGrid)
        {
            dataGrid = newDataGrid;

            httpClient.DefaultRequestHeaders.Add("Host", "beatsage.com");
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BeatSage-Downloader/1.1.2");

            Thread worker = new Thread(RunDownloads);
            worker.IsBackground = true;
            worker.SetApartmentState(System.Threading.ApartmentState.STA);
            worker.Start();

            //RunDownloads();
            
        }

        public async void RunDownloads()
        {
            Console.WriteLine("RunDownloads Started");

            int previousNumberOfDownloads = downloads.Count;

            while (true)
            {
                cts = new CancellationTokenSource();

                List<Download> incompleteDownloads = new List<Download>();

                foreach (Download download in downloads)
                {
                    if (download.Status == "Queued")
                    {
                        incompleteDownloads.Add(download);
                    }
                }

                Console.WriteLine("Checking for Downloads...");

                if (incompleteDownloads.Count >= 1)
                {
                    Download currentDownload = incompleteDownloads[0];
                    currentDownload.IsAlive = true;

                    if ((currentDownload.YoutubeID != "") && (currentDownload.YoutubeID != null))
                    {
                        string itemUrl = "https://www.youtube.com/watch?v=" + currentDownload.YoutubeID;

                        try
                        {
                            await RetrieveMetaData(itemUrl, currentDownload);
                        }
                        catch
                        {
                            currentDownload.Status = "Unable To Retrieve Metadata";
                            currentDownload.IsAlive = false;
                            cts.Dispose();
                            return;
                        }

                    }
                    else if ((currentDownload.FilePath != "") && (currentDownload.FilePath != null))
                    {
                        await CreateCustomLevelFromFile(currentDownload);
                        currentDownload.IsAlive = false;
                        cts.Dispose();
                        return;
                    }

                }

                cts.Dispose();
                System.Threading.Thread.Sleep(1000);
            }

            
        }

        public static ObservableCollection<Download> Downloads
        {
            get
            {
                return downloads;
            }
        }

        public void Add(Download download)
        {
            downloads.Add(download);
        }

        public async static Task RetrieveMetaData(string url, Download download)
        {
            download.Status = "Retrieving Metadata";

            var values = new Dictionary<string, string>
            {
                { "youtube_url", url }
            };

            string sJson = JsonConvert.SerializeObject(values);

            var httpContent = new StringContent(sJson, Encoding.UTF8, "application/json");

            Console.WriteLine(sJson);

            //POST the object to the specified URI 
            var response = await httpClient.PostAsync("https://beatsage.com/youtube_metadata", httpContent);

            //Read back the answer from server
            var responseString = await response.Content.ReadAsStringAsync();
            int attempts = 0;

            if (!response.IsSuccessStatusCode)
            {
                download.Status = "Unable To Retrieve Metadata";
                return;
            }

            while (attempts < 2)
            {
                try
                {
                    JObject jsonString = JObject.Parse(responseString);

                    if ((int)jsonString["duration"] / 60 > 10)
                    {
                        Console.WriteLine("Failed, download greater than 10 mins!");
                        download.Status = "Song >10 Minutes";
                        return;
                    }

                    await CreateCustomLevel(jsonString, download);
                    break;
                }
                catch
                {
                    attempts += 1;

                    Console.WriteLine("Failed to Create Custom Level!");
                    download.Status = "Unable To Create Level";
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        static async Task CreateCustomLevel(JObject responseData, Download download)
        {
            download.Status = "Generating Custom Level";

            string trackName = "Unknown";

            if (((string)responseData["track"]) != null)
            {
                trackName = (string)responseData["track"];
            }
            else if (((string)responseData["fulltitle"]) != null)
            {
                trackName = (string)responseData["fulltitle"];
            }

            string artistName = "Unknown";

            if (((string)responseData["artist"]) != null)
            {
                artistName = (string)responseData["artist"];
            }
            else if (((string)responseData["uploader"]) != null)
            {
                artistName = (string)responseData["uploader"];
            }
            
            var invalids = System.IO.Path.GetInvalidFileNameChars();

            trackName = String.Join("_", trackName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            artistName = String.Join("_", artistName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            Console.WriteLine("trackName: " + trackName);
            Console.WriteLine("artistName: " + artistName);

            download.Title = trackName;
            download.Artist = artistName;

            string fileName = "[BSD] " + trackName + " - " + artistName;

            if (!Properties.Settings.Default.overwriteExisting)
            {
                if (((!Properties.Settings.Default.automaticExtraction) && (File.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName + ".zip"))) || ((Properties.Settings.Default.automaticExtraction) && (Directory.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName))))
                {
                    download.Status = "Already Exists";
                    return;
                }
            }

            Console.WriteLine("download.Title: " + download.Title);
            Console.WriteLine("download.Artist : " + download.Artist);

            string boundary = "----WebKitFormBoundaryaA38RFcmCeKFPOms";
            var content = new MultipartFormDataContent(boundary);

            content.Add(new StringContent((string)responseData["webpage_url"]), "youtube_url");

            var imageContent = new ByteArrayContent((byte[])responseData["beatsage_thumbnail"]);
            imageContent.Headers.Remove("Content-Type");
            imageContent.Headers.Add("Content-Disposition", "form-data; name=\"cover_art\"; filename=\"cover\"");
            imageContent.Headers.Add("Content-Type", "image/jpeg");
            content.Add(imageContent);

            content.Add(new StringContent(trackName), "audio_metadata_title");
            content.Add(new StringContent(artistName), "audio_metadata_artist");
            content.Add(new StringContent(download.Difficulties), "difficulties");
            content.Add(new StringContent(download.GameModes), "modes");
            content.Add(new StringContent(download.SongEvents), "events");
            content.Add(new StringContent(download.Environment), "environment");
            content.Add(new StringContent(download.ModelVersion), "system_tag");

            var response = await httpClient.PostAsync("https://beatsage.com/beatsaber_custom_level_create", content, cts.Token);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);

            JObject jsonString = JObject.Parse(responseString);

            string levelID = (string)jsonString["id"];

            Console.WriteLine(levelID);

            await CheckDownload(levelID, trackName, artistName, download);
        }

        static async Task CreateCustomLevelFromFile(Download download)
        {
            download.Status = "Uploading File";

            TagLib.File tagFile = TagLib.File.Create(download.FilePath);

            string artistName = "Unknown";
            string trackName = "Unknown";
            byte[] imageData = null;

            var invalids = System.IO.Path.GetInvalidFileNameChars();

            if (tagFile.Tag.FirstPerformer != null)
            {
                artistName = String.Join("_", tagFile.Tag.FirstPerformer.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            if (tagFile.Tag.Title != null)
            {
                trackName = String.Join("_", tagFile.Tag.Title.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }
            else
            {
                trackName = System.IO.Path.GetFileNameWithoutExtension(download.FilePath);
            }

            if (tagFile.Tag.Pictures.Count() > 0)
            {
                if (tagFile.Tag.Pictures[0].Data.Data != null)
                {
                    imageData = tagFile.Tag.Pictures[0].Data.Data;
                }
            }


            download.Artist = artistName;
            download.Title = trackName;

            string fileName = "[BSD] " + trackName + " - " + artistName;

            if (!Properties.Settings.Default.overwriteExisting)
            {
                if (((!Properties.Settings.Default.automaticExtraction) && (File.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName + ".zip"))) || ((Properties.Settings.Default.automaticExtraction) && (Directory.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName))))
                {
                    download.Status = "Already Exists";
                    return;
                }
            }

            byte[] bytes = System.IO.File.ReadAllBytes(download.FilePath);

            string boundary = "----WebKitFormBoundaryaA38RFcmCeKFPOms";
            var content = new MultipartFormDataContent(boundary);            

            content.Add(new ByteArrayContent(bytes), "audio_file", download.FileName);

            if (imageData != null)
            {
                var imageContent = new ByteArrayContent(imageData);
                imageContent.Headers.Remove("Content-Type");
                imageContent.Headers.Add("Content-Disposition", "form-data; name=\"cover_art\"; filename=\"cover\"");
                imageContent.Headers.Add("Content-Type", "image/jpeg");
                content.Add(imageContent);
            }
            
            content.Add(new StringContent(trackName), "audio_metadata_title");
            content.Add(new StringContent(artistName), "audio_metadata_artist");
            content.Add(new StringContent(download.Difficulties), "difficulties");
            content.Add(new StringContent(download.GameModes), "modes");
            content.Add(new StringContent(download.SongEvents), "events");
            content.Add(new StringContent(download.Environment), "environment");
            content.Add(new StringContent(download.ModelVersion), "system_tag");

            var response = await httpClient.PostAsync("https://beatsage.com/beatsaber_custom_level_create", content, cts.Token);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);

            JObject jsonString = JObject.Parse(responseString);

            string levelID = (string)jsonString["id"];

            Console.WriteLine(levelID);

            await CheckDownload(levelID, trackName, artistName, download);
        }

        static async Task CheckDownload(string levelId, string trackName, string artistName, Download download)
        {
            download.Status = "Generating Custom Level";
            
            string url = "https://beatsage.com/beatsaber_custom_level_heartbeat/" + levelId;

            Console.WriteLine(url);

            string levelStatus = "PENDING";


            while (levelStatus == "PENDING")
            {
                try
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    Console.WriteLine(levelStatus);

                    System.Threading.Thread.Sleep(1000);

                    //POST the object to the specified URI 
                    var response = await httpClient.GetAsync(url, cts.Token);

                    //Read back the answer from server
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject jsonString = JObject.Parse(responseString);

                    levelStatus = (string)jsonString["status"];

                }
                catch
                {
                }

            }

            if (levelStatus == "DONE")
            {
                RetrieveDownload(levelId, trackName, artistName, download);
            }
        }

        static void RetrieveDownload(string levelId, string trackName, string artistName, Download download)
        {
            download.Status = "Downloading";

            string url = "https://beatsage.com/beatsaber_custom_level_download/" + levelId;

            Console.WriteLine(url);

            string fileName = "[BSD] " + trackName + " - " + artistName;

            WebClient client = new WebClient();
            Uri uri = new Uri(url);

            if (Properties.Settings.Default.outputDirectory == "")
            {
                Properties.Settings.Default.outputDirectory = @"Downloads";
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.automaticExtraction)
            {
                client.DownloadFile(uri, fileName + ".zip");

                download.Status = "Extracting";

                if (Directory.Exists(fileName))
                {
                    Directory.Delete(fileName);
                }
                
                if (Directory.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName))
                {
                    Directory.Delete(Properties.Settings.Default.outputDirectory + @"\" + fileName, true);
                }

                ZipFile.ExtractToDirectory(fileName + ".zip", Properties.Settings.Default.outputDirectory + @"\" + fileName);

                if (File.Exists(fileName + ".zip"))
                {
                    File.Delete(fileName + ".zip");
                }
            }
            else
            {
                if (File.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName + ".zip"))
                {
                    File.Delete(Properties.Settings.Default.outputDirectory + @"\" + fileName + ".zip");
                }

                client.DownloadFile(uri, Properties.Settings.Default.outputDirectory + @"\" + fileName + ".zip");
            }


            download.Status = "Completed";
        }

        public static List<string> RetrieveYouTubePlaylist(string playlistULR)
        {
            string cleanPlaylistURL = playlistULR.Replace("watch?v", "playlist?v").Replace("music.", "");

            string htmlContent = new WebClient().DownloadString(cleanPlaylistURL);

            List<string> urls = new List<string>();

            string searchString = "data-video-id=";

            int htmlPointerLocation = 1;

            while (htmlPointerLocation > 0)
            {

                htmlPointerLocation = htmlContent.IndexOf(searchString);

                if (htmlPointerLocation > 0)
                {
                    string temporaryURL = "";

                    int i = 0;

                    for (i = (htmlPointerLocation + searchString.Count() + 1); i < htmlContent.Count(); i++)
                    {
                        if (htmlContent[i].ToString() != "\"")
                        {
                            temporaryURL += htmlContent[i];
                        }
                        else
                        {
                            break;
                        }
                    }

                    htmlContent = htmlContent.Substring(i);

                    string newURL = "https://www.youtube.com/watch?v=" + temporaryURL;

                    bool alreadyExists = false;

                    foreach (string currentURL in urls)
                    {
                        if ((currentURL == newURL) || (newURL.Contains(currentURL)))
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (alreadyExists == false)
                    {
                        Console.WriteLine(newURL);
                        urls.Add(newURL);
                    }
                }
            }

            return urls;
        }
    }
}
