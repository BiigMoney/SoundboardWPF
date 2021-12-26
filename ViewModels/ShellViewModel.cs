using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SoundboardWPF.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        public static XmlDocument doc = new XmlDocument();

        private static List<Sound> Sounds = new List<Sound>();

        private ShellViewModel()
        {
            doc.PreserveWhitespace = true;
            doc.Load(@".\sounds.xml");
            XmlNodeList sounds = doc.SelectNodes("/sounds/sound");
            foreach (XmlNode item in sounds)
            {
                Sounds.Add(new Sound(item.Attributes["name"].Value, item.Attributes["length"].Value, item.Attributes["path"].Value));
            }
        }

        public static List<Sound> GetSounds()
        {
            return Sounds;
        }

        public async void OpenMySounds()
        {
            await ActivateItemAsync(new MySoundsViewModel());
        }

        public async void OpenAddSound()
        {
            await ActivateItemAsync(new AddSoundViewModel());
        }

        public async void OpenBrowseSounds()
        {
            await ActivateItemAsync(new BrowseSoundViewModel());
        }

        public async void OpenSettings()
        {
            await ActivateItemAsync(new SettingsViewModel());
        }

        public class Sound
        {
            public string Name { get; set; }
            public String Length { get; set; }
            public string Path { get; set; }

            public Sound(string n, String l, string p)
            {
                Name = n;
                Length = l;
                Path = p;
            }
        }

    }
}
