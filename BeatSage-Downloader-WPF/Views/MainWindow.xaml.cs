using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BeatSage_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //Fields
        public static DownloadManager downloadManager;

        public static Label updateAvailableLabel;

        //Constructor
        public MainWindow()
        {
            InitializeComponent();
            downloadManager = new DownloadManager();

            dataGrid.ItemsSource = DownloadManager.downloads;
            updateAvailableLabel = newUpdateAvailableLabel;

            CheckUpdateAvailable();

            if (Directory.Exists("Downloads") == false)
            {
                Directory.CreateDirectory("Downloads");
            }
        }

        //Methods
        public async void CheckUpdateAvailable()
        {
            await DownloadManager.CheckUpdateAvailable();
        }

        public static void SaveDownloads()
        {
            List<Download> downloadsList = new List<Download>();

            foreach (Download download in DownloadManager.downloads)
            {
                downloadsList.Add(download);
            }

            if (downloadsList.Count > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, downloadsList);
                    ms.Position = 0;
                    byte[] buffer = new byte[(int)ms.Length];
                    ms.Read(buffer, 0, buffer.Length);
                    Properties.Settings.Default.savedDownloads = Convert.ToBase64String(buffer);
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                Properties.Settings.Default.savedDownloads = "";
                Properties.Settings.Default.Save();
            }
        }

        public static ObservableCollection<Download> GetSavedDownloads()
        {
            try
            {
                if ((Properties.Settings.Default.savedDownloads != null) && (Properties.Settings.Default.saveDownloadsQueue))
                {
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.savedDownloads)))
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        ObservableCollection<Download> downloads = new ObservableCollection<Download>();

                        foreach (Download download in (List<Download>)bf.Deserialize(ms))
                        {
                            if (download.IsAlive)
                            {
                                download.Status = "Queued";
                                download.IsAlive = false;
                            }
                            downloads.Add(download);
                        }

                        return downloads;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;

        }

        public void OpenAddDownloadWindow(object sender, RoutedEventArgs e)
        {
            AddDownloadWindow addDownloadWindow = new AddDownloadWindow
            {
                Owner = this
            };
            addDownloadWindow.ShowDialog();
            MainWindow.SaveDownloads();
        }

        public void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow
            {
                Owner = this
            };
            settingsWindow.ShowDialog();
            MainWindow.SaveDownloads();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            MainWindow.SaveDownloads();
            Properties.Settings.Default.Save();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public void MoveSelectedDownloadUp(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                selectedDownloads.Add(download);
            }

            selectedDownloads = selectedDownloads.OrderBy(o => DownloadManager.downloads.IndexOf(o)).ToList();

            foreach (Download download in selectedDownloads)
            {
                int selectedIndex = DownloadManager.downloads.IndexOf(download);

                if (selectedIndex - 1 >= 0)
                {
                    Download selectedDownloadItem = download;
                    Download downloadItemAbove = (Download)dataGrid.Items[selectedIndex - 1];
                    DownloadManager.downloads.Remove(selectedDownloadItem);
                    DownloadManager.downloads.Insert(selectedIndex - 1, selectedDownloadItem);
                    dataGrid.SelectedIndex = selectedIndex - 1;
                }
                else
                {
                    return;
                }
            }

            foreach (Download download in selectedDownloads)
            {
                dataGrid.SelectedItems.Add((Download)dataGrid.Items[DownloadManager.downloads.IndexOf(download)]);
            }
            MainWindow.SaveDownloads();

        }

        public void MoveSelectedDownloadDown(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                selectedDownloads.Add(download);
            }

            selectedDownloads = selectedDownloads.OrderBy(o => DownloadManager.downloads.IndexOf(o)).Reverse().ToList();

            foreach (Download download in selectedDownloads)
            {
                int selectedIndex = DownloadManager.downloads.IndexOf(download);

                if (selectedIndex + 1 < dataGrid.Items.Count)
                {
                    Download selectedDownloadItem = download;
                    Download downloadItemBelow = (Download)dataGrid.Items[selectedIndex + 1];
                    DownloadManager.downloads.Remove(selectedDownloadItem);
                    DownloadManager.downloads.Insert(selectedIndex + 1, selectedDownloadItem);
                    dataGrid.SelectedIndex = selectedIndex + 1;
                }
                else
                {
                    return;
                }
            }

            foreach (Download download in selectedDownloads)
            {
                dataGrid.SelectedItems.Add((Download)dataGrid.Items[DownloadManager.downloads.IndexOf(download)]);
            }
            MainWindow.SaveDownloads();
        }

        public void RetrySelectedDownload(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                selectedDownloads.Add(download);
            }

            foreach (Download download in selectedDownloads)
            {
                download.Status = "Queued";
            }

            foreach (Download download in selectedDownloads)
            {
                dataGrid.SelectedItems.Add((Download)dataGrid.Items[DownloadManager.downloads.IndexOf(download)]);
            }
            MainWindow.SaveDownloads();
        }

        public void RemoveSelectedDownload(object sender, RoutedEventArgs e)
        {
            List<Download> selectedDownloads = new List<Download>();

            foreach (Download download in dataGrid.SelectedItems)
            {
                if (download.IsAlive == true)
                {
                    DownloadManager.cts.Cancel();
                    DownloadManager.cts.Dispose();
                }

                selectedDownloads.Add(download);
            }

            foreach (Download download in selectedDownloads)
            {
                DownloadManager.downloads.Remove(download);
            }

            Thread.Sleep(250);
            MainWindow.SaveDownloads();
        }
    }
}
