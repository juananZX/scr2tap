using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TransformZxFiles;
using ZxFilesConverter.Properties;

using Forms = System.Windows.Forms;

namespace ZxFilesConverter
{
    internal class ViewModel : INotifyPropertyChanged
    {
        #region Fields
        private CommandHandler blockTypeCommand;
        private CommandHandler clearCommand;
        private CommandHandler clearListCommand;
        private Cursor cursor = Cursors.Arrow;
        private CommandHandler deleteCommand;
        private CommandHandler findCommand;
        private CommandHandler outputCommand;
        private CommandHandler transformCommand;
        private CommandHandler transformScreenCommand;
        private CommandHandler transformRLECommand;
        private CommandHandler transformRLE2Command;
        private KeyValuePair<string, string> languageSelected;
        private string outputFolder;
        #endregion

        #region Constructors
        public ViewModel()
        {
            BinaryFiles = new ObservableCollection<ZXFile>();
            RLEFiles = new ObservableCollection<ZXFile>();
            RLE2Files = new ObservableCollection<ZXFile>();
            ScreenFiles = new ObservableCollection<ZXFile>();

            OutputFolder = string.Format("{0}", Settings.Default.outputbinary);
            Translator = new Translator();
            Translator.Code = Settings.Default.language;

            BlockTypes = new Dictionary<int, string>
            {
                {0, "Bytes" },
                {1, "Program" },
                {2, "Autoload" }
            };

            Languages = new Dictionary<string, string>
            {
                { "en", "English" },
                { "es", "Castellano"}
            };

            LanguageSelected = new KeyValuePair<string, string>(Translator.Code, Languages[Translator.Code]);
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public properties
        public bool CanTransform
        { 
            get
            {
                return BinaryFiles.Any();
            }
        }

        public bool CanTransformScreen
        {
            get
            {
                return ScreenFiles.Any();
            }
        }

        public bool CanTransformRLE
        {
            get
            {
                return RLEFiles.Any();
            }
        }

        public bool CanTransformRLE2
        {
            get
            {
                return RLE2Files.Any();
            }
        }

        public Cursor Cursor 
        { 
            get
            {
                return cursor;
            }

            set
            {
                cursor = value;

                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ZXFile> BinaryFiles { get; private set; }

        public Dictionary<int, string> BlockTypes { get; set; }

        public CommandHandler BlockTypeCommand
        {
            get
            {
                return blockTypeCommand ?? (blockTypeCommand = new CommandHandler(param => BlockType(param), () => BinaryFiles.Any()));
            }
        }

        public CommandHandler ClearCommand
        {
            get
            {
                return clearCommand ?? (clearCommand = new CommandHandler(param => Clear(param), () => true));
            }
        }

        public CommandHandler ClearListCommand
        {
            get
            {
                return clearListCommand ?? (clearListCommand = new CommandHandler(param => ClearList(param), () => true));
            }
        }

        public CommandHandler DeleteCommand
        {
            get
            {
                return deleteCommand ?? (deleteCommand = new CommandHandler(param => Remove(param), () => true));
            }
        }

        public CommandHandler FindCommand
        {
            get
            {
                return findCommand ?? (findCommand = new CommandHandler(param => Find(param), () => true));
            }
        }

        public CommandHandler OutputCommand
        {
            get
            {
                return outputCommand ?? (outputCommand = new CommandHandler(param => Output(param), () => true));
            }
        }

        public CommandHandler TransformCommand
        {
            get
            {
                return transformCommand ?? (transformCommand = new CommandHandler(param => Transform(param), () => CanTransform));
            }
        }

        public CommandHandler TransformScreenCommand
        {
            get
            {
                return transformScreenCommand ?? (transformScreenCommand = new CommandHandler(param => Transform(param), () => CanTransformScreen));
            }
        }

        public CommandHandler TransformRLECommand
        {
            get
            {
                return transformRLECommand ?? (transformRLECommand = new CommandHandler(param => Transform(param), () => CanTransformRLE));
            }
        }


        public CommandHandler TransformRLE2Command
        {
            get
            {
                return transformRLE2Command ?? (transformRLE2Command = new CommandHandler(param => Transform(param), () => CanTransformRLE2));
            }
        }

        public Dictionary<string, string> Languages { get; private set; }

        public KeyValuePair<string, string> LanguageSelected 
        { 
            get
            {
                return languageSelected;
            }

            set
            {
                languageSelected = value;

                Settings.Default.language = languageSelected.Key;
                Settings.Default.Save();

                Translator.Code = languageSelected.Key;

                Translator.Load();

                NotifyPropertyChanged("Translator");
            }
        }

        public string OutputFolder
        { 
            get
            {
                return outputFolder;
            }

            set
            {
                outputFolder = value;
                Settings.Default.outputbinary = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ZXFile> RLEFiles { get; private set; }

        public ObservableCollection<ZXFile> RLE2Files { get; private set; }

        public ObservableCollection<ZXFile> ScreenFiles { get; private set; }

        public int Format { get; set; }

        public Translator Translator { get; set; }
        #endregion

        #region Public methods
        public void AddFiles(string[] files, FormatEnum format)
        {
            if (format == FormatEnum.tap)
            {
                AddBinary(files);
            }
            else if (format == FormatEnum.rle)
            {
                AddRLE(files);
            }
            else
            {
                AddImage(files);
            }
        }
        #endregion

        #region Private methods
        private void AddBinary(string[] files)
        {
            foreach (string file in files)
            {
                bool isScr = Path.GetExtension(file).ToLower() == ".scr";
                string extension = Path.GetExtension(file);
                string filename = !string.IsNullOrWhiteSpace(extension) ? Path.GetFileName(file).Replace(extension, "") : Path.GetFileName(file);

                if (BinaryFiles.Any(i => i.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase))) continue;

                string header = filename.ZXCharacter().PadRight(10, ' ').Substring(0, 10).TrimEnd(' ');

                BinaryFiles.Add(new ZXFile(filename, header, file, FormatEnum.tap, isScr ? 0x4000 : 0x8000));
            }
        }

        private void AddImage(string[] files)
        {

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);
                string filename = !string.IsNullOrWhiteSpace(extension) ? Path.GetFileName(file).Replace(extension, "") : Path.GetFileName(file);

                if (ScreenFiles.Any(i => i.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase))) continue;

                ScreenFiles.Add(new ZXFile(filename, string.Empty, file, FormatEnum.tap));
            }
        }

        private void AddRLE(string[] files)
        {
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);
                string filename = !string.IsNullOrWhiteSpace(extension) ? Path.GetFileName(file).Replace(extension, "") : Path.GetFileName(file);

                if (RLEFiles.Any(i => i.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase))) continue;

