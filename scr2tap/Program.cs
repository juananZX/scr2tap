using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.ExceptionServices;

namespace scr2tap
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("scr2tap: fatal error: no input files");
                return;
            }

            // 3, 83, 99, 114, 101, 101, 110, 32, 32, 32, 32 = nombre de la cabecera

            byte[] head = new byte[] { 19, 0, 0, 3, 83, 99, 114, 101, 101, 110, 32, 32, 32, 32, 0, 27, 0, 64, 0, 128, 244, 2, 27, 255 };

            foreach (string f in args)
            {
                
                byte[] screen;

                using (FileStream stream = File.Open(f, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                    {
                        screen = reader.ReadBytes((int)stream.Length);

                        reader.Close();
                    }

                    stream.Close();
                }

                byte checksum = 255;

                for (int i = 2; i < 6912; i++)
                {
                    checksum ^= screen[i];
                }

                string name = Path.GetFileName(f).Replace(Path.GetExtension(f), ".tap");

                using (FileStream stream = File.Create(name, 6937))
                {
                    using(BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8,false))
                    {
                        writer.Write(head);
                        writer.Write(screen);
                        writer.Write(checksum);

                        writer.Close();
                    }

                    stream.Close();
                }
            }
        }
    }
}
