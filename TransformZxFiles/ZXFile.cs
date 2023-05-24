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
            this(string.Empty, string.Empty, string.Empty, string.Empty, FormatEnum.tap) 
        { }

        public ZXFile(string destiny, string filename, string header, string path, FormatEnum format)
        {
            Destiny = destiny;
            Filename = filename;
            Header = header;
            Path = path;
            Format = format;
        }

        public string Destiny { get; set; }

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