                RLEFiles.Add(new ZXFile(filename, string.Empty, file, FormatEnum.rle));
            }
        }

        private void AddRLE2(string[] files)
        {
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);
                string filename = !string.IsNullOrWhiteSpace(extension) ? Path.GetFileName(file).Replace(extension, "") : Path.GetFileName(file);

                if (RLE2Files.Any(i => i.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase))) continue;

                RLE2Files.Add(new ZXFile(filename, string.Empty, file, FormatEnum.rle2));
            }
        }

        private void BlockType(object param)
        {
            int blockType = 0;

            int.TryParse(string.Format("{0}", param), out blockType);

            foreach(ZXFile f in BinaryFiles)
            {
                if (f.IsSelected) f.BlockType = blockType;
            }
        }

        private void Clear(object param)
        {
            if (param?.ToString() == "binary" || param?.ToString() == "rle")
            {
                OutputFolder = string.Empty;
            }
        }

        private void ClearList(object param)
        {
            if (param?.ToString() == "binary")
            {
                BinaryFiles.Clear();
            }
            else if (param?.ToString() == "rle")
            {
                RLEFiles.Clear();
            }
            else if (param?.ToString() == "rle2")
            {
                RLE2Files.Clear();
            }
            else
            {
                ScreenFiles.Clear();
            }
        }

        private void Find(object param)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (param?.ToString() == "binary")
            {
                dlg.Filter = "All (*.*)|*.*|ZX Spectrum Files (*.bin, *.scr)|*.bin;*.scr|Binary Files (*.bin)|*.bin|Screen Files (*.scr)|*.scr";
            }
            else if (param?.ToString() == "rle")
            {
                dlg.Filter = "All (*.*)|*.*";
            }
            else if (param?.ToString() == "rle2")
            {
                dlg.Filter = "All (*.*)|*.*";
            }
            else
            {
                dlg.Filter = "Screen Files (*.scr)|*.scr";
            }

            dlg.Multiselect = true;

            if (dlg.ShowDialog() == true)
            {
                if (param?.ToString() == "binary")
                {
                    AddBinary(dlg.FileNames);
                }
                if (param?.ToString() == "rle")
                {
                    AddRLE(dlg.FileNames);
                }
                if (param?.ToString() == "rle2")
                {
                    AddRLE2(dlg.FileNames);
                }
                else
                {
                    AddImage(dlg.FileNames);
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Output(object param)
        {
            // OpenFileDialog dlg = new OpenFileDialog();
            Forms.FolderBrowserDialog dlg = new Forms.FolderBrowserDialog();
            dlg.SelectedPath = Settings.Default.outputbinary;

            if (dlg.ShowDialog() == Forms.DialogResult.OK)
            {
                string folder = dlg.SelectedPath + "\\";

                OutputFolder = folder;
            }
        }

        private void Remove(object param)
        {
            if (param is ZXFile)
            {
                if (!string.IsNullOrEmpty(((ZXFile)param).Header))
                {
                    BinaryFiles.Remove((ZXFile)param);
                }
                else if (((ZXFile)param).Format == FormatEnum.rle)
                {
                    RLEFiles.Remove((ZXFile)param);
                }
                else if (((ZXFile)param).Format == FormatEnum.rle2)
                {
                    RLE2Files.Remove((ZXFile)param);
                }
                else
                {
                    ScreenFiles.Remove((ZXFile)param);
                }
            }
        }

        private void Transform(object param)
        {
            string currentFile = null;

            try
            {
                Cursor = Cursors.Wait;

                if (param?.ToString() == "binary")
                {
                    foreach (ZXFile f in BinaryFiles)
                    {
                        currentFile = f.Filename;

                        byte[] buffer;

                        using (FileStream r = new FileStream(f.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            buffer = new byte[r.Length];
                            r.Read(buffer, 0, buffer.Length);
                            r.Close();
                        }

                        buffer = ZxFileConverter.Bin2Tap(buffer, f.Header, f.Address, f.BlockType);
                        string oldExtension = Path.GetExtension(f.Path);
                        string output = !string.IsNullOrWhiteSpace(oldExtension) ? f.Path.Replace(Path.GetExtension(f.Path), ".tap") : f.Path + ".tap";

                        if (!string.IsNullOrWhiteSpace(OutputFolder))
                        {
                            output = string.Format("{0}{1}.tap", OutputFolder, f.Filename);
                        }

                        using (FileStream w = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                        {
                            w.Write(buffer, 0, buffer.Length);
                            w.Close();
                        }
                    }
                }
                else if (param?.ToString() == "unzip")
                {
                    foreach (ZXFile f in RLEFiles)
                    {
                        currentFile = f.Filename;

                        byte[] buffer = RLECompression.Decompress(f.Path);
                        string oldExtension = Path.GetExtension(f.Path);
                        string output = !string.IsNullOrWhiteSpace(oldExtension) ? f.Path.Replace(Path.GetExtension(f.Path), ".rlu") : f.Path + ".rlu";

                        if (!string.IsNullOrWhiteSpace(OutputFolder))
                        {
                            output = string.Format("{0}{1}.rlu", OutputFolder, f.Filename);
                        }

                        using (FileStream w = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                        {
                            w.Write(buffer, 0, buffer.Length);
                            w.Close();
                        }
                    }
                }
                else if (param?.ToString() == "unzip2")
                {
                    foreach (ZXFile f in RLEFiles)
                    {
                        currentFile = f.Filename;

                        byte[] buffer = RLECompression.Decompress2(f.Path);
                        string oldExtension = Path.GetExtension(f.Path);
                        string output = !string.IsNullOrWhiteSpace(oldExtension) ? f.Path.Replace(Path.GetExtension(f.Path), ".r2u") : f.Path + ".r2u";

                        if (!string.IsNullOrWhiteSpace(OutputFolder))
                        {
                            output = string.Format("{0}{1}.r2u", OutputFolder, f.Filename);
                        }

                        using (FileStream w = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                        {
                            w.Write(buffer, 0, buffer.Length);
                            w.Close();
                        }
                    }
                }
                else if (param?.ToString() == "zip")
                {
                    foreach (ZXFile f in RLEFiles)
                    {
                        currentFile = f.Filename;

                        byte[] buffer = RLECompression.Compress(f.Path);
                        string oldExtension = Path.GetExtension(f.Path);
                        string output = !string.IsNullOrWhiteSpace(oldExtension) ? f.Path.Replace(Path.GetExtension(f.Path), ".rlz") : f.Path + ".rlz";

                        if (!string.IsNullOrWhiteSpace(OutputFolder))
                        {
                            output = string.Format("{0}{1}.rlz", OutputFolder, f.Filename);
                        }

                        using (FileStream w = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                        {
                            w.Write(buffer, 0, buffer.Length);
                            w.Close();
                        }
                    }
                }
                else if (param?.ToString() == "zip2")
                {
                    foreach (ZXFile f in RLE2Files)
                    {
                        currentFile = f.Filename;

                        byte[] buffer = RLECompression.Compress2(f.Path);
                        string oldExtension = Path.GetExtension(f.Path);
                        string output = !string.IsNullOrWhiteSpace(oldExtension) ? f.Path.Replace(Path.GetExtension(f.Path), ".r2z") : f.Path + ".r2z";

                        if (!string.IsNullOrWhiteSpace(OutputFolder))
                        {
                            output = string.Format("{0}{1}.r2z", OutputFolder, f.Filename);
                        }

                        using (FileStream w = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                        {
                            w.Write(buffer, 0, buffer.Length);
                            w.Close();
                        }
                    }
                }
                else
                {
                    foreach (ZXFile f in ScreenFiles)
                    {
                        currentFile = f.Filename;

                        byte[] buffer;

                        using (FileStream r = new FileStream(f.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            buffer = new byte[r.Length];
                            r.Read(buffer, 0, buffer.Length);
                            r.Close();
                        }

                        using (Bitmap bmp = ZxFileConverter.Scr2Image(buffer))
                        {
                            string mime;
                            string extension;

                            switch ((FormatEnum)Format)
                            {
                                case FormatEnum.bmp:
                                    mime = "image/bmp";
                                    extension = ".bmp";
                                    break;
                                case FormatEnum.gif:
                                    mime = "image/gif";
                                    extension = ".gif   ";
                                    break;
                                case FormatEnum.jpeg:
                                    mime = "image/jpeg";
                                    extension = ".jpeg";
                                    break;
                                case FormatEnum.png:
                                    mime = "image/png";
                                    extension = ".png";
                                    break;
                                default:
                                    mime = "image/png";
                                    extension = ".png";
                                    break;
                            }

                            string oldExtension = Path.GetExtension(f.Path);
                            string output = !string.IsNullOrWhiteSpace(oldExtension) ? f.Path.Replace(Path.GetExtension(f.Path), ".tap") : f.Path + ".tap";

                            if (!string.IsNullOrWhiteSpace(OutputFolder))
                            {
                                output = string.Format("{0}{1}{2}", OutputFolder, f.Filename, extension);
                            }

                            ImageCodecInfo codec = GetEncoderInfo(mime);
                            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                            EncoderParameters parameters = new EncoderParameters(1);
                            EncoderParameter parameter = new EncoderParameter(encoder, 100L);
                            parameters.Param[0] = parameter;
                            bmp.Save(output, codec, parameters);
                        }
                    }
                }

                MessageBox.Show(Translator.ProcessSuccess, Translator.AppTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("{0} {1}", currentFile, ex.Message), Translator.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        #endregion
    }
}
