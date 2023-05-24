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

            ViewModel vm = new ViewModel();

            vm.BinaryFiles.Add(new ZXFile("", "Game Over", "Game Over", "", FormatEnum.tap));
            vm.BinaryFiles.Add(new ZXFile("", "La abadía del crimen", "Abadia", "", FormatEnum.tap));
            vm.BinaryFiles.Add(new ZXFile("", "Sir Fred", "SirFred", "", FormatEnum.tap));

            this.DataContext = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
