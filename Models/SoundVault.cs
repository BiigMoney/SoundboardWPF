using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SoundboardWPF.Models
{
    public class SoundVault
    {
        public static List<Sound> sounds;

        public SoundVault()
        {
            sounds = new List<Sound>();
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
                    sounds.Add(new Sound(name, length, soundFile));
                }
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
