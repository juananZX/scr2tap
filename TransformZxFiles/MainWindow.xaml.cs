using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ZxFilesConverter;

namespace TransformZxFiles
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModel();
        }

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            ((ViewModel)DataContext).AddFiles((string[])e.Data.GetData("FileDrop"), FormatEnum.tap);
        }

        private void DataGrid_Drop_1(object sender, DragEventArgs e)
        {
            ((ViewModel)DataContext).AddFiles((string[])e.Data.GetData("FileDrop"), FormatEnum.bmp);
        }

        private void DataGrid_Drop_2(object sender, DragEventArgs e)
        {
            ((ViewModel)DataContext).AddFiles((string[])e.Data.GetData("FileDrop"), FormatEnum.rle);
        }
    }
}
