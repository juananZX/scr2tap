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

namespace ZxFilesConverter
{
    internal class ViewModel : INotifyPropertyChanged
    {
        #region Fields
        private CommandHandler clearCommand;
        private CommandHandler clearListCommand;
        private Cursor cursor = Cursors.Arrow;
        private CommandHandler deleteCommand;
        private CommandHandler findCommand;
        private CommandHandler outputCommand;
        private CommandHandler transformCommand;
        private CommandHandler transformScreenCommand;
        private KeyValuePair<string, string> languageSelected;
        private string outputFolder;
        #endregion

        #region Constructors
        public ViewModel()
        {
            BinaryFiles = new ObservableCollection<ZXFile>();
            ScreenFiles = new ObservableCollection<ZXFile>();

            OutputFolder = string.Format("{0}", Settings.Default.outputbinary);
            Translator = new Translator();
            Translator.Code = Settings.Default.language;

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

        public ObservableCollection<ZXFile> ScreenFiles { get; private set; }

        public int Format { get; set; }

        public Translator Translator { get; set; }
        #endregion

        #region Private methods
        private void Clear(object param)
        {
            if (param?.ToString() == "binary")
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
                dlg.Filter = "All (*.bin, *.scr)|*.bin;*.scr|Binary (*.bin)|*.bin|Screen (*.scr)|*.scr";
            }
            else
            {
                dlg.Filter = "Screen (*.scr)|*.scr";
            }

            dlg.Multiselect = true;

            if (dlg.ShowDialog() == true)
            {
                if (param?.ToString() == "binary")
                {
                    foreach (string file in dlg.FileNames)
                    {
                        string filename = Path.GetFileName(file).Replace(Path.GetExtension(file), "");

                        if (BinaryFiles.Any(i => i.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase))) continue;

                        string header = filename.ZXCharacter().PadRight(10, ' ').Substring(0, 10).TrimEnd(' ');

                        BinaryFiles.Add(new ZXFile(filename, header, file, FormatEnum.tap));
                    }
                }
                else
                {
                    foreach (string file in dlg.FileNames)
                    {
                        string filename = Path.GetFileName(file).Replace(Path.GetExtension(file), "");

                        if (ScreenFiles.Any(i => i.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase))) continue;

                        ScreenFiles.Add(new ZXFile(filename, string.Empty, file, FormatEnum.tap));
                    }
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Output(object param)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == true)
            {
                string folder = Path.GetFullPath(dlg.FileName).Replace(Path.GetFileName(dlg.FileName), "");

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

                        buffer = ZxFileConverter.Bin2Tap(buffer, f.Header);
                        string output = f.Path.Replace(Path.GetExtension(f.Path), ".tap");

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

                            ImageCodecInfo codec = GetEncoderInfo(mime);
                            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                            EncoderParameters parameters = new EncoderParameters(1);
                            EncoderParameter parameter = new EncoderParameter(encoder, 100L);
                            parameters.Param[0] = parameter;
                            bmp.Save(f.Path.Replace(Path.GetExtension(f.Path), extension), codec, parameters);
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
