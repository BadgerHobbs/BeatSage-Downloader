using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace BeatSage_Downloader_WPF
{
    /// <summary>
    /// Interaction logic for AddDownloadWindow.xaml
    /// </summary>
    public partial class AddDownloadWindow : MetroWindow
    {
        public AddDownloadWindow()
        {
            InitializeComponent();
        }

        public void AddDownloads(object sender, RoutedEventArgs e)
        {
            DownloadManager downloadManager = MainWindow.downloadManager;
            
            for (int i = 0; i < linksTextBox.LineCount; i++)
            {
                if (linksTextBox.GetLineText(i).Replace(" ", "") == "")
                {
                    continue;
                }

                string youtubeID = linksTextBox.GetLineText(i).Replace("https://www.youtube.com/watch?v=", "").TrimEnd('\r', '\n');

                Console.WriteLine("Youtube ID: " + youtubeID);

                MainWindow.downloadManager.Add(new Download()
                {
                    Number = DownloadManager.downloads.Count + 1,
                    YoutubeID = youtubeID,
                    Title = "???",
                    Artist = "???",
                    Status = "Queued"
                });
            }

            this.Close();


        }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void ImportPlaylist(object sender, RoutedEventArgs e)
        {
            List<string> youtubeURLS = DownloadManager.RetrieveYouTubePlaylist(playlistURLTextBox.Text);

            foreach (string youtubeURL in youtubeURLS)
            {
                linksTextBox.AppendText(youtubeURL + "\n");
            }
        }
    }
}
