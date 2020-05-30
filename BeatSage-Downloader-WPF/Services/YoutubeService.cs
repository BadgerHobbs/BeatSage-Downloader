using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace BeatSage_Downloader
{
    class YoutubeService
    {
        public static void ImportPlaylist(TextBox linksTextBox, TextBox playlistURLTextBox)
        {

            List<string> youtubeURLS = YoutubeService.RetrievePlaylist(playlistURLTextBox.Text);

            if (linksTextBox.Text == "Enter YouTube Links Here (Separate Lines)")
            {
                var converter = new System.Windows.Media.BrushConverter();
                linksTextBox.Text = "";
                linksTextBox.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
            else
            {
                linksTextBox.AppendText("\n");
            }

            foreach (string youtubeURL in youtubeURLS)
            {
                linksTextBox.AppendText(youtubeURL + "\n");
            }

        }

        public static List<string> RetrievePlaylist(string playlistULR)
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

                    int i;
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

        public async static Task CreateCustomLevelWithLocalMP3Download(string url, Download download, HttpClient httpClient, CancellationTokenSource cts)
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
    }
}
