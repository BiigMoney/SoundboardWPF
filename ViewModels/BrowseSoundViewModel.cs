using Amazon.S3;
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
using System.Windows.Input;

namespace SoundboardWPF.ViewModels
{
    class BrowseSoundViewModel : Screen
    {
        private BindableCollection<SoundVaultSound> _soundList;
        public BindableCollection<SoundVaultSound> SoundList { 
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

        public ICommand PlaySoundCommand { get; private set; }
        public ICommand DownloadSoundCommand { get; private set; }

        private void GetSounds()
        {
            PlaySoundCommand = new RelayCommand(path => SoundVault.PlaySound(path.ToString()));
            DownloadSoundCommand = new RelayCommand(path =>
            {
                string name = SoundVault.sounds.Find(sound => sound.Path == path.ToString()).Name;
                try
                {
                    SoundVault.DownloadSound(path.ToString(), name);
                    SoundVault.sounds.Find(sound => sound.Path == path.ToString()).CanDownload = false;
                    SoundList = new BindableCollection<SoundVaultSound>(SoundVault.sounds);
                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.ErrorCode != null &&
                    (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                    ex.ErrorCode.Equals("InvalidSecurity")))
                    {
                        MessageBox.Show("Please check the provided AWS Credentials.");
                    }
                    else
                    {
                        MessageBox.Show("An error occurred with the message '{0}' when reading an object", ex.Message);
                    }
                }
                
            });
            try
            {
                SoundVault vault = new SoundVault();
                SoundList = new BindableCollection<SoundVaultSound>(SoundVault.sounds);
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
