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
using System.Windows.Input;

namespace SoundboardWPF.ViewModels
{
    
    public class MySoundsViewModel : Screen
    {
        private List<Sound> sounds = MySounds.Sounds;

        private BindableCollection<Sound> _soundList;

        public BindableCollection<Sound> SoundList { get { return _soundList; } set {
                _soundList = value;
                NotifyOfPropertyChange(() => SoundList);
            } }

        public ICommand PlaySoundCommand { get; private set; }
        public ICommand DeleteSoundCommand { get; private set; }

        private Visibility _showEmpty = Visibility.Hidden;

        public Visibility ShowEmpty
        {
            get { return _showEmpty; }
            set { _showEmpty = value;
                NotifyOfPropertyChange(() => ShowEmpty);
            }
        }

        private Visibility _showTable = Visibility.Hidden;

        public Visibility ShowTable
        {
            get { return _showTable; }
            set { _showTable = value;
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
            PlaySoundCommand = new RelayCommand(path => {
                MySounds.PlaySound(path.ToString());
                SoundList = new BindableCollection<Sound>(MySounds.Sounds);
                SetVisible();
            });
            DeleteSoundCommand = new RelayCommand(path => {
                MySounds.DeleteSound(path.ToString());
                SoundList = new BindableCollection<Sound>(MySounds.Sounds);
                SetVisible();
            });
            SoundList = new BindableCollection<Sound>(sounds);
        }

    }
}
