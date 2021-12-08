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
using System.Windows.Navigation;
using Microsoft.Win32;
using NAudio.Wave;
using System.Xml;
using MediaToolkit.Model;
using MediaToolkit;
using VideoLibrary;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Text.RegularExpressions;
using FFMpegCore;

namespace SoundboardWPF
{
    /// <summary>
    /// Interaction logic for Add_Sound.xaml
    /// </summary>
    public partial class Add_Sound : Page
    {
        private String filename;
        private double length;
        
        public Add_Sound()
        { 
            
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if(dialog.ShowDialog() == true)
            {
                if (dialog.FileName.EndsWith(".mp3")) {
                    Mp3FileReader reader = new Mp3FileReader(dialog.FileName);
                    TimeSpan l = reader.TotalTime;
                    length = l.TotalSeconds;
                    filename = dialog.FileName;
                    LocalDisplay();
                } else
                {
                    MessageBox.Show("Not MP3!");
                }
            }
        }

        private void Hide()
        {
            NameBox.Visibility = Visibility.Hidden;
            SaveButton.Visibility = Visibility.Hidden;
            URLName.Visibility = Visibility.Hidden;
            ClipRow.Visibility = Visibility.Hidden;
            SaveYoutubeButton.Visibility = Visibility.Hidden;
            ProgressText.Visibility = Visibility.Hidden;
        }

        private void LocalDisplay()
        {
            NameBox.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Visible;
        }

        private void SaveSound(object sender, RoutedEventArgs e)
        {
            AddSoundToList(NameBox.Text, Math.Round(length, 2).ToString(), filename);

            MessageBox.Show("Successfully added sound.");
            Hide();

        }

        public static void AddSoundToList(string name, string length, string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(@".\sounds.xml");
            XmlNode sounds = doc.SelectSingleNode("/sounds");

            XmlElement NewSound = doc.CreateElement("sound");

            NewSound.SetAttribute("name", name);
            NewSound.SetAttribute("length", length);
            NewSound.SetAttribute("path", path);


            sounds.AppendChild(NewSound);

            doc.Save(@".\sounds.xml");
        }

        private void OpenYoutube(object sender, RoutedEventArgs e)
        {
            URLName.Visibility = Visibility.Visible;
            ClipRow.Visibility = Visibility.Visible;
            SaveYoutubeButton.Visibility = Visibility.Visible;
        }

        private async void SaveYoutubeSound(object sender, RoutedEventArgs e)
        {
            var youtube = new YoutubeClient();
            string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);

            var regRes = regexExtractId.Match(URLBox.Text);
            if (regRes.Success)
            {
                ProgressText.Visibility = Visibility.Visible;
                ProgressText.Text = "Fetching...";
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(regRes.Groups[1].Value);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                ProgressText.Text = "Downloading...";
                string VidName = URLNameBox.Text + "." + streamInfo.Container;
                string OutName = URLNameBox.Text + ".mp3";
                await youtube.Videos.Streams.DownloadAsync(streamInfo,VidName);
                ProgressText.Text = "Converting...";

                TimeSpan begin = new TimeSpan(); 
                TimeSpan end = new TimeSpan();

                if (ClipCheck.IsChecked == true)
                {
                    begin = TimeSpan.FromSeconds(Convert.ToInt32(TimeSpan.Parse(StartAtBox.Text).TotalSeconds) / 60);
                    end = TimeSpan.FromSeconds(Convert.ToInt32(TimeSpan.Parse(EndAtBox.Text).TotalSeconds) / 60);
                }
                await FFMpegArguments.FromFileInput(VidName).OutputToFile(OutName).ProcessAsynchronously();

                if (ClipCheck.IsChecked == true)
                {
                    TrimMp3(OutName, URLNameBox.Text + "E.mp3", begin, end);
                    File.Delete(OutName);
                    OutName = URLNameBox.Text + "E.mp3";
                    length = end.Subtract(begin).TotalSeconds;
                }
                else
                {
                    Mp3FileReader reader = new Mp3FileReader(OutName);
                    TimeSpan l = reader.TotalTime;
                    length = l.TotalSeconds;
                }
                AddSoundToList(URLNameBox.Text, Math.Round(length, 2).ToString(), OutName);
                File.Delete(VidName);
                ProgressText.Visibility = Visibility.Hidden;
                MessageBox.Show("Success");
                return;
            }
            else
            {
                MessageBox.Show("Invalid URL");
                return;
            }

        }
        void TrimMp3(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
        }
    }
}
