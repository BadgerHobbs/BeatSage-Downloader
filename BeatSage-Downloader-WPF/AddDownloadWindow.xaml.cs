using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

            this.ErrorLabelRectangle.Visibility = Visibility.Hidden;
            this.ErrorDifficultyLabel.Visibility = Visibility.Hidden;
            this.ErrorModeLabel.Visibility = Visibility.Hidden;

            string previousDifficulties = Properties.Settings.Default.previousDifficulties;
            string previousGameModes = Properties.Settings.Default.previousGameModes;
            string previousSongEvents = Properties.Settings.Default.previousGameEvents;

            if (previousDifficulties.Contains("Normal"))
            {
                DifficultyNormalCheckBox.IsChecked = true;
            }

            if (previousDifficulties.Contains("Hard"))
            {
                DifficultyHardCheckBox.IsChecked = true;
            }

            if (previousDifficulties.Contains("Expert"))
            {
                DifficultyExpertCheckBox.IsChecked = true;
            }

            if (previousDifficulties.Contains("ExpertPlus"))
            {
                DifficultyExpertPlusCheckBox.IsChecked = true;
            }


            if (previousGameModes.Contains("Standard"))
            {
                GameModeStandardCheckBox.IsChecked = true;
            }

            if (previousGameModes.Contains("NoArrows"))
            {
                GameModeNoArrowsCheckBox.IsChecked = true;
            }

            if (previousGameModes.Contains("OneSaber"))
            {
                GameModeOneSaberCheckBox.IsChecked = true;
            }

            if (previousGameModes.Contains("90Degrees"))
            {
                GameMode90DegreesCheckBox.IsChecked = true;
            }

            if (previousGameModes.Contains("360Degrees"))
            {
                GameMode360DegreesCheckBox.IsChecked = true;
            }


            if (previousSongEvents.Contains("DotBlocks"))
            {
                SongEventDotBlocksCheckBox.IsChecked = true;
            }

            if (previousSongEvents.Contains("Bombs"))
            {
                SongEventsBombsCheckBox.IsChecked = true;
            }
            
        }

        public async void AddDownloads(object sender, RoutedEventArgs e)
        {
            DownloadManager downloadManager = MainWindow.downloadManager;

            string selectedDifficulties = "";
            string selectedGameModes = "";
            string selectedSongEvents = "";

            bool difficultySelected = false;
            bool gameModeSelected = false;

            if ((DifficultyNormalCheckBox.IsChecked == true) || (DifficultyHardCheckBox.IsChecked == true) || (DifficultyExpertPlusCheckBox.IsChecked == true) || (DifficultyExpertPlusCheckBox.IsChecked == true))
            {
                difficultySelected = true;
            }

            if ((GameModeStandardCheckBox.IsChecked == true) || (GameModeOneSaberCheckBox.IsChecked == true) || (GameModeNoArrowsCheckBox.IsChecked == true) || (GameMode90DegreesCheckBox.IsChecked == true) || (GameMode360DegreesCheckBox.IsChecked == true))
            {
                gameModeSelected = true;
            }

            if (difficultySelected == true)
            {
                if (DifficultyNormalCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "Normal,";
                }

                if (DifficultyHardCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "Hard,";
                }

                if (DifficultyExpertCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "Expert,";
                }

                if (DifficultyExpertPlusCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "ExpertPlus,";
                }

                if (selectedDifficulties[selectedDifficulties.Count() - 1] == ',')
                {
                    selectedDifficulties = selectedDifficulties.Remove(selectedDifficulties.Count() - 1);
                    
                    Properties.Settings.Default.previousDifficulties = selectedDifficulties;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                Console.WriteLine("Please Select at Least One Difficulty");

                ErrorDifficultyLabel.Visibility = Visibility.Visible;
                ErrorLabelRectangle.Visibility = Visibility.Visible;

                await Task.Delay(2000);

                ErrorDifficultyLabel.Visibility = Visibility.Hidden;
                ErrorLabelRectangle.Visibility = Visibility.Hidden;

                return;
            }

            if (gameModeSelected == true)
            {
                if (GameModeStandardCheckBox.IsChecked == true)
                {
                    selectedGameModes += "Standard,";
                }

                if (GameModeNoArrowsCheckBox.IsChecked == true)
                {
                    selectedGameModes += "NoArrows,";
                }

                if (GameModeOneSaberCheckBox.IsChecked == true)
                {
                    selectedGameModes += "OneSaber,";
                }

                if (GameMode90DegreesCheckBox.IsChecked == true)
                {
                    selectedGameModes += "90Degrees,";
                }

                if (GameMode360DegreesCheckBox.IsChecked == true)
                {
                    selectedGameModes += "360Degrees,";
                }

                if (selectedGameModes[selectedGameModes.Count() - 1] == ',')
                {
                    selectedGameModes = selectedGameModes.Remove(selectedGameModes.Count() - 1);

                    Properties.Settings.Default.previousGameModes = selectedGameModes;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                Console.WriteLine("Please Select at Least One Game Mode");

                ErrorLabelRectangle.Visibility = Visibility.Visible;
                ErrorModeLabel.Visibility = Visibility.Visible;

                await Task.Delay(2000);

                ErrorLabelRectangle.Visibility = Visibility.Hidden;
                ErrorModeLabel.Visibility = Visibility.Hidden;

                return;
            }

            if (SongEventsBombsCheckBox.IsChecked == true)
            {
                selectedSongEvents += "Bombs,";
            }

            if (SongEventDotBlocksCheckBox.IsChecked == true)
            {
                selectedSongEvents += "DotBlocks,";
            }

            if ((selectedSongEvents != "") && (selectedSongEvents[selectedSongEvents.Count() - 1] == ','))
            {
                selectedSongEvents = selectedSongEvents.Remove(selectedSongEvents.Count() - 1);

                Properties.Settings.Default.previousGameEvents = selectedSongEvents;
                Properties.Settings.Default.Save();
            }

            for (int i = 0; i < linksTextBox.LineCount; i++)
            {
                if (DownloadManager.downloads.Count >= 100)
                {
                    ErrorMaxDownloadsLabel.Visibility = Visibility.Visible;
                    ErrorLabelRectangle.Visibility = Visibility.Visible;

                    await Task.Delay(2000);

                    ErrorMaxDownloadsLabel.Visibility = Visibility.Hidden;
                    ErrorLabelRectangle.Visibility = Visibility.Hidden;
                    return;
                }

                if (linksTextBox.GetLineText(i).Replace(" ", "").Replace("\n","").Replace("\r","").Count() < 5)
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
                    Status = "Queued",
                    Difficulties = selectedDifficulties,
                    GameModes = selectedGameModes,
                    SongEvents = selectedSongEvents,
                });
            }

            this.Close();


        }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public async void ImportPlaylist(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> youtubeURLS = DownloadManager.RetrieveYouTubePlaylist(playlistURLTextBox.Text);

                foreach (string youtubeURL in youtubeURLS)
                {
                    linksTextBox.AppendText(youtubeURL + "\n");
                }
            }
            catch
            {
                ErrorPlaylistLabel.Visibility = Visibility.Visible;
                ErrorLabelRectangle.Visibility = Visibility.Visible;

                await Task.Delay(2000);

                ErrorPlaylistLabel.Visibility = Visibility.Hidden;
                ErrorLabelRectangle.Visibility = Visibility.Hidden;
                
            }

            
        }

        private void ErrorModeLabel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

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
}
