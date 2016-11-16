using System;
using System.Drawing;

namespace Silverfeelin.StarboundDrawables
{
    public static class ColorConversions
    {
        /// <summary>
        /// Returns a System.Drawing.Color from a hexadecimal color string, formatted 'RRGGBB' or 'RRGGBBAA'.
        /// </summary>
        /// <param name="rgba">Hexadecimal color string, formatted 'RRGGBB' or 'RRGGBBAA'</param>
        /// <returns><see cref="System.Drawing.Color"/> for the given color string</returns>
        /// <exception cref="FormatException">Thrown if the given value is not a valid hexadecimal string.</exception>
        public static Color RGBAHexStringToColor(string rgba)
        {
            if (rgba.Length == 6 || rgba.Length == 8)
            {
                int r = HexToInt(rgba.Substring(0, 2)),
                    g = HexToInt(rgba.Substring(2, 2)),
                    b = HexToInt(rgba.Substring(4, 2)),
                    a;

                if (rgba.Length == 8)
                {
                    a = HexToInt(rgba.Substring(6, 2));
                }
                else
                {
                    a = 255;
                }

                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                return Color.Transparent;
            }
        }

        /// <summary>
        /// Returns a hexadecimal color string from a System.Drawing.Color, formatted 'RRGGBBAA'.
        /// </summary>
        /// <param name="c">The System.Drawing.Color to convert</param>
        /// <returns>Hexadecimal color string, formatted 'RRGGBBAA'</returns>
        public static string ToRGBAHexString(this Color c)
        {
            string r = c.R.ToString("X2");
            string g = c.G.ToString("X2");
            string b = c.B.ToString("X2");
            string a = c.A.ToString("X2");

            return r + g + b + a;
        }

        /// <summary>
        /// Returns the integral value of the given hexadecimal number.
        /// </summary>
        /// <param name="hex">Hexadecimal number string</param>
        /// <returns>Converted Integer or -1 if the conversion failed.</returns>
        /// <exception cref="FormatException">Thrown if the given value is not a valid hexadecimal string.</exception>
        public static int HexToInt(string hex)
        {
            uint number = Convert.ToUInt32(hex, 16);
            return (int)number;
        }
    }
}
