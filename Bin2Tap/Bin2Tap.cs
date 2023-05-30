using System;
using System.IO;

namespace ZxFilesConverter
{
    /// <summary>
    /// Binary file to tape file converter.
    /// </summary>
    internal class Bin2Tap
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Bin2Tap: fatal error, no input files");
            }
            else
            {
                foreach (string arg in args)
                {
                    string name = Path.GetFileName(arg).Replace(Path.GetExtension(arg), "");
                    string path = Path.GetFullPath(arg).Replace(Path.GetExtension(arg), ".tap");
                    byte[] buffer;

                    using (FileStream r = new FileStream(arg, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        buffer = new byte[r.Length];
                        r.Read(buffer, 0, buffer.Length);
                        r.Close();
                    }

                    buffer = ZxFileConverter.Bin2Tap(buffer, name);
                    
                    using (FileStream w = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                    {
                        w.Write(buffer, 0, buffer.Length);
                        w.Close();
                    }
                }
            }
        }
    }
}
