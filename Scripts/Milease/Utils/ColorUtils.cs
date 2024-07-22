using UnityEngine;

namespace Milease.Utils
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
    }
}