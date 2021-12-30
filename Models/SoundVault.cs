using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MySql.Data.MySqlClient;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SoundboardWPF.Models
{
    public class SoundVault
    {
        public static List<SoundVaultSound> sounds;
        static string bucketName = "my-bucket-of-sounds";
        static IAmazonS3 client = new AmazonS3Client(RegionEndpoint.USWest2);
        public static WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
        public static string currentlyPlaying = "";
        public SoundVault()
        {
            sounds = new List<SoundVaultSound>();
            string connStr = "server=24.67.112.53;user=wpf;database=sounds;port=3306;password=notsecret";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from soundboard_sound limit 10", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    string length = reader["length"].ToString();
                    string soundFile = reader["soundFile"].ToString();
                    bool canDownload = true;
                    if (MySounds.Sounds.Any(sound => sound.Path.Split('/').GetValue(sound.Path.Split('/').Length - 1).ToString() == soundFile))
                    {
                        canDownload = false;
                    }
                    sounds.Add(new SoundVaultSound(name, length, soundFile, canDownload));
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public static void PlaySound(string path)
        {
            Thread playThread = new Thread(() => PlayMp3FromUrl(path));
            playThread.IsBackground = true;
            playThread.Start();
        }

        private static void PlayMp3FromUrl(string url)
        {
            if(waveOut.PlaybackState == PlaybackState.Playing && currentlyPlaying == url)
            {
                waveOut.Stop();
                return;
            }
            if(waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
            }
            currentlyPlaying = url;
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
                    currentlyPlaying = "";
                    if (ex.ErrorCode != null &&
                    (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                    ex.ErrorCode.Equals("InvalidSecurity")))
                    {
                        MessageBox.Show("Please check the provided AWS Credentials.");                }
                    else
                    {
                        MessageBox.Show("An error occurred with the message '{0}' when reading an object", ex.Message);
                    }
                }


                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                        currentlyPlaying = "";
                    }
                }
            }
        }

        public static void DownloadSound(string path, string name)
        {
            Thread playThread = new Thread(() => DownloadSoundFromURL(path, name));
            playThread.IsBackground = true;
            playThread.Start();
        }

        private static void DownloadSoundFromURL(string url, string name)
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
                    MySounds.AddSound(name, length.ToString(), path);
                    MessageBox.Show("Successfully downloaded sound!");
                }
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
        }
    }
}
