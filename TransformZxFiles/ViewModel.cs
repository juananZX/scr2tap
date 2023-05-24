using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ZxFilesConverter
{
    internal class ViewModel
    {
        private ICommand findBinaryCommand;

        public ViewModel()
        {
            BinaryFiles = new ObservableCollection<ZXFile>();
        }

        public ObservableCollection<ZXFile> BinaryFiles { get; private set; }

        public ICommand FindBinaryCommand 
        {
            get
            {
                return findBinaryCommand ?? (findBinaryCommand = new CommandHandler(() => FindBinary(), () => CanExecute));
            }
        }

        public bool CanExecute
        {
            get
            {
                // check if executing is allowed, i.e., validate, check if a process is running, etc. 
                return true;
            }
        }

        public void FindBinary()
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == true)
            {
                BinaryFiles.Clear();
            }
        }
    }
}
