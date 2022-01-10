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
using SoundboardWPF.Models;

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

        public AddSoundViewModel()
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./", TemporaryFilesFolder = "/tmp" });
        }

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
        }

        public void SaveSound(object sender, RoutedEventArgs e)
        {
            if(MySounds.Sounds.Any(sound => sound.Name == Name))
            {
                MessageBox.Show("A sound with this name already exists.");
            } else
            {
                MySounds.AddSound(Name, Math.Round(length, 2).ToString(), filename);
                FileSelect = Visibility.Hidden;
                MessageBox.Show("Successfully added sound.");
            }
        }

        public async void SaveYoutubeSound(object sender, RoutedEventArgs e)
        {
            if (MySounds.Sounds.Any(sound => sound.Name == Name))
            {
                MessageBox.Show("A sound with this name already exists.");
                return;
            }

            string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
            TimeSpan begin = new TimeSpan();
            TimeSpan end = new TimeSpan();

            if (ClipCheck)
            {
                try
                {
                    begin = TimeSpan.FromSeconds(Convert.ToInt32(TimeSpan.Parse(Start).TotalSeconds) / 60);
                    end = TimeSpan.FromSeconds(Convert.ToInt32(TimeSpan.Parse(End).TotalSeconds) / 60);
                    if (begin > end)
                    {
                        throw new ArgumentOutOfRangeException("end", "end should be greater than begin");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show("The start and end times you have provided are invalid.");
                    return;
                }
            }

            var regRes = regexExtractId.Match(URL);
            if (regRes.Success)
            {
                Progress = "Fetching...";

                IStreamInfo streamInfo = null;
                YoutubeClient youtube = null;
                try
                {
                    youtube = new YoutubeClient();
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(regRes.Groups[1].Value);
                    streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                } catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Progress = "";
                    MessageBox.Show("Could not get youtube data.");
                    return;
                }
                Progress = "Downloading...";
                string rand = Guid.NewGuid().ToString("n").Substring(0, 8);
                Directory.CreateDirectory("./sounds");
                string VidName = "./sounds/" + rand + "." + streamInfo.Container;
                string OutName = "./sounds/" + rand + ".mp3";
                await youtube.Videos.Streams.DownloadAsync(streamInfo, VidName);
                Progress = "Converting...";

                try
                {
                    await FFMpegArguments.FromFileInput(VidName).OutputToFile(OutName).ProcessAsynchronously();
                }

                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    MessageBox.Show("Could not find FFmpeg executable, you should reinstall SoundVaultPro.");
                    Progress = "";
                    return;
                }

                try
                {

                    if (ClipCheck)
                    {
                        TrimMp3(OutName, rand + "E.mp3", begin, end);
                        File.Delete(OutName);
                        OutName = rand + "E.mp3";
                        length = end.Subtract(begin).TotalSeconds;
                    }
                    else
                    {
                        Mp3FileReader reader = new Mp3FileReader(OutName);
                        TimeSpan l = reader.TotalTime;
                        length = l.TotalSeconds;
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show("There was an error while converting the sound, the sound was not saved.");
                    Progress = "";
                    return;
                }
                
                MySounds.AddSound(Name, Math.Round(length, 2).ToString(), OutName);
                File.Delete(VidName);
                Progress = "";
                YoutubeSelect = Visibility.Hidden;
                MessageBox.Show("Successfully added sound.");
            }
            else
            {
                MessageBox.Show("The URL you have provided is invalid.");
            }

        }
        void TrimMp3(string inputPath, string outputPath, TimeSpan begin, TimeSpan end)
        {
            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin)
                    {
                        if (reader.CurrentTime <= end)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
        }
    }
}
