using System;
using System.Drawing;

namespace Silverfeelin.StarboundDrawables
{
    public static class ColorConversions
    {
        /// <summary>
        /// Returns a System.Drawing.Color from a hexadecimal color string, formatted 'RRGGBB' or 'RRGGBBAA'.
        /// If the string is null or empty, returns <see cref="Color.Transparent"/>.
        /// </summary>
        /// <param name="rgba">Hexadecimal color string, formatted 'RRGGBB' or 'RRGGBBAA'</param>
        /// <returns><see cref="Color"/> for the given color string</returns>
        /// <exception cref="FormatException">Thrown if the given value is not a valid hexadecimal string.</exception>
        public static Color RGBAHexStringToColor(string rgba)
        {
            if (string.IsNullOrWhiteSpace(rgba)) return Color.Transparent;

            int r = 0, g = 0, b = 0, a = 255;
            switch (rgba.Length)
            {
                case 4:
                    a = HexToInt(rgba[3].ToString());
                    goto case 3;
                case 3:
                    r = HexToInt(rgba[0].ToString());
                    g = HexToInt(rgba[1].ToString());
                    b = HexToInt(rgba[2].ToString());
                    break;
                case 8:
                    a = HexToInt(rgba.Substring(6, 2));
                    goto case 6;
                case 6:
                    r = HexToInt(rgba.Substring(0, 2));
                    g = HexToInt(rgba.Substring(2, 2));
                    b = HexToInt(rgba.Substring(4, 2));
                    break;
                default:
                    throw new FormatException("Invalid hex length");
            }

            return Color.FromArgb(a, r, g, b);
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

            bool isOpaque = a == "FF";
            bool isShort = r[0] == r[1] && g[0] == g[1] && b[0] == b[1] && a[0] == a[1];

            return isShort
                ? isOpaque ? new string(new[] { r[0], g[0], b[0] }) : new string(new[] { r[0], g[0], b[0], a[0] })
                : r + g + b + (isOpaque ? "" : a);
        }

        /// <summary>
        /// Returns the integral value of the given hexadecimal number.
        /// </summary>
        /// <param name="hex">Hexadecimal number string</param>
        /// <returns>Converted Integer or -1 if the conversion failed.</returns>
        /// <exception cref="FormatException">Thrown if the given value is not a valid hexadecimal string.</exception>
        public static int HexToInt(this string hex)
        {
            uint number = Convert.ToUInt32(hex, 16);
            return (int)number;
        }
    }
}
