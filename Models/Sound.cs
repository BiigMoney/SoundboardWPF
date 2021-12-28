using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundboardWPF.Models
{
    public class Sound
    {
        public string Name { get; set; }
        public string Length { get; set; }
        public string Path { get; set; }

        public Sound(string n, string l, string p)
        {
            Name = n;
            Length = l;
            Path = p;
        }
    }
}
