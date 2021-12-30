using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.IO;
using System.Threading.Tasks;

namespace SoundboardWPF.Models
{
    class Settings
    {
        private FileIniDataParser parser = new FileIniDataParser();
        private IniData data { get; set; }
        public static int Volume;
        public static string SecondaryOutputDevice;
        public Settings()
        {
            try
            {
                data = parser.ReadFile("config.ini");
            }
            catch (FileNotFoundException ex)
            {
                WriteNewConfig();
                data = parser.ReadFile("config.ini");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                data = new IniData();
                data["config"]["Volume"] = "50";
                data["config"]["SecondaryOutputDevice"] = "null";
            }
            finally
            {
                Volume = int.Parse(data["config"]["Volume"]);
                SecondaryOutputDevice = data["config"]["SecondaryOutputDevice"];
            }
        }

        private void WriteNewConfig()
        {
            string lines = "[config]\nVolume=50\nSecondaryOutputDevice=null";
            File.WriteAllText("config.ini", lines);
        }
    }
}
