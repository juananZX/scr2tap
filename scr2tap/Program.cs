// Especificación de como montar un .tap
// https://wiki.speccy.org/cursos/ensamblador/rutinas_save_load

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace scr2tap
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("scr2tap: fatal error: no input files");
                return 0x01;
            }


            //byte[] head = new byte[] { 0x13, 0x00, 0x00, 0x03, 0x53, 0x63, 0x72, 0x65, 0x65, 0x6E, 0x20, 0x20, 0x20, 0x20, 0x00, 0x1B, 0x00, 0x40, 0x00, 0x80, 0xF4, 0x02, 0x1B, 0xFF };

            foreach (string f in args)
            {
                if (File.Exists(f))
                {
                    byte[] screen;

                    // Abre el fichero de la pantalla y lo carga en screen
                    using (FileStream stream = File.Open(f, FileMode.Open))
                    {
                        using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
                        {
                            screen = reader.ReadBytes((int)stream.Length);

                            reader.Close();
                        }

                        stream.Close();
                    }

                    // Datos para el fichero de salida
                    string extension = Path.GetExtension(f);
                    string file = Path.GetFileName(f).Replace(extension, ".tap");
                    string head = file.Replace(".tap", "").PadRight(10, ' ').Substring(0, 10);

                    // Inicia bytes con longitud bloque cabecera (Little Endian) x13, 0x00
                    // Byte Flag 0x00 y Block Type 0x03
                    // Los dos primeros bytes no se incluyen en la longitud
                    List<byte> bytes = new List<byte>() { 0x13, 0x00, 0x00, 0x03 };

                    // Agrega nombre de cabecera
                    foreach (char c in head)
                    {
                        bytes.Add(Convert.ToByte(c));
                    }

                    // Agrega longitud de la pantalla en Litle Endian (6192)
                    bytes.Add(0x00);
                    bytes.Add(0x1b);
                    // Agrega parámetro 1 en Litle Endian (16384)
                    bytes.Add(0x00);
                    bytes.Add(0x40);
                    // Agrega parámetro 2 en Litle Endian (32768)
                    bytes.Add(0x00);
                    bytes.Add(0x80);

                    // Calcula checksum de cabecera (los dos primeros bytes no forman parte de la cabecera)
                    byte checksum = 0x00;

                    for (int i = 2; i < bytes.Count; i++)
                    {
                        checksum ^= bytes[i];
                    }

                    // Agrega chechsum y fin de la cabecera
                    bytes.Add(checksum);

                    // Agrega longitud de la pantalla en Little Endian 6192
                    // + 2 bytes (Byte Flag y Checksum)
                    bytes.Add(0x02);
                    bytes.Add(0x1b);

                    // Agrega Byte Flag del bloque de la pantalla
                    bytes.Add(0xFF);

                    // Agrega los bytes de la pantalla
                    bytes.AddRange(screen.ToList());

                    // Calcula checksum de la parte de pantalla
                    // Byte Flag
                    checksum = 0xff;

                    foreach (byte b in screen)
                    {
                        checksum ^= b;
                    }

                    // Agreaga checksum
                    bytes.Add(checksum);

                    // Crea el fichero de salida y lo graba
                    // 2 bytes con la longitud de la cabecera
                    // 19 de la cebecera
                    // 2 bytes con la longitud de la pantalla + 2
                    // 1 byte de Byte Flag
                    // 6912 bytes de la pantalla
                    // 1 byte de checksum
                    // En total 6937 bytes 
                    using (FileStream stream = File.Create(file, 6937))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false))
                        {
                            writer.Write(bytes.ToArray());

                            writer.Close();
                        }

                        stream.Close();
                    }
                }
                else
                {
                    return 0x02;
                }
            }

            return 0x00;
        }
    }
}
