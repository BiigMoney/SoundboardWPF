using Caliburn.Micro;
using NAudio.Wave;
using SoundboardWPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SoundboardWPF.ViewModels
{
    class SettingsViewModel : Screen
    {
        private BindableCollection<string> _audioDevices = new BindableCollection<string>();
        public BindableCollection<string> AudioDevices
        {
            get { return _audioDevices; }
            set
            {
                _audioDevices = value;
                NotifyOfPropertyChange(() => AudioDevices);
            }
        }

        public int StartIndex;

        private int _volume = Settings.Volume;

        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if(SecondaryAudioDevice == Settings.SecondaryOutputDevice && Volume == Settings.Volume)
                {
                    EnableSave = false;
                } else
                {
                    EnableSave = true;
                }
                NotifyOfPropertyChange(() => Volume);
            }
        }

        private string _secondaryAudioDevice = Settings.SecondaryOutputDevice;

        public string SecondaryAudioDevice
        {
            get { return _secondaryAudioDevice; }
            set
            {
                _secondaryAudioDevice = value;
                if(SecondaryAudioDevice == Settings.SecondaryOutputDevice && Volume == Settings.Volume)
                {
                    EnableSave = false;
                } else
                {
                    EnableSave = true;
                }
                NotifyOfPropertyChange(() => SecondaryAudioDevice);
            }
        }

        private bool _enableSave = false;

        public bool EnableSave
        {
            get { return _enableSave; }
            set 
            {
                _enableSave = value;
                NotifyOfPropertyChange(() => EnableSave);
            }
        }

        public SettingsViewModel()
        {
            new Settings();
            foreach(string device in Settings.AudioDevices)
            {
                Console.WriteLine(device);
                AudioDevices.Add(device);
            }
        }

        public void SaveSettings()
        {
            foreach(string name in AudioDevices)
            {
                Console.WriteLine(name);
            }
        }
    }
}
