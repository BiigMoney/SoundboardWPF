using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using SoundboardWPF.Models;

namespace SoundboardWPF.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {

        public ShellViewModel()
        {
            new MySounds();
            OpenMySounds();
        }

        public async void OpenMySounds()
        {
            await ActivateItemAsync(new MySoundsViewModel());
        }

        public async void OpenAddSound()
        {
            await ActivateItemAsync(new AddSoundViewModel());
        }

        public async void OpenBrowseSounds()
        {
            await ActivateItemAsync(new BrowseSoundViewModel());
        }

        public async void OpenSettings()
        {
            await ActivateItemAsync(new SettingsViewModel());
        }

    }
}
