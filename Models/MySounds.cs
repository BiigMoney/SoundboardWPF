using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SoundboardWPF.Models
{
    class MySounds
    {
        private static XmlDocument doc = new XmlDocument();

        public static List<Sound> Sounds = new List<Sound>();

        private static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();

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
            SetVisible();
        }

        private static void SetVisible()
        {
            if (Sounds.Count > 0)
            {
                ShowTable = Visibility.Visible;
                ShowEmpty = Visibility.Hidden;
            }
            else
            {
                ShowEmpty = Visibility.Visible;
                ShowTable = Visibility.Hidden;
            }
        }

        public static void AddSound(string name, string length, string path)
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
            Sounds.Add(new Sound(name, length, path));
            SetVisible();
        }

        public static void DeleteSound(string path)
        {
            Sounds.RemoveAll(snd => snd.Path == path);
            XmlNode node = doc.SelectSingleNode(String.Format("/sounds/sound[@path='{0}']", path));
            node.ParentNode.RemoveChild(node);
            doc.Save(@".\sounds.xml");
            SetVisible();
        }

        public static void PlaySound(string path)
        {
            player.URL = path;
            player.controls.play();
        }
    }
}
