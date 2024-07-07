using UnityEngine;

namespace Milease.Colors
{
    public static class ColorUtils
    {
        public static Color WhiteClear = new Color(1f, 1f, 1f, 0f);
        
        public static Color RGB(float r, float g, float b)
            => new Color(r / 255f, g / 255f, b / 255f, 1f);
        
        public static Color RGBA(float r, float g, float b, float a)
            => new Color(r / 255f, g / 255f, b / 255f, a);

        public static Color Clear(this Color color)
        {
            color.a = 0;
            return color;
        }
        
        public static Color Opacity(this Color color, float a)
        {
            color.a = a;
            return color;
        }
        
        public static Color Opacity255(this Color color, float a)
        {
            color.a = a / 255f;
            return color;
        }
        
        public static string SerializeColor(Color color)
        {
            var oklch = Oklch.FromColor(color);
            return oklch.ToString();
        }

        public static Color ParseColor(string src)
        {
            src = src.Trim();
            if (string.IsNullOrWhiteSpace(src))
            {
                return Color.white;
            }

            if (uint.TryParse(src, out var uintColor))
            {
                return ParseUintColor(uintColor);
            }

            if (src.StartsWith("#"))
            {
                src = src[1..];
                if (src.Length == 3 &&
                    Tools.TryParseHexToUint($"{src[0]}{src[0]}{src[1]}{src[1]}{src[2]}{src[2]}FF", out uintColor))
                {
                    return ParseUintColor(uintColor);
                }

                if (src.Length == 6 && Tools.TryParseHexToUint($"{src}FF", out uintColor))
                {
                    return ParseUintColor(uintColor);
                }

                if (src.Length == 8 && Tools.TryParseHexToUint($"{src}", out uintColor))
                {
                    return ParseUintColor(uintColor);
                }
            }

            if (src.StartsWith("oklab(") && src.EndsWith(")"))
            {
                return Oklab.ParseOklch(src).ToColor();
            }

            if (src.StartsWith("oklch(") && src.EndsWith(")"))
            {
                return Oklch.ParseOklch(src).ToColor();
            }

            return Color.white;
        }

        private static Color ParseUintColor(uint src)
        {
            var b = new byte[]
            {
                (byte)(src >> 24),
                (byte)((src >> 16) & 0x00FF),
                (byte)((src >> 8) & 0x0000FF),
                (byte)(src & 0x000000FF)
            };
            return new Color(b[0] * 1f / 255f, b[1] * 1f / 255f, b[2] * 1f / 255f, b[3] * 1f / 255f);
        }
    }
}