using System;
using System.ComponentModel;

namespace BeatSage_Downloader
{
    [Serializable]
    public class Download : INotifyPropertyChanged
    {
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public int Number { get; set; }

        public string Identifier { get; set; }

        public string YoutubeID { get; set; }
        private void OnYoutubeIDChanged()
        {
            if ((FileName == "") || (FileName == null))
            {
                Identifier = YoutubeID;
            }
        }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Status { get; set; }

        public string Difficulties { get; set; }

        public string GameModes { get; set; }

        public string SongEvents { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }

        private void OnFilenameChanged()
        {
            if ((YoutubeID == "") || (YoutubeID == null))
            {
                Identifier = FileName;
            }
        }

        public string Environment { get; set; }

        public string ModelVersion { get; set; }

        public bool IsAlive { get; set; }

    }
}
