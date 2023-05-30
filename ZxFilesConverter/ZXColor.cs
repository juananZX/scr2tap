using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxFilesConverter
{
    internal class ZXColor
    {
        public static Color GetColor(ZXColorEnum color)
        {
            switch(color)
            {
                case ZXColorEnum.Black:
                    return Color.FromArgb(0x00, 0x00, 0x00);
                case ZXColorEnum.Blue:
                    return Color.FromArgb(0x00, 0x00, 0xd8);
                case ZXColorEnum.Red:
                    return Color.FromArgb(0xd8, 0x00, 0x00);
                case ZXColorEnum.Magenta:
                    return Color.FromArgb(0xd8, 0x00, 0xd8);
                case ZXColorEnum.Green:
                    return Color.FromArgb(0x00, 0xd8, 0x00);
                case ZXColorEnum.Cyan:
                    return Color.FromArgb(0x00, 0xd8, 0xd8);
                case ZXColorEnum.Yellow:
                    return Color.FromArgb(0xd8, 0xd8, 0x00);
                case ZXColorEnum.White:
                    return Color.FromArgb(0xd8, 0xd8, 0xd8);
                case ZXColorEnum.LightBlack:
                    return Color.FromArgb(0x00, 0x00, 0x00);
                case ZXColorEnum.LightBlue:
                    return Color.FromArgb(0x00, 0x00, 0xff);
                case ZXColorEnum.LightRed:
                    return Color.FromArgb(0xff, 0x00, 0x00);
                case ZXColorEnum.LightMagenta:
                    return Color.FromArgb(0xff, 0x00, 0xff);
                case ZXColorEnum.LightGreen:
                    return Color.FromArgb(0x00, 0xff, 0x00);
                case ZXColorEnum.LightCyan:
                    return Color.FromArgb(0x00, 0xff, 0xff);
                case ZXColorEnum.LightYellow:
                    return Color.FromArgb(0xff, 0xff, 0x00);
                case ZXColorEnum.LightWhite:
                    return Color.FromArgb(0xff, 0xff, 0xff);
                default:
                    return Color.FromArgb(0x00, 0x00, 0x00);
            }
        }
    }
}
