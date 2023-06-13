using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ZxFilesConverter
{
    internal class ZXFile : INotifyPropertyChanged
    {
        int oldAddress = 0;
        string header = string.Empty;
        int blockType = 0;

        public ZXFile():
            this(filename: string.Empty, header: string.Empty, path: string.Empty, format: FormatEnum.tap, address: 0, blockType: 0)
        { }

        public ZXFile(string filename, string header, string path, FormatEnum format, int address = 0, int blockType = 0)
        {
            Filename = filename;
            Header = header;
            Path = path;
            Format = format;
            Address = address;
            oldAddress = address;
            BlockType = 0;
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public int BlockType 
        { 
            get
            {
                return blockType;
            }

            set
            {
                if (blockType != value)
                {
                    if (blockType != 2)
                    {
                        oldAddress = Address;
                    }

                    blockType = value;

                    if (blockType == 2)
                    {
                        Address = 0;
                    }
                    else
                    {
                        Address = oldAddress;
                    }

                    NotifyPropertyChanged("Address");
                }
            }
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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
