using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ZxFilesConverter
{
    /// <summary>
    /// ZX Spectrum file conversions to other formats.
    /// </summary>
    public static class ZxFileConverter
    {
        #region Bin2Tap
        /// <summary>
        /// Converts an array to tape file content.
        /// </summary>
        /// <param name="buffer">Array.</param>
        /// <param name="header">Header name.</param>
        /// <param name="address">Start address.</param>
        /// <returns>Contents of a tape file</returns>
        public static byte[] Bin2Tap(byte[] buffer, string header, int address = 0)
        {
            //
            // HEADER
            //
            
            // Header length (little endian): 0x13, 0x00
            // Byte flag: 0x00
            // Block type: 0x00
            List<byte> data = new List<byte> { 0x13, 0x00, 0x00, 0x03 };

            // Header name
            foreach (char c in header.ZXCharacter().PadRight(10, ' ').Substring(0, 10))
            {
                data.Add(Convert.ToByte(c));
            }

            // Fixed data (little endian)
            // Length buffer
            data.Add((byte)(buffer.Length % 0x0100));
            data.Add((byte)((buffer.Length - (buffer.Length % 0x0100)) / 0x0100));
            // Param 1 (start address)
            data.Add((byte)(address % 0x0100));
            data.Add((byte)((address - (address % 0x0100)) / 0x0100));
            // Param 2 (32768)
            data.Add(0x00);
            data.Add(0x80);

            // Header checksum (excludes bytes 0 & 1)
            byte checksum = 0x00;
            for (int i = 2; i < data.Count; i++)
            {
                checksum ^= data[i];
            }
            data.Add(checksum);

            //
            // SOURCE
            //

            // Source length + 2 bytes (Flag & checksum)
            // little endian
            int length = buffer.Length + 0x02;
            data.Add((byte)(length % 0x0100));
            data.Add((byte)((length - (length % 0x0100)) / 0x0100));
            // Byte flag
            data.Add(0xff);
            // Source bytes
            data.AddRange(buffer);

            // Source checksum (includes byte flag)
            checksum = 0xff;
            foreach(byte b in buffer)
            {
                checksum ^= b;
            }
            data.Add(checksum);

            return data.ToArray();
        } // Bin2Tap

        /// <summary>
        /// Converts a stream to tape file content.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="header">Header name.</param>
        /// /// <param name="address">Start address.</param>
        /// <returns>Contents of a tape file</returns>
        public static byte[] Bin2Tap(Stream stream, string header, int address = 0)
        {
            byte[] buffer = new byte[stream.Length];

            stream.Read(buffer, 0, (int)stream.Length);

            return Bin2Tap(buffer, header, address);
        } // Bin2Tap
        #endregion

        #region Scr2Image
        /// <summary>
        /// Converts an array to bitmap.
        /// </summary>
        /// <param name="buffer">Array.</param>
        /// <returns>Contents of a tape file</returns>
        public static Bitmap Scr2Image(byte[] buffer)
        {
            if (buffer?.Length != 0x1b00)
            {
                throw new BadImageFormatException("Bad format");
            }

            byte[] pixels = buffer.ToList().GetRange(0x00, 0x1800).ToArray();
            byte[] attr = buffer.ToList().GetRange(0x1800, 0x0300).ToArray();
            int ptr = 0x4000;
            List<ZXVideoByte> bytes = new List<ZXVideoByte>(0x1800);

            foreach (byte b in pixels)
            {
                ZXVideoByte vb = GetCoords(ptr);
                GetAttr(ref vb, attr[(0x20 * vb.Line) + vb.Column]);
                vb.Pixels = b;
                bytes.Add(vb);
                ptr++;
            }

            Color color;
            Bitmap bmp = new Bitmap(0x0100, 0xc0);

            int x = 0, y = 0;

            List<ZXVideoByte> list = bytes.OrderBy(i => i.Column).OrderBy(i => i.Scanline).OrderBy(i => i.Line).ToList();

            foreach (ZXVideoByte vb in bytes.OrderBy(i => i.Column).OrderBy(i => i.Scanline).OrderBy(i => i.Line))
            {
                byte mask = 0x80;

                for (int i = 0; i < 8; i++)
                {
                    color = (vb.Pixels & mask) != 0 ? ZXColor.GetColor(vb.Ink) : ZXColor.GetColor(vb.Paper);
                    bmp.SetPixel(x, y, color);
                    mask >>= 1;
                    x++;
                }

                if (x == 0x0100)
                {
                    y++;
                    x = 0;
                }
            }

            Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
            
            bmp = bmp.Clone(rect, PixelFormat.Format8bppIndexed);

            return bmp;
        } // Scr2Image

        /// <summary>
        /// Converts a stream to bitmap.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>Contents of a tape file</returns>
        public static Bitmap Scr2Image(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];

            stream.Read(buffer, 0, (int)stream.Length);

            return Scr2Image(buffer);
        } // Scr2Image
        #endregion

        #region Private methods
        private static ZXVideoByte GetCoords(int ptr)
        {
            ZXVideoByte videoByte = new ZXVideoByte();

            videoByte.Column = ptr & 0x1f;
            videoByte.Line =  ((ptr & 0x1800) >> 8) + ((ptr & 0xe0) >> 5);
            videoByte.Scanline = (ptr & 0x0700) >> 8;
            
            return videoByte;
        }

        private static void GetAttr(ref ZXVideoByte vb, byte attr)
        {
            vb.Ink = (ZXColorEnum)(((attr & 0x40) >> 6) * 8 + (attr & 0x07));
            vb.Paper = (ZXColorEnum)(((attr & 0x40) >> 6) * 8 + ((attr & 0x38) >> 3));
        }
        #endregion
    } // ZxFileConverter
} // ZxFileConverter