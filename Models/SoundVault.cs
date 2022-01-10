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
        public static WaveOut waveOut = new WaveOut();
        public static WaveOut waveOut2 = new WaveOut();
        public static string currentlyPlaying = "";
        public SoundVault()
        {
            sounds = new List<SoundVaultSound>();
            string connStr = "server=24.67.112.53;user=wpf;database=sounds;port=3306;password=notsecret";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select s.id, name, length, soundFile, count(l.id) as likes from soundboard_sound s left join soundboard_sound_likes l on s.id = l.sound_id group by s.id  order by id desc limit 200", conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string likes = reader["likes"].ToString();
                    string name = reader["name"].ToString();
                    string length = reader["length"].ToString();
                    string soundFile = reader["soundFile"].ToString();
                    bool canDownload = true;
                    if (MySounds.Sounds.Any(sound => {
                        string path = sound.Path.Split('/').GetValue(sound.Path.Split('/').Length - 1).ToString();
                        if (path.Split('.')[0].Length > 8)
                        {
                            return path.Split('.')[0].Substring(8) + '.' + path.Split('.')[1] == soundFile;
                        }
                        return false;
                       }))
                    {
                        canDownload = false;
                    }
                    sounds.Add(new SoundVaultSound(name, length, soundFile, canDownload, likes));
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public static void PlaySound(string url)
        {
            PlaybackState playing = waveOut.PlaybackState;
            if (playing == PlaybackState.Playing)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut2.Stop();
                waveOut2.Dispose();
            }
            if (playing == PlaybackState.Playing && currentlyPlaying == url)
            {
                return;
            }
            currentlyPlaying = url;

            Thread t1 = new Thread(() => SecondSound(url, -1));
            t1.IsBackground = true;
            t1.Start();
            if (Settings.SecondaryAudioDeviceID != -1)
            {
                Thread t2 = new Thread(() => SecondSound(url, Settings.SecondaryAudioDeviceID));
                t2.IsBackground = true;
                t2.Start();
            }
        }

        private static void SecondSound(string url, int device)
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
                    currentlyPlaying = "";
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


                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    if(device == -1)
                    {
                        waveOut = new WaveOut();
                        waveOut.Volume = (float)Settings.Volume / 100;
                        try
                        {
                            waveOut.Init(blockAlignedStream);
                            waveOut.Play();
                            while (waveOut.PlaybackState == PlaybackState.Playing)
                            {
                                Thread.Sleep(100);
                            }
                            waveOut.Dispose();
                            currentlyPlaying = "";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            MessageBox.Show("Unable to play sound.");
                        }
                    } else
                    {
                        waveOut2 = new WaveOut();
                        waveOut2.Volume = (float)Settings.Volume / 100;
                        waveOut2.DeviceNumber = device;
                        try
                        {
                            waveOut2.Init(blockAlignedStream);
                            waveOut2.Play();
                            while (waveOut2.PlaybackState == PlaybackState.Playing)
                            {
                                Thread.Sleep(100);
                            }
                            waveOut2.Dispose();
                        } catch(NAudio.MmException ex)
                        {
                            Console.WriteLine(ex.ToString());
                            MessageBox.Show("Unable to play sound on secondary audio device, the selected audio device may not be compatible.");
                        } catch(Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                            
               
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
            string path = "./sounds/"+ Guid.NewGuid().ToString("n").Substring(0, 8) + url;
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
