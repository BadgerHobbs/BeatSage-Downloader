using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BeatSage_Downloader
{
    class CustomLevelService
    {
        public async static Task CreateWithLocalMP3Download(string url, Download download, HttpClient httpClient, CancellationTokenSource cts)
        {
            download.Status = "Downloading File";

            var youtube = new YoutubeClient();

            // You can specify video ID or URL
            var video = await youtube.Videos.GetAsync(url);

            var duration = video.Duration; // 00:07:14

            if (video.Duration.Minutes + (video.Duration.Seconds / 60) > 10)
            {
                return;
            }

            string artistName = "Unknown";
            string trackName = "Unknown";

            var invalids = System.IO.Path.GetInvalidFileNameChars();

            if (video.Author != null)
            {
                artistName = String.Join("_", video.Author.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            if (video.Title != null)
            {
                trackName = String.Join("_", video.Title.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            }

            download.Artist = artistName;
            download.Title = trackName;

            string fileName = "[BSD] " + trackName + " - " + artistName;
            download.FilePath = fileName + ".mp3";

            if (!Properties.Settings.Default.overwriteExisting)
            {
                if (((!Properties.Settings.Default.automaticExtraction) && (File.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName + ".zip"))) || ((Properties.Settings.Default.automaticExtraction) && (Directory.Exists(Properties.Settings.Default.outputDirectory + @"\" + fileName))))
                {
                    download.Status = "Already Exists";
                    download.IsAlive = false;
                    return;
                }
            }

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

            var streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();

            if (streamInfo != null)
            {
                // Get the actual stream
                _ = await youtube.Videos.Streams.GetAsync(streamInfo);

                // Download the stream to file
                await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName + ".mp3");
            }

            string boundary = "----WebKitFormBoundaryaA38RFcmCeKFPOms";
            var content = new MultipartFormDataContent(boundary);

            byte[] bytes = System.IO.File.ReadAllBytes(download.FilePath);

            if (File.Exists(download.FilePath))
            {
                File.Delete(download.FilePath);
            }

            content.Add(new ByteArrayContent(bytes), "audio_file", download.FilePath);

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(new Uri("https://img.youtube.com/vi/" + video.Id + "/maxresdefault.jpg"), "cover.jpg");
                }
                catch
                {
                    try
                    {
                        client.DownloadFile(new Uri("https://img.youtube.com/vi/" + video.Id + "/sddefault.jpg"), "cover.jpg");
                    }
                    catch
                    {
                        try
                        {
                            client.DownloadFile(new Uri("https://img.youtube.com/vi/" + video.Id + "/hqdefault.jpg"), "cover.jpg");
                        }
                        catch
                        {

                        }
                    }
                }
            }

            byte[] imageData = System.IO.File.ReadAllBytes("cover.jpg");

            if (imageData != null)
            {
                var imageContent = new ByteArrayContent(imageData);
                imageContent.Headers.Remove("Content-Type");
                imageContent.Headers.Add("Content-Disposition", "form-data; name=\"cover_art\"; filename=\"cover\"");
                imageContent.Headers.Add("Content-Type", "image/jpeg");
                content.Add(imageContent);
            }

            if (File.Exists("cover.jpg"))
            {
                File.Delete("cover.jpg");
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

            await DownloadManager.CheckDownload(levelID, trackName, artistName, download);
        }

        public static async Task CreateFromFile(Download download, HttpClient httpClient, CancellationTokenSource cts)
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

            await DownloadManager.CheckDownload(levelID, trackName, artistName, download);
        }

        public static async Task Create(JObject responseData, Download download, HttpClient httpClient, CancellationTokenSource cts)
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

            await DownloadManager.CheckDownload(levelID, trackName, artistName, download);
        }
    }
}
