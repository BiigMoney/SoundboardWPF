using Caliburn.Micro;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SoundboardWPF.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        XmlDocument doc = new XmlDocument();

        public ArrayList MySounds = new ArrayList();

        private Visibility _showEmpty = Visibility.Hidden;

        public Visibility ShowEmpty
        {
            get { return _showEmpty; }
            set { _showEmpty = value; }
        }

        private Visibility _showTable = Visibility.Hidden;

        public Visibility ShowTable
        {
            get { return _showTable; }
            set { _showTable = value; }
        }

        public ShellViewModel()
        {
            doc.PreserveWhitespace = true;
            doc.Load(@".\sounds.xml");
            XmlNodeList sounds = doc.SelectNodes("/sounds/sound");
            foreach (XmlNode item in sounds)
            {
                MySounds.Add(new Sound(item.Attributes["name"].Value, item.Attributes["length"].Value, item.Attributes["path"].Value));
            }

            if(MySounds.Count > 0)
            {
                ShowTable = Visibility.Visible;
            } else
            {
                ShowEmpty = Visibility.Visible;
            }
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
