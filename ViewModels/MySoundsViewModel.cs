using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using SoundboardWPF.ViewModels;
using static SoundboardWPF.ViewModels.ShellViewModel;

namespace SoundboardWPF.ViewModels
{
    public class MySoundsViewModel : Screen
    {
        private List<Sound> sounds = GetSounds();
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

        private void SetVisible()
        {
            if (sounds.Count > 0)
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

        public MySoundsViewModel()
        {
            SetVisible();
        }
        public void PlaySound(object sender, RoutedEventArgs e)
        {
            Sound sound = (sender as Button).DataContext as Sound;
            MainWindow.PlaySound(sound.Path);

        }
        public void DeleteSound(object sender, RoutedEventArgs e)
        {
            SetVisible();
            Sound sound = (sender as Button).DataContext as Sound;
            List<Sound> sounds = GetSounds();
            sounds.RemoveAll(snd => snd.Path == sound.Path);
            XmlNode node = doc.SelectSingleNode(String.Format("/sounds/sound[@path='{0}']", sound.Path));
            node.ParentNode.RemoveChild(node);
            doc.Save(@".\sounds.xml");
        }
    }
}
