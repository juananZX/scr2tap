using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxFilesConverter
{
    public static class Extensions
    {
        static Dictionary<char, char> ascii = new Dictionary<char, char>
        {
            { 'á', 'a' },
            { 'à', 'a' },
            { 'ä', 'a' },
            { 'â', 'a' },
            { 'Á', 'A' },
            { 'À', 'A' },
            { 'Ä', 'A' },
            { 'Â', 'A' },
            { 'é', 'e' },
            { 'è', 'e' },
            { 'ë', 'e' },
            { 'ê', 'e' },
            { 'É', 'E' },
            { 'È', 'E' },
            { 'Ë', 'E' },
            { 'Ê', 'E' },
            { 'í', 'i' },
            { 'ì', 'i' },
            { 'ï', 'i' },
            { 'î', 'i' },
            { 'Í', 'I' },
            { 'Ì', 'I' },
            { 'Ï', 'I' },
            { 'Î', 'I' },
            { 'ó', 'o' },
            { 'ò', 'o' },
            { 'ö', 'o' },
            { 'ô', 'o' },
            { 'Ó', 'O' },
            { 'Ò', 'O' },
            { 'Ö', 'O' },
            { 'Ô', 'O' },
            { 'ú', 'u' },
            { 'ù', 'u' },
            { 'ü', 'u' },
            { 'û', 'u' },
            { 'Ú', 'U' },
            { 'Ù', 'U' },
            { 'Ü', 'U' },
            { 'Û', 'U' }
        };

        public static string ZXCharacter(this string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);

            foreach(char c in text)
            {
                if (ascii.ContainsKey(c))
                {
                    sb.Append(ascii[c]);
                }
                else if (Convert.ToByte(c) > 31 && Convert.ToByte(c) < 128)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('.');
                }
            }

            return sb.ToString();
        }
    }
}
