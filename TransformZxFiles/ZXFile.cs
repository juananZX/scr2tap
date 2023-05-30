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
            this(filename: string.Empty, header: string.Empty, path: string.Empty, format: FormatEnum.tap)
        { }

        public ZXFile(string filename, string header, string path, FormatEnum format)
        {
            Filename = filename;
            Header = header;
            Path = path;
            Format = format;
        }

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
