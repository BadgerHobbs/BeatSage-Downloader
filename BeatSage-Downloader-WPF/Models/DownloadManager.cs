using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BeatSage_Downloader
{
    [Serializable]
    public class DownloadManager
    {
        //Fields
        public static ObservableCollection<Download> downloads = new ObservableCollection<Download>();
        public static readonly HttpClient httpClient = new HttpClient();
        public static CancellationTokenSource cts;
        public static Label newUpdateAvailableLabel;
        public static ObservableCollection<Download> Downloads { get; }
        
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
                                await YoutubeService.CreateCustomLevelWithLocalMP3Download(itemUrl, currentDownload, httpClient, cts);
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
                            await DownloadManager.CreateCustomLevelFromFile(currentDownload);
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
            await YoutubeService.CreateCustomLevelWithLocalMP3Download(url, download, httpClient, cts);
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

                    await CreateCustomLevel(jsonString, download);
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
                    download.IsAlive = false;
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

        public static async Task CreateCustomLevelFromFile(Download download)
        {
            download.Status = "Uploading File";

            TagLib.File tagFile = TagLib.File.Create(download.FilePath);

            string artistName = "Unknown";
            byte[] imageData = null;

            var invalids = System.IO.Path.GetInvalidFileNameChars();

            if (tagFile.Tag.FirstPerformer != null)
            {
                artistName = String.Join("_", tagFile.Tag.FirstPerformer.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            string trackName;
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
                    download.IsAlive = false;
                    return;
                }
            }

            byte[] bytes = File.ReadAllBytes(download.FilePath);

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
