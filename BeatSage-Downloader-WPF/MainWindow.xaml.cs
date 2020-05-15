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

namespace BeatSage_Downloader_WPF
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
        }

        public void OpenAddDownloadWindow(object sender, RoutedEventArgs e)
        {
            AddDownloadWindow addDownloadWindow = new AddDownloadWindow();
            addDownloadWindow.ShowDialog();
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseProperChanged([CallerMemberName] string caller = "")
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
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; Rigor/1.0.0; http://rigor.com)");

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
                Console.WriteLine("Checking for Downloads...");

                if (previousNumberOfDownloads != downloads.Count)
                {
                    for (int i = previousNumberOfDownloads; i < downloads.Count; i++)
                    {
                        // Download Item
                        string itemUrl = "https://www.youtube.com/watch?v=" + downloads[i].YoutubeID;

                        //CollectionViewSource.GetDefaultView(dataGrid.ItemsSource).Refresh();

                        await RetrieveMetaData(itemUrl, downloads[i]);

                        //CollectionViewSource.GetDefaultView(dataGrid.ItemsSource).Refresh();
                    }

                    previousNumberOfDownloads = downloads.Count;
                }
                
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
            download.Status = "Retrieving MetaData";

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

            while (attempts < 5)
            {
                try
                {
                    JObject jsonString = JObject.Parse(responseString);

                    await CreateCustomLevel(jsonString, download);
                    break;
                }
                catch
                {
                    attempts += 1;

                    System.Threading.Thread.Sleep(500);
                }
            }

            if (attempts >= 5)
            {
                Console.WriteLine("Failed to Create Custom Level!");

                download.Status = "Failed";
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
            content.Add(new StringContent("null"), "audio_file");
            content.Add(new StringContent((string)responseData["webpage_url"]), "youtube_url");
            content.Add(new StringContent(trackName), "audio_metadata_title");
            content.Add(new StringContent(artistName), "audio_metadata_artist");
            content.Add(new StringContent(download.Difficulties), "difficulties");
            content.Add(new StringContent(download.GameModes), "modes");
            content.Add(new StringContent(download.SongEvents), "events");
            content.Add(new StringContent("0"), "seed");

            var response = await httpClient.PostAsync("https://beatsage.com/beatsaber_custom_level_create", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);

            JObject jsonString = JObject.Parse(responseString);

            string levelID = (string)jsonString["id"];

            Console.WriteLine(levelID);

            await CheckDownload(levelID, trackName, artistName, download);
        }

        static async Task CheckDownload(string levelId, string trackName, string artistName, Download download)
        {
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
            download.Status = "Downloading";

            string url = "https://beatsage.com/beatsaber_custom_level_download/" + levelId;

            Console.WriteLine(url);

            string fileName = "[BSD] " + trackName + " - " + artistName;

            WebClient client = new WebClient();
            Uri uri = new Uri(url);
            client.DownloadFile(uri, fileName + ".zip");

            download.Status = "Extracting";

            ZipFile.ExtractToDirectory(fileName + ".zip", fileName);

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
