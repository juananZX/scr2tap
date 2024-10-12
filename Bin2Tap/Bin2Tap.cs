using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

namespace ZxFilesConverter
{
    /// <summary>
    /// Binary file to tape file converter.
    /// </summary>
    internal class Bin2Tap
    {
        /// <summary>
        /// Convierte binarios en archivos .tap
        /// </summary>
        /// <param name="args">Argumentos.</param>
        /// <returns>Resultado del programa.</returns>
        /// <remarks>
        /// Puede procesar uno o más ficheros.
        /// -i:   path del fichero de entrada. Si se omite -i se asume como fichero de entrada
        /// [-f]: carpeta donde está los ficheros.
        /// [-x]: cadena de búsqueda para los ficheros.
        /// [-q]: no muestra mensajes.
        /// [-o]: path del fichero de salida. Por defecto fichero de entrada con extensión .tap.
        /// [-a]: dirección o línea basic de inicio.
        /// [-p]: tipo de bloque. Por defecto 0 (bytes), 1 = programa.
        /// [-s]: nombre del bloque. Por defecto los 10 primeros caracteres del nombre del fichero de entrada sin extensión.
        /// 
        /// Si se especifican algunos de los parámetros opcionales y más de un fichero de entrada, 
        /// se deben especificar todos esos parámetros para todos los ficheros de entrada.
        /// </remarks>
        static int Main(string[] args)
        {
            StringBuilder error = new StringBuilder();
            bool quiet = false;

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Bin2Tap: fatal error -1, no input files");
                return -1;
            }
            else
            {
                List<string> inputPaths = new List<string>();
                List<string> outputPaths = new List<string>();
                List<string> blockNames = new List<string>();
                List<int> addresses = new List<int>();
                List<int> blockTypes = new List<int>();

                foreach (string arg in args)
                {
                    if (arg.Length < 2)
                    {
                        Console.WriteLine($"Bin2Tap: fatal error -2, param: {arg}");

                        return -2;
                    }

                    string param = arg.ToLower().Substring(0, 2);

                    if(param == "-q")
                    {
                        quiet = true;
                        continue;
                    }

                    string value = arg.Substring(2);
                    int valueInt = 0;

                    switch (param)
                    {
                        case "-i":
                            inputPaths.Add(value);
                            break;
                        case "-a":
                            if (!int.TryParse(value, out valueInt))
                            {
                                if (!quiet) Console.WriteLine($"Bin2Tap: fatal error -3, param: {arg}");
                                return -3;
                            }
                            addresses.Add(valueInt);
                            break;
                        case "-o":
                            outputPaths.Add(value);
                            break;
                        case "-p":
                            if (!int.TryParse(value, out valueInt))
                            {
                                if (!quiet) Console.WriteLine($"Bin2Tap: fatal error -3, param: {arg}");
                                return -3;
                            }
                            blockTypes.Add(valueInt);
                            break;
                        case "-s":
                            blockNames.Add(value);
                            break;
                        default:
                            inputPaths.Add(arg);
                            break;
                    }
                }

                if (addresses.Any() && addresses.Count != inputPaths.Count) error.AppendLine($"inputs length != addresses length: {inputPaths.Count}, {addresses.Count}");
                if (outputPaths.Any() && outputPaths.Count != inputPaths.Count) error.AppendLine($"inputs length != ouptputs length: {inputPaths.Count}, {outputPaths.Count}");
                if (blockTypes.Any() && blockTypes.Count != inputPaths.Count) error.AppendLine($"inputs length != blockTypes length: {inputPaths.Count}, {blockTypes.Count}");
                if (blockNames.Any() && blockNames.Count != inputPaths.Count) error.AppendLine($"inputs length != blockNames length: {inputPaths.Count}, {blockNames.Count}");

                if (error.Length > 0)
                {
                    if (!quiet) Console.WriteLine("Bin2Tap: fatal error - 4:");
                    if (!quiet) Console.WriteLine(error.ToString());
                    return -4;
                }

                for (int i = 0; i < inputPaths.Count; i++)
                {
                    string inputPath = inputPaths[i];
                    string outputPath = outputPaths.Count > i ? outputPaths[i] : $"{Path.GetFileNameWithoutExtension(inputPaths[i])}.tap";
                    int address = addresses.Count > i ? addresses[i] : 0;
                    int blockType = blockTypes.Count > i ? blockTypes[i] : 0;
                    string blockName = blockNames.Count > i ? blockNames[i] : Path.GetFileNameWithoutExtension(inputPaths[i]).PadRight(10).Substring(0, 10);
                    byte[] buffer;

                    using (FileStream r = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        buffer = new byte[r.Length];
                        r.Read(buffer, 0, buffer.Length);
                        r.Close();
                    }

                    buffer = ZxFileConverter.Bin2Tap(buffer, blockName, address, blockType);

                    using (FileStream w = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                    {
                        w.Write(buffer, 0, buffer.Length);
                        w.Close();
                    }

                    if (!quiet) Console.WriteLine($"File created: {Path.GetFileName(outputPath)}");
                }
            }

            if (!quiet) Console.WriteLine("Proccess sucess");
            return 0;
        }
    }
}
