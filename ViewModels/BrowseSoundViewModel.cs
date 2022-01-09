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
        private readonly int soundsPerPage = 10;
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

        private int _currentPage = 1;

        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                _currentPage = value;
                if(_currentPage == 1)
                {
                    EnablePrev = false;
                }
            }
        }

        private bool _enablePrev = false;

        public bool EnablePrev
        {
            get { return _enablePrev; }
            set
            {
                _enablePrev = value;
                NotifyOfPropertyChange(() => EnablePrev);
            }
        }

        private bool _enableNext = false;

        public bool EnableNext
        {
            get { return _enableNext; }
            set
            {
                _enableNext = value;
                NotifyOfPropertyChange(() => EnableNext);
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

        public void GoPrev()
        {
            CurrentPage -= 1;
            SoundList = new BindableCollection<SoundVaultSound>(SoundVault.sounds.Skip(soundsPerPage*(CurrentPage-1)).Take(soundsPerPage));
            if(CurrentPage == 1)
            {
                EnablePrev = false;
            }
            if (SoundVault.sounds.Count > CurrentPage * soundsPerPage)
            {
                EnableNext = true;
            }
        }

        public void GoNext()
        {
            CurrentPage += 1;
            SoundList = new BindableCollection<SoundVaultSound>(SoundVault.sounds.Skip(soundsPerPage * (CurrentPage - 1)).Take(soundsPerPage));
            if(SoundVault.sounds.Count <= CurrentPage * soundsPerPage)
            {
                EnableNext = false;
            }
            if(CurrentPage == 2)
            {
                EnablePrev = true;
            }
        }

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
                if(SoundVault.sounds.Count > soundsPerPage * CurrentPage)
                {
                    EnableNext = true;
                }
                SoundList = new BindableCollection<SoundVaultSound>(SoundVault.sounds.Take(soundsPerPage));
                ShowTable = Visibility.Visible;
            }
            catch(MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                ShowError = Visibility.Visible;
            }
            finally
            {
                ShowLoading = Visibility.Hidden;
            }
        }
    }
}
