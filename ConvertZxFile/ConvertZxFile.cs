using System.Text;

namespace ConvertZxFile
{
    /// <summary>
    /// Convert ZX Files to other formats.
    /// </summary>
    public static class ConvertFile
    {
        /// <summary>
        /// Converts a byte array to a byte array for tape file.
        /// </summary>
        /// <param name="source">Byte array.</param>
        /// <param name="header">Header name.</param>
        /// <returns>Byte array for tape file</returns>
        public static byte[] Bin2Tap(byte[] source, string header)
        {
            // Flag byte: 0x13
            // Block type: 0x00
            // Block lenght (little endian): 0x00, 0x03
            List<byte> bytes = new() { 0x13, 0x00, 0x00, 0x03 };
            
            // Add head
            foreach(char c in header.PadRight(10, ' ')[..10])
            {
                bytes.Add(Convert.ToByte(c));
            }

            // Fixed data (little endian)
            // Add source lenght
            bytes.Add(Convert.ToByte(source.Length % 0x0100));
            bytes.Add(Convert.ToByte((source.Length - (source.Length % 0x0100)) / 0x0100));
            // Add param 1 (16384)
            bytes.Add(0x40);
            bytes.Add(0x00);
            // Add param 2 (32768)
            bytes.Add(0x00);
            bytes.Add(0x80);

            // Checksum
            byte checksum = 0x00;
            for (int i = 2; i < bytes.Count; i++)
            {
                checksum ^= bytes[i];
            }
            bytes.Add(checksum);
            // End Head

            // Source data (little endian)
            // Add length + 2 bytes (Flag byte and Checksum)
            int length = source.Length + 0x02;
            bytes.Add(Convert.ToByte(length % 0x0100));
            bytes.Add(Convert.ToByte((length - (length % 0x0100)) / 0x0100));
            // Add flag byte
            bytes.Add(0xff);
            // Add source bytes
            bytes.AddRange(source);

            // Source cheksum
            checksum = 0xff;
            foreach(byte b in source)
            {
                checksum ^= b;
            }
            bytes.Add(checksum);

            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a byte array to a byte array for tape file.
        /// </summary>
        /// <param name="source">Byte array.</param>
        /// <param name="header">Header name.</param>
        /// <returns>Byte array for tape file</returns>
        public static byte[] Bin2Tap(Stream source, string header)
        {
            byte[] buffer;

            using (BinaryReader reader = new(source, Encoding.UTF8, false))
            {
                buffer = reader.ReadBytes((int)source.Length);
            }

            return Bin2Tap(buffer, header);
        }
    }
}