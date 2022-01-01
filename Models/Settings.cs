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
            }
            catch (ParsingException ex)
            {
                Console.WriteLine(ex.ToString());
                WriteNewConfig(50, "None");
                data = parser.ReadFile("config.ini");
            }
            catch (AggregateException ex) {
                Console.WriteLine(ex.ToString());
                data = new IniData();
                data["config"]["Volume"] = "50";
                data["config"]["SecondaryOutputDevice"] = "None";
            }
            finally
            {
                Volume = int.Parse(data["config"]["Volume"]);
                string device = data["config"]["SecondaryOutputDevice"];
                if(AudioDevices.Any(audio => audio == device))
                {
                    SecondaryOutputDevice = device;
                    SecondaryAudioDeviceID = AudioDevices.FindIndex(audio => audio == device) - 1;
                } else
                {
                    SecondaryOutputDevice = "None";
                }
            }
        }

        private void WriteNewConfig(int volume, string device)
        {
            string lines = "[config]\nVolume=" + volume.ToString() + "\nSecondaryOutputDevice="  + device;
            File.WriteAllText("config.ini", lines);
        }
    }
}
