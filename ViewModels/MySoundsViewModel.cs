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
using SoundboardWPF.Models;

namespace SoundboardWPF.ViewModels
{
    public class MySoundsViewModel : Screen
    {
        public List<Sound> sounds = MySounds.Sounds;
        public BindableCollection<Sound> SoundList { get; set; }
        
        public Visibility ShowEmpty
        {
            get { return MySounds.ShowEmpty; }
            set { 
                MySounds.ShowEmpty = value;
                NotifyOfPropertyChange(() => ShowEmpty);
            }
        }

        public Visibility ShowTable
        {
            get { return MySounds.ShowTable; }
            set { 
                MySounds.ShowTable = value;
                NotifyOfPropertyChange(() => ShowTable);
            }
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
            Sound sound = (sender as Button).DataContext as Sound;
            MySounds.DeleteSound(sound.Path);
        }
    }
}
