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

        private static readonly HttpClient httpClient = new HttpClient();

        private static DataGrid dataGrid;

        public DownloadManager(DataGrid newDataGrid)
        {
            dataGrid = newDataGrid;

            httpClient.DefaultRequestHeaders.Add("Host", "beatsage.com");
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("User-Agent", $"BeatSage-Downloader/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");

            Thread worker = new Thread(RunDownloads);
            worker.IsBackground = true;
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();

            //RunDownloads();
            
        }

        public async void RunDownloads()
        {
            Console.WriteLine("RunDownloads Started");

            int previousNumberOfDownloads = downloads.Count;

            while (true)
            {
                Console.WriteLine("Checking for Downloads...");

                if (previousNumberOfDownloads != downloads.Count)
                {
                    for (int i = previousNumberOfDownloads; i < downloads.Count; i++)
                    {
                        if ((downloads[i].YoutubeID != "") && (downloads[i].YoutubeID != null))
                        {
                            string itemUrl = $"https://www.youtube.com/watch?v={downloads[i].YoutubeID}";
                            try { await RetrieveMetaData(itemUrl, downloads[i]); }
                            catch (HttpRequestException ex)
                            {
                                downloads[i].Status = "Failed";
                                if (ex.InnerException.InnerException.Message.Contains("forbidden by its access permissions")) // An attempt was made to access a socket in a way forbidden by its access permissions
                                {
                                    downloads[i].Status += " (Firewall)";
                                    break;
                                }
                            }
                            string itemUrl = "https://www.youtube.com/watch?v=" + downloads[i].YoutubeID;

                            try
                            {
                                await RetrieveMetaData(itemUrl, downloads[i]);
                            }
                            catch
                            {
                                downloads[i].Status = "Failed: Unable To Retrieve Metadata";
                                return;
                            }

                        }
                        else if ((downloads[i].FilePath != "") && (downloads[i].FilePath != null))
                        {
                            await CreateCustomLevelFromFile(downloads[i]);
                        }
                    }

                    previousNumberOfDownloads = downloads.Count;
                }

                Thread.Sleep(1000);
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
                download.Status = "Failed: Unable To Retrieve Metadata";
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
                        download.Status = "Failed: Song >10 Minutes";
                        return;
                    }

                    await CreateCustomLevel(jsonString, download);
                    break;
                }
                catch
                {
                    attempts += 1;

                    Thread.Sleep(500);
                    Console.WriteLine("Failed to Create Custom Level!");
                    download.Status = "Failed: Unable To Create Level";
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        static async Task CreateCustomLevel(JObject responseData, Download download)
        {
            download.Status = "Generating Custom Level";

            string trackName = "null";

            if (((string)responseData["track"]) != null)
            {
                trackName = (string)responseData["track"];
            }
            else if (((string)responseData["fulltitle"]) != null)
            {
                trackName = (string)responseData["fulltitle"];
            }

            string artistName = "null";

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

            Console.WriteLine("download.Title: " + download.Title);
            Console.WriteLine("download.Artist : " + download.Artist);

            string boundary = "----WebKitFormBoundaryaA38RFcmCeKFPOms";
            var content = new MultipartFormDataContent(boundary);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);

            //byte[] bytes = System.IO.File.ReadAllBytes("cover.jpg");
            //content.Add(new ByteArrayContent(bytes), "cover_art", "cover.jpg");

            //content.Add(new ByteArrayContent((byte[])responseData["beatsage_thumbnail"]), "cover_art", "cover.jpg");

            content.Add(new StringContent((string)responseData["webpage_url"]), "youtube_url");
            content.Add(new StringContent(trackName), "audio_metadata_title");
            content.Add(new StringContent(artistName), "audio_metadata_artist");
            content.Add(new StringContent(download.Difficulties), "difficulties");
            content.Add(new StringContent(download.GameModes), "modes");
            content.Add(new StringContent(download.SongEvents), "events");
            content.Add(new StringContent(download.Environment), "environment");
            content.Add(new StringContent(download.ModelVersion), "system_tag");

            var response = await httpClient.PostAsync("https://beatsage.com/beatsaber_custom_level_create", content);

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

            string artist = "";
            string title = "";

            var invalids = System.IO.Path.GetInvalidFileNameChars();

            if (tagFile.Tag.FirstPerformer != null)
            {
                artist = String.Join("_", tagFile.Tag.FirstPerformer.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            if (tagFile.Tag.Title != null)
            {
                title = String.Join("_", tagFile.Tag.Title.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }
            

            download.Artist = artist;
            download.Title = title;

            byte[] bytes = System.IO.File.ReadAllBytes(download.FilePath);

            //HttpContent fileStreamContent = new StreamContent(fileStream);

            //var stream = File.OpenRead("Keane - Somewhere Only We Know.mp3");
            //var streamContent = new StreamContent(stream);

            //HttpContent bytesContent = new ByteArrayContent(bytes);

            string boundary = "----WebKitFormBoundaryaA38RFcmCeKFPOms";
            var content = new MultipartFormDataContent(boundary);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
            //content.Add(streamContent, "audio_file");
            content.Add(new ByteArrayContent(bytes), "audio_file", download.FileName);
            content.Add(new StringContent(title), "audio_metadata_title");
            content.Add(new StringContent(artist), "audio_metadata_artist");
            content.Add(new StringContent(download.Difficulties), "difficulties");
            content.Add(new StringContent(download.GameModes), "modes");
            content.Add(new StringContent(download.SongEvents), "events");
            content.Add(new StringContent(download.Environment), "environment");
            content.Add(new StringContent(download.ModelVersion), "system_tag");

            var response = await httpClient.PostAsync("https://beatsage.com/beatsaber_custom_level_create", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);

            JObject jsonString = JObject.Parse(responseString);

            string levelID = (string)jsonString["id"];

            Console.WriteLine(levelID);

            await CheckDownload(levelID, title, artist, download);
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

                    Console.WriteLine(levelStatus);

                    System.Threading.Thread.Sleep(1000);

                    //POST the object to the specified URI 
                    var response = await httpClient.GetAsync(url);

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
            if (Properties.Settings.Default.outputDirectory == "")
            {
                Properties.Settings.Default.outputDirectory = @"Downloads";
                Properties.Settings.Default.Save();
            }
            string fileName = "[BSD] " + trackName + " - " + artistName;
            var outputDir = new DirectoryInfo(Properties.Settings.Default.outputDirectory).Combine(fileName);
            var outputZIP = outputDir.Parent.CombineFile(fileName + ".zip");


            download.Status = "Downloading";


            WebClient client = new WebClient();
            Uri uri = new Uri("https://beatsage.com/beatsaber_custom_level_download/" + levelId);
            Console.WriteLine(uri.OriginalString);


            if (Properties.Settings.Default.automaticExtraction)
            {
                if (Properties.Settings.Default.skipExisting && outputDir.Exists) { download.Status = "Already exists"; return; }

                client.DownloadFile(uri, fileName + ".zip");

                download.Status = "Extracting";

                if (outputDir.Exists) outputDir.Delete(true);

                ZipFile.ExtractToDirectory(fileName + ".zip", outputDir.FullName);

                if (outputZIP.Exists) outputZIP.Delete();
            }
            else
            {
                if (Properties.Settings.Default.skipExisting && outputZIP.Exists) { download.Status = "Already exists"; return; }

                if (outputZIP.Exists) outputZIP.Delete();

                client.DownloadFile(uri, outputZIP.FullName);
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
