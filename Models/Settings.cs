using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.IO;
using NAudio.Wave;
using IniParser.Exceptions;
using System.Windows;

namespace SoundboardWPF.Models
{
    class Settings
    {
        private FileIniDataParser parser = new FileIniDataParser();
        private IniData data { get; set; }
        public static int Volume;
        public static string SecondaryOutputDevice;
        public static int SecondaryAudioDeviceID;

        public static List<string> AudioDevices = new List<string>();
        public Settings()
        {
            AudioDevices.Add("None");
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                WaveOutCapabilities WOC = WaveOut.GetCapabilities(i);
                AudioDevices.Add(WOC.ProductName);
            }
            try
            {
                data = parser.ReadFile("config.ini");
                Volume = int.Parse(data["config"]["Volume"]);
                string device = data["config"]["SecondaryOutputDevice"];
                if (AudioDevices.Any(audio => audio == device))
                {
                    SecondaryOutputDevice = device;
                    SecondaryAudioDeviceID = AudioDevices.FindIndex(audio => audio == device) - 1;
                }
                else
                {
                    SecondaryOutputDevice = "None";
                    SecondaryAudioDeviceID = -1;
                }
            }
            catch (ParsingException ex)
            {
                WriteNewConfig(50, "None");
            }
            catch (Exception ex) {
                MessageBox.Show("Could not read config file.");
                Volume = 50;
                SecondaryAudioDeviceID = -1;
                SecondaryOutputDevice = "None";
            }
        }

        public static void WriteNewConfig(int volume, string device)
        {
            try
            {
                string lines = "[config]\nVolume=" + volume.ToString() + "\nSecondaryOutputDevice=" + device;
                File.WriteAllText("config.ini", lines);
                Volume = volume;
                if (AudioDevices.Any(audio => audio == device))
                {
                    SecondaryOutputDevice = device;
                    SecondaryAudioDeviceID = AudioDevices.FindIndex(audio => audio == device) - 1;
                } else
                {
                    SecondaryOutputDevice = "None";
                    SecondaryAudioDeviceID = -1;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show("Unable to access config file.");
            }
        }
    }
}
