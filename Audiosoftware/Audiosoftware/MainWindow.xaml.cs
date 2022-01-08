using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WinForms = System.Windows.Forms;

namespace Audiosoftware
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public Song song = new Song();
        public Song Selectedsong { get; set; }
        public ObservableCollection<Song> Songs = new ObservableCollection<Song>();
        public MediaPlayer mediaPlayer = new MediaPlayer();
        public string dataListFilename = "datalist.csv";
        public OpenFileDialog ofd = new OpenFileDialog()
        {
            Filter = "MP3 files (*.mp3,*.flac)|*.mp3|All files (*.*)|*.*",
            Multiselect = true
        };


        public MainWindow()
        {
            InitializeComponent();
            //IF FILE EXIST, READ FILE
            if (File.Exists(dataListFilename))
            {
                ReadDataFromCSV();
            }
        }

        //This method will read the CSV file which will have value pairs like 
        //Example of a line will be like 
        //NAMEOFFILE;FULLPATH
        public void ReadDataFromCSV()
        {
            using (StreamReader sr = new StreamReader(dataListFilename))
            {
                while (!sr.EndOfStream)
                {
                    string[] array = sr.ReadLine().Split(';');
                    string name = array[0].ToString();
                    string path = array[1].ToString();

                    Songs.Add(new Song(array[0].ToString(), array[1].ToString()));
                }
            }
            listview.ItemsSource = Songs;
        }


        private void AddFButtonclick(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog folderdialog = new WinForms.FolderBrowserDialog();
            folderdialog.ShowNewFolderButton = false;
            folderdialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
            WinForms.DialogResult result = folderdialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {

                string spath = folderdialog.SelectedPath;
                string[] fileEntries = Directory.GetFiles(folderdialog.SelectedPath);

                foreach (string file in fileEntries)
                {
                    //Perform check if object already added
                    //Perform check if object already added
                    if (!Songs.Any(x => x.Name.Equals(System.IO.Path.GetFileNameWithoutExtension(file)))
                        && System.IO.Path.GetFileName(file).Contains(".mp3")
                        || System.IO.Path.GetFileName(file).Contains(".flac"))
                    {
                        Songs.Add(new Song(System.IO.Path.GetFileNameWithoutExtension(file), System.IO.Path.GetFullPath(file)));
                    }
                    else if (System.IO.Path.GetFileName(file).Contains(".mp3") || System.IO.Path.GetFileName(file).Contains(".flac"))
                    {
                        MessageBox.Show($"{System.IO.Path.GetFileNameWithoutExtension(file)} already in list", "Alert");
                    }
                }

                listview.ItemsSource = Songs;
            }

        }


        private void Openbuttonclick(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    //Perform check if object already added
                    if (!Songs.Any(x => x.Name.Equals(System.IO.Path.GetFileNameWithoutExtension(file)))
                        && System.IO.Path.GetFileName(file).Contains(".mp3")
                        || System.IO.Path.GetFileName(file).Contains(".flac"))
                    {
                        Songs.Add(new Song(System.IO.Path.GetFileNameWithoutExtension(file), System.IO.Path.GetFullPath(file)));
                    }
                    else if (System.IO.Path.GetFileName(file).Contains(".mp3") || System.IO.Path.GetFileName(file).Contains(".flac"))
                    {
                        MessageBox.Show($"{System.IO.Path.GetFileNameWithoutExtension(file)} already in list", "Alert");
                    }
                }

                listview.ItemsSource = Songs;
            }
        }

        private void Play_buttonclick(object sender, RoutedEventArgs e)
        {
            if (Selectedsong != null)
            {

                mediaPlayer.Play();
                mediaPlayer.Volume = sliderVol.Value;
                playingStatus.Content = "Currently Playing";
                playingTitle.Text = (Selectedsong.Name).ToString();


                //sngDuration.Text = mediaPlayer.NaturalDuration.ToString();

                mediaPlayer.MediaOpened += new EventHandler(me_MediaOpened);
                mediaPlayer.MediaEnded += new EventHandler(me_MediaEnded);


                DispatcherTimer playTimer;
                playTimer = new DispatcherTimer();
                playTimer.Interval = TimeSpan.FromMilliseconds(1000); //one second  
                playTimer.Tick += new EventHandler(playTimer_Tick);
                playTimer.Start();

            }
        }

        private void me_MediaEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("Song ended");
        }

        private void me_MediaOpened(object sender, EventArgs e)
        {
            Debug.WriteLine("Song started");
        }

        private void playTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                currentPosition.Text = String.Format(@"{0:hh\:mm\:ss}");
            }
            catch
            {
                currentPosition.Text = String.Format(@"{0:hh\:mm\:ss}",
                mediaPlayer.Position);
            }
        }

        private void Pause_buttonclick(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            playingStatus.Content = "Currently Paused";
        }

        private void Stop_buttonclick(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            playingStatus.Content = "Currently Stopped";
            playingTitle.Text = string.Empty;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Songs.Count > 0)
            {
                //Write current data in list to CSV file
                using (StreamWriter sw = new StreamWriter(dataListFilename))
                {
                    foreach (Song song in Songs)
                    {
                        sw.WriteLine($"{song.Name};{song.FilePath}");
                    }
                }
            }
            else
            {
                MessageBox.Show("The list is empty and therefore cannot be saved", "Empty List");
            }
        }

        //Still figuring this out as this is more tricky removing it from Song type.
        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            Songs.Remove((Song)listview.SelectedItem);
            using (StreamWriter sw = new StreamWriter(dataListFilename))
            {
                foreach (Song song in Songs)
                {
                    sw.WriteLine($"{song.Name};{song.FilePath}");
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = e.NewValue;
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview.SelectedItem != null)
            {
                playingTitle.Text = (Selectedsong.Name).ToString();
                mediaPlayer.Open(new Uri(Selectedsong.FilePath));
            }
        }
    }
}


