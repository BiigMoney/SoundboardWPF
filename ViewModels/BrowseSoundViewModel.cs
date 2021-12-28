using Caliburn.Micro;
using MySql.Data.MySqlClient;
using SoundboardWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SoundboardWPF.ViewModels
{
    class BrowseSoundViewModel : Screen
    {
        private BindableCollection<Sound> _soundList;
        public BindableCollection<Sound> SoundList { 
            get { return _soundList; } 
            set {
                _soundList = value;
                NotifyOfPropertyChange(() => SoundList);
            } 
        }

        private Visibility _showLoading = Visibility.Visible;
        public Visibility ShowLoading
        {
            get { return _showLoading; }
            set { _showLoading = value;
                NotifyOfPropertyChange(() => ShowLoading);
            }
        }

        private Visibility _showTable = Visibility.Hidden;
        public Visibility ShowTable
        {
            get { return _showTable; }
            set
            {
                _showTable = value;
                NotifyOfPropertyChange(() => ShowTable);
            }
        }

        private Visibility _showError = Visibility.Hidden;
        public Visibility ShowError
        {
            get { return _showError; }
            set
            {
                _showError = value;
                NotifyOfPropertyChange(() => ShowError);
            }
        }
        public BrowseSoundViewModel()
        {
            Thread GetData = new Thread(() => GetSounds());
            GetData.IsBackground = true;
            GetData.Start();
        }

        private void GetSounds()
        {
            try
            {
                SoundVault vault = new SoundVault();
                SoundList = new BindableCollection<Sound>(SoundVault.sounds);
                ShowTable = Visibility.Visible;
            }
            catch(MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
                ShowError = Visibility.Visible;
            }
            finally
            {
                ShowLoading = Visibility.Hidden;
            }
        }

    }
}
