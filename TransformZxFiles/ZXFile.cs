using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxFilesConverter
{
    internal class ZXFile
    {
        string header = string.Empty;

        public ZXFile():
            this(filename: string.Empty, header: string.Empty, path: string.Empty, format: FormatEnum.tap, address: 0)
        { }

        public ZXFile(string filename, string header, string path, FormatEnum format, int address = 0)
        {
            Filename = filename;
            Header = header;
            Path = path;
            Format = format;
            Address = address;
        }

        public int Address { get; set; }

        public string Filename { get; set; }

        public string Header 
        { 
            get
            {
                return header;
            }

            set
            {
                header = value?.ZXCharacter();
            }
        }

        public string Path { get; set; }

        public FormatEnum Format { get; set; }
    }
}
