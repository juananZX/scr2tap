using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        static int Main(string[] args)
        {
            StringBuilder error = new StringBuilder();

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
                                Console.WriteLine($"Bin2Tap: fatal error -3, param: {arg}");
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
                                Console.WriteLine($"Bin2Tap: fatal error -3, param: {arg}");
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

                if (addresses.Any() && addresses.Count != inputPaths.Count) error.AppendLine($"inputs != addresses: {inputPaths.Count}, {addresses.Count}");
                if (outputPaths.Any() && outputPaths.Count != inputPaths.Count) error.AppendLine($"inputs != oputputs: {inputPaths.Count}, {outputPaths.Count}");
                if (blockTypes.Any() && blockTypes.Count != inputPaths.Count) error.AppendLine($"inputs != blockTypes: {inputPaths.Count}, {blockTypes.Count}");
                if (blockNames.Any() && blockNames.Count != inputPaths.Count) error.AppendLine($"inputs != blockNames: {inputPaths.Count}, {blockNames.Count}");

                if (error.Length > 0)
                {
                    Console.WriteLine("Bin2Tap: fatal error - 4:");
                    Console.WriteLine(error.ToString());
                    Console.ReadKey();
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
                }
            }

            return 0;
        }
    }
}
