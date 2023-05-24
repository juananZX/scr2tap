using System;
using System.Collections.Generic;
using System.IO;

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
        /// <returns>Contents of a tape file</returns>
        public static byte[] Bin2Tap(byte[] buffer, string header)
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
            // Param 1 (16384)
            data.Add(0x00);
            data.Add(0x40);
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
        /// <param name="stream">Array.</param>
        /// <param name="header">Header name.</param>
        /// <returns>Contents of a tape file</returns>
        public static byte[] Bin2Tap(Stream stream, string header)
        {
            byte[] buffer = new byte[stream.Length];

            stream.Read(buffer, 0, (int)stream.Length);

            return Bin2Tap(buffer, header);
        } // Bin2Tap
        #endregion
    } // ZxFileConverter
} // ZxFileConverter