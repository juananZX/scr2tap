using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZxFilesConverter
{
    internal class Translator
    {
        public string AddFiles { get; set; }

        public string Address { get; set; }

        public string AppTitle { get; set; }

        public string BinToTap { get; set; }

        public string BlockType { get; set; }

        public string Clear { get; set; }

        public string Code { get; set; }

        public string File { get; set; }

        public string Filename { get; set; }

        public string Format { get; set; }

        public string Header { get; set; }

        public string Language { get; set; }

        public string OriginFolder { get; set; }

        public string Output { get; set; }

        public string ProcessSuccess { get; set; }

        public string Remove { get; set; }

        public string RLECompression { get; set; }

        public string ScreenToImage { get; set; }

        public string Transform { get; set; }

        public string Unzip { get; set; }

        public string Zip { get; set; }

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
            Address = "Address / Line";
            AppTitle = "Transform ZX Spectrum files";
            BinToTap = "Bin to tap";
            BlockType = "Block type";
            Clear = "Clear";
            File = "File";
            Filename = "File";
            Format = "Format";
            Header = "Header";
            Language = "Language";
            OriginFolder = "To origin folder";
            Output = "Output folder";
            ProcessSuccess = "Process success";
            Remove = "Remove";
            RLECompression = "RLE compression";
            ScreenToImage = "Screen to image";
            Transform = "Transform";
            Unzip = "Unzip";
            Zip = "Zip";
        }

        private void LoadEs()
        {
            AddFiles = "Añadir archivos";
            Address = "Dirección / Línea";
            AppTitle = "Conversor de archivos de ZX Spectrum";
            BinToTap = "Binario a cinta";
            BlockType = "Tipo de bloque";
            Clear = "Limpiar";
            File = "Archivo";
            Filename = "Archivo";
            Format = "Formato";
            Header = "Cabecera";
            Language = "Idioma";
            OriginFolder = "En carpeta origen";
            Output = "Carpeta de salida";
            ProcessSuccess = "Proceso finalizado";
            Remove = "Quitar";
            RLECompression = "Compresión RLE";
            ScreenToImage = "Pantalla a imagen";
            Transform = "Convertir";
            Unzip = "Unzip";
            Zip = "Zip";
        }
    }
}
