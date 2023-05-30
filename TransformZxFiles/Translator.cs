using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxFilesConverter
{
    internal class Translator
    {
        public string AddFiles { get; set; }

        public string AppTitle { get; set; }

        public string BinToTap { get; set; }

        public string Clear { get; set; }

        public string Code { get; set; }

        public string File { get; set; }

        public string Format { get; set; }

        public string Header { get; set; }

        public string Language { get; set; }

        public string Output { get; set; }

        public string ProcessSuccess { get; set; }

        public string Remove { get; set; }

        public string ScreenToImage { get; set; }

        public string Transform { get; set; }

        public void Load()
        {
            switch(Code?.ToLower()) 
            {
                case "es":
                    LoadEs();
                    break;
                default:
                    LoadEn();
                    break;
            }
        }

        private void LoadEn()
        {
            AddFiles = "Add files";
            AppTitle = "Transform ZX Spectrum files";
            BinToTap = "Bin to tap";
            Clear = "Clear";
            File = "File";
            Format = "Format";
            Header = "Header";
            Language = "Language";
            Output = "Output";
            ProcessSuccess = "Process success";
            Remove = "Remove";
            ScreenToImage = "Screen to image";
            Transform = "Transform";
        }

        private void LoadEs()
        {
            AddFiles = "Añadir archivos";
            AppTitle = "Conversor de archivos de ZX Spectrum";
            BinToTap = "Binario a cinta";
            Clear = "Limpiar";
            File = "Archivo";
            Format = "Formato";
            Header = "Cabecera";
            Language = "Idioma";
            Output = "Salida";
            ProcessSuccess = "Proceso finalizado";
            Remove = "Quitar";
            ScreenToImage = "Pantalla a imagen";
            Transform = "Convertir";
        }
    }
}
