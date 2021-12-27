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
        public List<Sound> sounds = GetSounds();
        public BindableCollection<Sound> SoundList { get; set; }
        private Visibility _showEmpty = Visibility.Hidden;
        public Visibility ShowEmpty
        {
            get { return _showEmpty; }
            set { _showEmpty = value;
                NotifyOfPropertyChange(() => ShowEmpty);
            }
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
            SoundList = new BindableCollection<Sound>(sounds);
        }
        public void PlaySound(object sender, RoutedEventArgs e)
        {
            Sound sound = (sender as Button).DataContext as Sound;
            //PlaySound(sound.Path);

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
