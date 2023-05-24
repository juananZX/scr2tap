// Especificación de como montar un .tap
// https://wiki.speccy.org/cursos/ensamblador/rutinas_save_load

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using convertScr;

namespace scr2tap
{
    [Obsolete]
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("scr2tap: fatal error: no input files");
                return 0x01;
            }

            (new ConvertScr()).ToTap(args);

            return 0x00;
        }
    }
}