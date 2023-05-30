using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxFilesConverter
{
    internal class ZXVideoByte
    {
        public int Column { get; set; }

        public ZXColorEnum Ink { get; set; }

        public int Line { get; set; }

        public ZXColorEnum Paper { get; set; }

        public byte Pixels { get; set; }

        public int Scanline { get; set; }
    }
}
