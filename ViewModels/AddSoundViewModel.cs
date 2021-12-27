using FFMpegCore;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using YoutubeExplode;
using Caliburn.Micro;
using YoutubeExplode.Videos.Streams;
using static SoundboardWPF.ViewModels.ShellViewModel;

namespace SoundboardWPF.ViewModels
{
    class AddSoundViewModel : Screen
    {
        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _url = "";

        public string URL
        {
            get { return _url; }
            set { _url = value; }
        }

        private string _start = "";

        public string Start
        {
            get { return _start; }
            set { _start = value; }
        }

        private string _end = "";

        public string End
        {
            get { return _end; }
            set { _end = value; }
        }

        private string _progress = "";

        public string Progress
        {
            get { return _progress; }
            set { _progress = value;
                NotifyOfPropertyChange(() => Progress);
            }
        }

        private bool _clipCheck = false;

        public bool ClipCheck
        {
            get { return _clipCheck; }
            set { _clipCheck = value; }
        }

        private Visibility _fileSelect = Visibility.Hidden;

        public Visibility FileSelect
        {
            get { return _fileSelect; }
            set {
                _fileSelect = value;
                NotifyOfPropertyChange(() => FileSelect);
            }
        }

        private Visibility _youtubeSelect = Visibility.Hidden;

        public Visibility YoutubeSelect
        {
            get { return _youtubeSelect; }
            set { _youtubeSelect = value;
                NotifyOfPropertyChange(() => YoutubeSelect);
            }
        }

        private String filename;
        private double length;
        public void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileName.EndsWith(".mp3"))
                {
                    Mp3FileReader reader = new Mp3FileReader(dialog.FileName);
                    TimeSpan l = reader.TotalTime;
                    length = l.TotalSeconds;
                    filename = dialog.FileName;
                    FileSelect = Visibility.Visible;
                    YoutubeSelect = Visibility.Hidden;
                    Console.WriteLine(FileSelect);
                }
                else
                {
                    MessageBox.Show("Not MP3!");
                }
            }
        }

        public void OpenYoutube(object sender, RoutedEventArgs e)
        {
            FileSelect = Visibility.Hidden;
            YoutubeSelect = Visibility.Visible;
            Console.WriteLine(YoutubeSelect);
        }

        public void SaveSound(object sender, RoutedEventArgs e)
        {
            AddSoundToList(Name, Math.Round(length, 2).ToString(), filename);
            FileSelect = Visibility.Hidden;
            MessageBox.Show("Successfully added sound.");
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

        public async void SaveYoutubeSound(object sender, RoutedEventArgs e)
        {
            var youtube = new YoutubeClient();
            string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);

            var regRes = regexExtractId.Match(URL);
            if (regRes.Success)
            {
                Progress = "Fetching...";
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(regRes.Groups[1].Value);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                Progress = "Downloading...";
                string VidName = regRes.Groups[1].Value + "." + streamInfo.Container;
                string OutName = regRes.Groups[1].Value + ".mp3";
                await youtube.Videos.Streams.DownloadAsync(streamInfo, VidName);
                Progress = "Converting...";

                TimeSpan begin = new TimeSpan();
                TimeSpan end = new TimeSpan();

                if (ClipCheck == true)
                {
                    begin = TimeSpan.FromSeconds(Convert.ToInt32(TimeSpan.Parse(Start).TotalSeconds) / 60);
                    end = TimeSpan.FromSeconds(Convert.ToInt32(TimeSpan.Parse(End).TotalSeconds) / 60);
                }
                await FFMpegArguments.FromFileInput(VidName).OutputToFile(OutName).ProcessAsynchronously();

                if (ClipCheck == true)
                {
                    TrimMp3(OutName, regRes.Groups[1].Value + "E.mp3", begin, end);
                    File.Delete(OutName);
                    OutName = regRes.Groups[1].Value + "E.mp3";
                    length = end.Subtract(begin).TotalSeconds;
                }
                else
                {
                    Mp3FileReader reader = new Mp3FileReader(OutName);
                    TimeSpan l = reader.TotalTime;
                    length = l.TotalSeconds;
                }
                AddSoundToList(Name, Math.Round(length, 2).ToString(), OutName);
                File.Delete(VidName);
                Progress = "";
                YoutubeSelect = Visibility.Hidden;
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
