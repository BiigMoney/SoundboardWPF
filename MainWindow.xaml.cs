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
using WMPLib;
using System.Xml;
using System.Globalization;

namespace SoundboardWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private enum Window
        {
            MySounds,
            AddSound,
            BrowseSound,
            Settings
        }

        public static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();

        private Window scene = Window.MySounds;
        public MainWindow()
        {
            InitializeComponent();
            Main.Content = new MySounds();
        }

        public static void PlaySound(string url)
        {
            player.URL = url;
            player.controls.play();
        }

        private void OpenMySounds(object sender, RoutedEventArgs e)
        {
            if(scene != Window.MySounds)
            {
                Main.Content = new MySounds();
                scene = Window.MySounds;
            }
        }

        private void OpenAddSound(object sender, RoutedEventArgs e)
        {
            if (scene != Window.AddSound)
            {
                Main.Content = new Add_Sound();
                scene = Window.AddSound;
            }
        }

        private void OpenBrowseSounds(object sender, RoutedEventArgs e)
        {
            if (scene != Window.BrowseSound)
            {
                Main.Content = new Browse_Sounds();
                scene = Window.BrowseSound;
            }
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            if (scene != Window.Settings)
            {
                Main.Content = new Settings();
                scene = Window.Settings;
            }
        }

    }
}
