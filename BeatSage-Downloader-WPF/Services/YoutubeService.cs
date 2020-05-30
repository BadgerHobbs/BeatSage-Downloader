using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;

namespace BeatSage_Downloader
{
    class YoutubeService
    {
        public static void ImportPlaylist(TextBox linksTextBox, TextBox playlistURLTextBox)
        {

            List<string> youtubeURLS = RetrievePlaylist(playlistURLTextBox.Text);

            if (linksTextBox.Text == "Enter YouTube Links Here (Separate Lines)")
            {
                var converter = new BrushConverter();
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

    }
}
