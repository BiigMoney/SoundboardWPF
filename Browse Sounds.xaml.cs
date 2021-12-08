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
using System.Data;
using MySql.Data.MySqlClient;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using NAudio.Wave;
using System.Threading;
using System.Globalization;

namespace SoundboardWPF
{
    /// <summary>
    /// Interaction logic for Browse_Sounds.xaml
    /// </summary>
    public partial class Browse_Sounds : Page
    {
        static string bucketName = "my-bucket-of-sounds";
        static IAmazonS3 client = new AmazonS3Client(RegionEndpoint.USWest2);

        public Browse_Sounds()
        {
            InitializeComponent();
            Thread init = new Thread(() => InitData());
            init.IsBackground = true;
            init.Start();
        }

        private void InitData()
        {
            string connStr = "server=24.67.112.53;user=wpf;database=sounds;port=3306;password=notsecret";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from soundboard_sound", conn);
                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                Console.WriteLine(dt.Rows.Count.ToString());
                this.Dispatcher.Invoke((Action)(() =>
                {//this refer to form in WPF application 
                    SoundList.DataContext = dt;
                }));
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void PlaySound(object sender, RoutedEventArgs e)
        {
            string url = ((Button)sender).CommandParameter.ToString();
            Thread playThread = new Thread(() => PlayMp3FromUrl(url));
            playThread.IsBackground = true;
            playThread.Start();
        }

        private void PlayMp3FromUrl(string url)
        {
            using (Stream ms = new MemoryStream())
            {

                try
                {
                    GetObjectRequest request = new GetObjectRequest()
                    {
                        BucketName = bucketName,
                        Key = url
                    };
                    using (GetObjectResponse response = client.GetObject(request))
                    {
                        using (Stream stream = response.ResponseStream)
                        {
                            byte[] buffer = new byte[32768];
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                        }
                    }
                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.ErrorCode != null &&
                    (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                    ex.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Please check the provided AWS Credentials.");
                        Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                    }
                    else
                    {
                        Console.WriteLine("An error occurred with the message '{0}' when reading an object", ex.Message);
                    }
                }


                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        private void DownloadSound(object sender, RoutedEventArgs e)
        {
            string name = (((Button)sender).DataContext as DataRowView).Row["name"] as string;
            string url = ((Button)sender).CommandParameter.ToString();
            Thread playThread = new Thread(() => DownloadSoundFromURL(url, name));
            playThread.IsBackground = true;
            playThread.Start();
        }

        private void DownloadSoundFromURL(string url, string name)
        {
            string path = "./" + url;
            try
            {
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = url
                };
                using (GetObjectResponse response = client.GetObject(request))
                {
                    response.WriteResponseStreamToFile(path);
                    Mp3FileReader mp3 = new Mp3FileReader(path);
                    TimeSpan l = mp3.TotalTime;
                    double length = l.TotalSeconds;
                    Add_Sound.AddSoundToList(name, length.ToString(), path);
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != null &&
                (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                ex.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when reading an object", ex.Message);
                }
            }
        }
    }
}
