using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxFilesConverter
{
    /// <summary>
    /// RLE compress.
    /// </summary>
    public static class RLECompression
    {
        public static byte[] Compress(byte[] data)
        {
            byte old = data[0];
            byte count = 0;
            List<(byte, byte)> tmp = new List<(byte, byte)>();
            List<byte> buffer = new List<byte>();

            foreach (byte b in data)
            {
                if (b == old)
                {
                    count += 1;
                }
                else
                {
                    tmp.Add(new ValueTuple<byte, byte>(count, old));
                    old = b;
                    count = 1;
                }
            }

            tmp.Add(new ValueTuple<byte, byte>(count, old));

            foreach (ValueTuple<byte, byte> t in tmp)
            {
                count = t.Item1;

                if (count == 1)
                {
                    // Solo un elemento
                    if (t.Item2 < 193)
                    {
                        // Si es menor que 193, no se comprime
                        buffer.Add(t.Item2);
                    }
                    else
                    {
                        // Es mayor que 192, se añade una repetición y el valor
                        buffer.Add(193);
                        buffer.Add(t.Item2);
                    }
                }
                else
                {

                    while (count > 63)
                    {
                        buffer.Add(Convert.ToByte(192 + 63));
                        buffer.Add(t.Item2);
                        count -= 63;
                    }

                    if (count < 0)
                    {
                        count += 63;
                    }

                    if (count > 0)
                    {

                        buffer.Add(Convert.ToByte(192 + count));
                        buffer.Add(t.Item2);
                    }
                }
            }

            return buffer.ToArray();
        }

        public static byte[] Compress(string path)
        {
            byte[] buffer = null;

            using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[f.Length];

                f.Read(buffer, 0, (int)f.Length);
            }

            return Compress(buffer);
        }

        public static byte[] Decompress(byte[] data)
        {
            List<byte> buffer = new List<byte>();

            for (int index = 0; index < data.Length; index++)
            {
                if (data[index] < 193)
                {
                    buffer.Add(data[index]);
                }
                else
                {
                    int count = data[index] & 63;
                    
                    index += 1;

                    for (int i = 0; i < count; i++)
                    {
                        buffer.Add(data[index]);
                    }
                }
            }

            return buffer.ToArray();
        }

        public static byte[] Decompress(string path)
        {
            byte[] buffer = null;

            using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[f.Length];

                f.Read(buffer, 0, (int)f.Length);
            }

            return Decompress(buffer);
        }
    } // RLECompress
} // ZxFilesConverter
