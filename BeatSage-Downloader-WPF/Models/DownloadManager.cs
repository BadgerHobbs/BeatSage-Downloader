using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BeatSage_Downloader
{
    [Serializable]
    public class DownloadManager : INotifyPropertyChanged
    {
        //Fields
        public static ObservableCollection<Download> downloads = new ObservableCollection<Download>();
        public static readonly HttpClient httpClient = new HttpClient();
        public static CancellationTokenSource cts;
        public static Label newUpdateAvailableLabel;

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        //Constructor
        public DownloadManager()
        {
            httpClient.DefaultRequestHeaders.Add("Host", "beatsage.com");
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BeatSage-Downloader/1.2.1");

            if (MainWindow.GetSavedDownloads() != null)
            {
                downloads = MainWindow.GetSavedDownloads();
            }

            Thread worker = new Thread(RunDownloads)
            {
                IsBackground = true
            };
            worker.SetApartmentState(System.Threading.ApartmentState.STA);
            worker.Start();

        }

        //Methods
        public async void RunDownloads()
        {
            Console.WriteLine("RunDownloads Started");

            while (true)
            {
                MainWindow.SaveDownloads();

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
                            if (Properties.Settings.Default.enableLocalYouTubeDownload)
                            {
                                await CustomLevelService.CreateWithLocalMP3Download(itemUrl, currentDownload, httpClient, cts);
                            }
                            else
                            {
                                await DownloadManager.RetrieveMetaData(itemUrl, currentDownload);
                            }
                        }
                        catch
                        {
                            currentDownload.Status = "Unable To Retrieve Metadata";

                        }

                        currentDownload.IsAlive = false;
                        cts.Dispose();

                    }
                    else if ((currentDownload.FilePath != "") && (currentDownload.FilePath != null))
                    {
                        try
                        {
                            await CustomLevelService.CreateFromFile(currentDownload, httpClient, cts);
                        }
                        catch
                        {
                            currentDownload.Status = "Unable To Create Level";
                        }

                        currentDownload.IsAlive = false;
                        cts.Dispose();
                    }

                }

                cts.Dispose();
                System.Threading.Thread.Sleep(1000);

            }
        }

        public static async Task CheckUpdateAvailable()
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Host", "api.github.com");
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; Rigor/1.0.0; http://rigor.com)");

            //POST the object to the specified URI 
            var response = await httpClient.GetAsync("https://api.github.com/repos/BadgerHobbs/BeatSage-Downloader/releases/latest");

            //Read back the answer from server
            var responseString = await response.Content.ReadAsStringAsync();

            JObject jsonString = JObject.Parse(responseString);


            if (response.IsSuccessStatusCode)
            {
                string latestVersionString = ((string)jsonString["tag_name"]).Replace("v", "");
                string currentVersionString = Properties.Settings.Default.currentVersion.Replace("v", "");

                var latestVersion = new Version(latestVersionString);
                var currentVersion = new Version(currentVersionString);

                var result = latestVersion.CompareTo(currentVersion);

                if (result > 0)
                {
                    Console.WriteLine("New Update Available");
                    MainWindow.updateAvailableLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    Console.WriteLine("Running Latest Version");
                    MainWindow.updateAvailableLabel.Visibility = Visibility.Hidden;

                }
            }
        }

        public void Add(Download download)
        {
            downloads.Add(download);
        }

        public static async Task CreateCustomLevelWithLocalMP3Download(string url, Download download)
        {
            await CustomLevelService.CreateWithLocalMP3Download(url, download, httpClient, cts);
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
                download.IsAlive = false;
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
                        download.IsAlive = false;
                        return;
                    }

                    await CustomLevelService.Create(jsonString, download, httpClient, cts);
                    break;
                }
                catch
                {
                    attempts += 1;

                    Console.WriteLine("Failed to Create Custom Level!");
                    download.Status = "Unable To Create Level";
                    download.IsAlive = false;
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        public static async Task CheckDownload(string levelId, string trackName, string artistName, Download download)
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
            download.IsAlive = false;
        }
    }
}
