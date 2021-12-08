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
using System.Windows.Shapes;
using System.Xml;
using WMPLib;

namespace SoundboardWPF
{
    /// <summary>
    /// Interaction logic for MySounds.xaml
    /// </summary>
    public partial class MySounds : Page
    {
        XmlDocument doc = new XmlDocument();

        public MySounds()
        {
            InitializeComponent();
            doc.PreserveWhitespace = true;
            doc.Load(@".\sounds.xml");
            XmlNodeList sounds = doc.SelectNodes("/sounds/sound");
            foreach (XmlNode item in sounds)
            {
                SoundList.Items.Add(new Sound(item.Attributes["name"].Value, item.Attributes["length"].Value, item.Attributes["path"].Value));
            }
        }
        public void PlaySound(object sender, RoutedEventArgs e)
        {
            Sound sound = (sender as Button).DataContext as Sound;
            MainWindow.PlaySound(sound.Path);

        }
        public void DeleteSound(object sender, RoutedEventArgs e)
        {
            Sound sound = (sender as Button).DataContext as Sound;
            SoundList.Items.Remove(sound);
            XmlNode node = doc.SelectSingleNode(String.Format("/sounds/sound[@path='{0}']", sound.Path));
            node.ParentNode.RemoveChild(node);
            doc.Save(@".\sounds.xml");
        }

    }
}
