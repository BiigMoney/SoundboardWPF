using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SoundboardWPF.Models
{
    class MySounds
    {
        private static readonly XmlDocument doc = new XmlDocument();

        public static List<Sound> Sounds = new List<Sound>();

        private static WaveOut waveOut = new WaveOut();
        private static WaveOut waveOut2 = new WaveOut();
        private static string currentlyPlaying = "";

        private static Visibility _showEmpty = Visibility.Hidden;

        public static Visibility ShowEmpty
        {
            get { return _showEmpty; }
            set { _showEmpty = value; }
        }

        private static Visibility _showTable = Visibility.Hidden;

        public static Visibility ShowTable
        {
            get { return _showTable; }
            set { _showTable = value; }
        }

        public MySounds()
        {
            doc.PreserveWhitespace = true;
            doc.Load(@".\sounds.xml");
            XmlNodeList sounds = doc.SelectNodes("/sounds/sound");
            foreach (XmlNode item in sounds)
            {
                Sounds.Add(new Sound(item.Attributes["name"].Value, item.Attributes["length"].Value, item.Attributes["path"].Value));
            }
        }

        public static void AddSound(string name, string length, string path)
        {
            try
            {
                doc.Load(@".\sounds.xml");
                XmlNode sounds = doc.SelectSingleNode("/sounds");

                XmlElement NewSound = doc.CreateElement("sound");

                NewSound.SetAttribute("name", name);
                NewSound.SetAttribute("length", length);
                NewSound.SetAttribute("path", path);


                sounds.AppendChild(NewSound);

                doc.Save(@".\sounds.xml");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show("There was an error when saving the config file, your sound has not been added.");
                return;
            }
            Sounds.Add(new Sound(name, length, path));
        }

        public static void DeleteSound(string name)
        {
            Sound removed = Sounds.Find(snd => snd.Name == name);
            try
            {
                doc.Load(@".\sounds.xml");
                XmlNode node = doc.SelectSingleNode(String.Format("/sounds/sound[@name='{0}']", name));
                node.ParentNode.RemoveChild(node);
                doc.Save(@".\sounds.xml");
                Sounds.Remove(removed);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show("There was an error when saving the config file, your sound has not been deleted.");
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show("Would you like to delete the associated MP3 too?", "Delete Sound", MessageBoxButton.YesNo);
            if(messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    File.Delete(removed.Path);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show("Error deleting file.");
                }
            } 
        }

        public static void PlaySound(string path)
        {
            PlaybackState playing = waveOut.PlaybackState;
            if (playing == PlaybackState.Playing)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut2.Stop();
                waveOut2.Dispose();
            }
            if (playing == PlaybackState.Playing && currentlyPlaying == path)
            {
                return;
            }

            currentlyPlaying = path;

            Thread t1 = new Thread(() => SecondSound(path, -1));
            t1.IsBackground = true;
            t1.Start();
            if (Settings.SecondaryAudioDeviceID != -1)
            {
                Thread t2 = new Thread(() => SecondSound(path, Settings.SecondaryAudioDeviceID));
                t2.IsBackground = true;
                t2.Start();
            }

        }

        private static void SecondSound(string path, int device)
        {
            Mp3FileReader reader = new Mp3FileReader(path);
            if(device == -1)
            {
                waveOut = new WaveOut();
                waveOut.Volume = (float) Settings.Volume / 100;
                try
                {
                    waveOut.Init(reader);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(100);
                    }
                    waveOut.Dispose();
                    currentlyPlaying = "";
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show("Unable to play sound, has the file been deleted?");
                }
            } else
            {
                waveOut2 = new WaveOut();
                waveOut2.Volume = (float) Settings.Volume / 100;
                waveOut2.DeviceNumber = device;
                try
                {
                    waveOut2.Init(reader);
                    waveOut2.Play();
                    while (waveOut2.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(100);
                    }
                    waveOut2.Dispose();
                }
                catch (NAudio.MmException ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show("Unable to play sound on secondary audio device, the selected audio device may not be compatible.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
