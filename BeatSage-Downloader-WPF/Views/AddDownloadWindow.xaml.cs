using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BeatSage_Downloader
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
            this.ErrorLabel.Visibility = Visibility.Hidden;

            string previousDifficulties = Properties.Settings.Default.previousDifficulties;
            string previousGameModes = Properties.Settings.Default.previousGameModes;
            string previousSongEvents = Properties.Settings.Default.previousGameEvents;
            string previousEnvironment = Properties.Settings.Default.previousEnvironment;
            string previousModelVersion = Properties.Settings.Default.previousModelVersion;

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

            if (previousGameModes.Contains("90Degree"))
            {
                GameMode90DegreesCheckBox.IsChecked = true;
            }

            if (previousGameModes.Contains("360Degree"))
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

            if (previousSongEvents.Contains("Obstacles"))
            {
                SongEventsObstaclesCheckBox.IsChecked = true;
            }

            if (previousSongEvents.Contains("LightShow"))
            {
                SongEventsLightShowCheckBox.IsChecked = true;
            }

            if ((previousModelVersion != "") || (previousModelVersion != null))
            {
                foreach (ComboBoxItem cbi in ModelVersionComboBox.Items)
                {
                    if (cbi.Content.ToString() == previousModelVersion)
                    {
                        cbi.IsSelected = true;
                    }
                }
            }

            if ((previousEnvironment != "") || (previousEnvironment != null))
            {
                foreach (ComboBoxItem cbi in EnvironmentComboBox.Items)
                {
                    if (cbi.Content.ToString() == previousEnvironment)
                    {
                        cbi.IsSelected = true;
                    }
                }
            }

        }

        public void AddDownloads(object sender, RoutedEventArgs e)
        {
            if (DownloadManager.downloads.Count >= 100)
            {
                RaiseAnError("Maximum of 100 Downloads Reached");

                return;
            }

            DownloadManager downloadManager = MainWindow.downloadManager;

            string selectedDifficulties = "";
            string selectedGameModes = "";
            string selectedSongEvents = "";
            string selectedEnvironment = "";
            string selectedModelVersion = "";

            bool difficultySelected = false;
            bool gameModeSelected = false;

            if ((DifficultyNormalCheckBox.IsChecked == true) || (DifficultyHardCheckBox.IsChecked == true) || (DifficultyExpertCheckBox.IsChecked == true) || (DifficultyExpertPlusCheckBox.IsChecked == true))
            {
                difficultySelected = true;
            }

            if ((GameModeStandardCheckBox.IsChecked == true) || (GameModeOneSaberCheckBox.IsChecked == true) || (GameModeNoArrowsCheckBox.IsChecked == true) || (GameMode90DegreesCheckBox.IsChecked == true) || (GameMode360DegreesCheckBox.IsChecked == true))
            {
                gameModeSelected = true;
            }

            if (difficultySelected == true)
            {

                if (DifficultyExpertCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "Expert,";
                }

                if (DifficultyExpertPlusCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "ExpertPlus,";
                }

                if (DifficultyNormalCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "Normal,";
                }

                if (DifficultyHardCheckBox.IsChecked == true)
                {
                    selectedDifficulties += "Hard,";
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

                RaiseAnError("Please Select at Least One Difficulty");

                return;
            }

            if (gameModeSelected == true)
            {
                if (GameModeStandardCheckBox.IsChecked == true)
                {
                    selectedGameModes += "Standard,";
                }

                if (GameMode90DegreesCheckBox.IsChecked == true)
                {
                    selectedGameModes += "90Degree,";
                }

                if (GameModeNoArrowsCheckBox.IsChecked == true)
                {
                    selectedGameModes += "NoArrows,";
                }

                if (GameModeOneSaberCheckBox.IsChecked == true)
                {
                    selectedGameModes += "OneSaber,";
                }

                if (GameMode360DegreesCheckBox.IsChecked == true)
                {
                    selectedGameModes += "360Degree,";
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

                RaiseAnError("Please Select at Least One Game Mode");

                return;
            }

            if (SongEventDotBlocksCheckBox.IsChecked == true)
            {
                selectedSongEvents += "DotBlocks,";
            }

            if (SongEventsObstaclesCheckBox.IsChecked == true)
            {
                selectedSongEvents += "Obstacles,";
            }

            if (SongEventsBombsCheckBox.IsChecked == true)
            {
                selectedSongEvents += "Bombs,";
            }

            if (SongEventsLightShowCheckBox.IsChecked == true)
            {
                selectedSongEvents += "LightShow,";
            }

            if ((selectedSongEvents != "") && (selectedSongEvents[selectedSongEvents.Count() - 1] == ','))
            {
                selectedSongEvents = selectedSongEvents.Remove(selectedSongEvents.Count() - 1);
            }

            List<string> songEnvironments = new List<string>();

            foreach (ComboBoxItem comboBoxItem in EnvironmentComboBox.Items)
            {
                if (!comboBoxItem.Tag.ToString().Contains("Random"))
                {
                    songEnvironments.Add(comboBoxItem.Tag.ToString());
                    songEnvironments.Add(comboBoxItem.Tag.ToString());
                }
            }

            Properties.Settings.Default.previousEnvironment = EnvironmentComboBox.Text;
            Properties.Settings.Default.previousModelVersion = ModelVersionComboBox.Text;
            Properties.Settings.Default.previousGameEvents = selectedSongEvents;
            Properties.Settings.Default.Save();

            selectedEnvironment = GetSelectedEnvironment();

            Random random = new Random();

            if (selectedEnvironment == "Random")
            {
                int randomItemIndex = random.Next(songEnvironments.Count);
                selectedEnvironment = songEnvironments[randomItemIndex];
                Console.WriteLine("Random Environment: " + selectedEnvironment);
            }

            selectedModelVersion = GetSelectedModelVersion();

            loadingLabel.Visibility = Visibility.Visible;

            for (int i = 0; i < linksTextBox.LineCount; i++)
            {
                if (DownloadManager.downloads.Count >= 100)
                {

                    RaiseAnError("Maximum of 100 Downloads Reached");

                    loadingLabel.Visibility = Visibility.Hidden;
                    return;
                }

                if (linksTextBox.GetLineText(i).Replace(" ", "").Replace("\n", "").Replace("\r", "").Count() < 5)
                {
                    continue;
                }

                if (GetSelectedEnvironment() == "RandomPerSong")
                {
                    int randomItemIndex = random.Next(songEnvironments.Count);
                    selectedEnvironment = songEnvironments[randomItemIndex];
                    Console.WriteLine("Random Per Song Environment: " + selectedEnvironment);
                }

                if ((linksTextBox.GetLineText(i).Contains("youtube.com/watch?v=")) || (linksTextBox.GetLineText(i).Contains("https://youtu.be/")))
                {
                    string youtubeID = linksTextBox.GetLineText(i).Replace("https://youtu.be/", "").Replace("music.", "www.").Replace("https://www.youtube.com/watch?v=", "").TrimEnd('\r', '\n');

                    if (youtubeID.Contains("&"))
                    {
                        youtubeID = youtubeID.Substring(0, youtubeID.IndexOf("&"));
                    }

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
                        FilePath = "",
                        FileName = "",
                        Environment = selectedEnvironment,
                        ModelVersion = selectedModelVersion,
                        IsAlive = false
                    });
                }
                else if (linksTextBox.GetLineText(i).Contains(".mp3"))
                {
                    string filePath = linksTextBox.GetLineText(i).TrimEnd('\r', '\n');

                    Console.WriteLine("File Path: " + filePath);

                    MainWindow.downloadManager.Add(new Download()
                    {
                        Number = DownloadManager.downloads.Count + 1,
                        YoutubeID = "",
                        Title = "???",
                        Artist = "???",
                        Status = "Queued",
                        Difficulties = selectedDifficulties,
                        GameModes = selectedGameModes,
                        SongEvents = selectedSongEvents,
                        FilePath = filePath,
                        FileName = System.IO.Path.GetFileName(filePath),
                        Environment = selectedEnvironment,
                        ModelVersion = selectedModelVersion,
                        IsAlive = false
                    });
                }
            }

            loadingLabel.Visibility = Visibility.Hidden;
            this.Close();
        }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void ImportPlaylist(object sender, RoutedEventArgs e)
        {
            loadingLabel.Visibility = Visibility.Visible;
            try
            {
                YoutubeService.ImportPlaylist(linksTextBox, playlistURLTextBox);
            }
            catch
            {
                RaiseAnError("Please Enter a Valid YouTube Playlist URL");
            }
            playlistURLTextBox.Text = "";
            loadingLabel.Visibility = Visibility.Hidden;
        }

        public void PlaylistTextBoxFocusChange(object sender, RoutedEventArgs e)
        {
            var converter = new System.Windows.Media.BrushConverter();

            if (playlistURLTextBox.IsFocused == true)
            {
                if (playlistURLTextBox.Text == "Enter YouTube Playlist Link Here")
                {
                    playlistURLTextBox.Text = "";
                    playlistURLTextBox.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                }
            }
            else
            {
                if (playlistURLTextBox.Text == "")
                {
                    playlistURLTextBox.Text = "Enter YouTube Playlist Link Here";
                    playlistURLTextBox.Foreground = (Brush)converter.ConvertFromString("#FF959595");
                }
            }
        }

        public void LinksTextBoxFocusChange(object sender, RoutedEventArgs e)
        {
            var converter = new System.Windows.Media.BrushConverter();

            if (linksTextBox.IsFocused == true)
            {
                if (linksTextBox.Text == "Enter YouTube Links Here (Separate Lines)")
                {
                    linksTextBox.Text = "";
                    linksTextBox.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                }
            }
            else
            {
                if (linksTextBox.Text == "")
                {
                    linksTextBox.Text = "Enter YouTube Links Here (Separate Lines)";
                    linksTextBox.Foreground = (Brush)converter.ConvertFromString("#FF959595");
                }
            }
        }

        public async void RaiseAnError(string errorText)
        {
            ErrorLabel.Content = errorText;
            ErrorLabel.Visibility = Visibility.Visible;
            ErrorLabelRectangle.Visibility = Visibility.Visible;

            await Task.Delay(10000);

            ErrorLabel.Visibility = Visibility.Hidden;
            ErrorLabelRectangle.Visibility = Visibility.Hidden;
        }

        public void GetMP3Files(object sender, RoutedEventArgs e)
        {
            loadingLabel.Visibility = Visibility.Visible;

            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Filter = "MP3 Files (*.mp3)|*.mp3";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();


                if (result.ToString() == "OK" && !string.IsNullOrWhiteSpace(dialog.FileNames[0]))
                {
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

                    string[] files = dialog.FileNames;


                    foreach (string file in files)
                    {
                        var size = new FileInfo(file).Length / 1024 / 1024;

                        if (size > 30)
                        {
                            RaiseAnError("Please Select Files With A Maxium Size of 30MB");
                            continue;
                        }
                        else if (TagLib.File.Create(file).Properties.Duration.TotalMinutes > 10.0)
                        {
                            RaiseAnError("Please Select Files With A Maxium Length of 10 Minutes");
                            continue;
                        }

                        linksTextBox.AppendText(file + "\n");
                    }
                }

            }

            loadingLabel.Visibility = Visibility.Hidden;
        }

        public string GetSelectedEnvironment()
        {
            string selectedEnvironment = ((ComboBoxItem)EnvironmentComboBox.SelectedItem).Tag.ToString();

            return selectedEnvironment;
        }

        public string GetSelectedModelVersion()
        {
            string selectedModelVersion = ((ComboBoxItem)ModelVersionComboBox.SelectedItem).Tag.ToString();

            return selectedModelVersion;
        }

        public void ModelVersionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((ComboBoxItem)ModelVersionComboBox.SelectedItem).Tag.ToString() == "v1")
                {
                    GameModeOneSaberCheckBox.IsEnabled = false;
                    GameMode90DegreesCheckBox.IsEnabled = false;
                    SongEventsObstaclesCheckBox.IsEnabled = false;
                }
                else
                {
                    GameModeOneSaberCheckBox.IsEnabled = true;
                    GameMode90DegreesCheckBox.IsEnabled = true;
                    SongEventsObstaclesCheckBox.IsEnabled = true;
                }
            }
            catch { }

        }

    }
}
