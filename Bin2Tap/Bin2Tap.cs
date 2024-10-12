using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace ZxFilesConverter
{
    /// <summary>
    /// Binary file to tape file converter.
    /// </summary>
    internal class Bin2Tap
    {
        static int Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Bin2Tap: fatal error, no input files");
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
                        Console.WriteLine($"Bin2Tap: fatal error, param: {arg}");

                        return -1;
                    }

                    string param = arg.ToLower().Substring(0, 2);
                    string value = arg.Substring(2);
                    int valueInt = 0;

                    switch (param)
                    {
                        case "":
                        case "-i":
                            inputPaths.Add(arg);
                            break;
                        case "-a":
                            if (!int.TryParse(value, out valueInt))
                            {
                                Console.WriteLine($"Bin2Tap: fatal error, param: {arg}");
                            }
                            addresses.Add(valueInt);
                            break;
                        case "-o":
                            outputPaths.Add(param);
                            break;
                        case "p":
                            if (!int.TryParse(value, out valueInt))
                            {
                                Console.WriteLine($"Bin2Tap: fatal error, param: {arg}");
                            }
                            blockTypes.Add(valueInt == 0 ? 0 : 3);
                            break;
                        case "-s":
                            blockNames.Add(param);
                            break;
                    }
                }

                for (int i = 0; i < inputPaths.Count; i++)
                {
                    string inputPath = inputPaths[i];
                    string outputPath = outputPaths.Count > i ? outputPaths[i] : $"{Path.GetFileNameWithoutExtension(inputPaths[i])}.tap";
                    int address = addresses.Count > i ? addresses[i] : 0;
                    int blockType = blockTypes.Count > i ? blockTypes[i] : 3;
                    string blockName = blockNames.Count > i ? blockNames[i] : blockType == 0 ? "PROGRAM" : "BYTES";
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
