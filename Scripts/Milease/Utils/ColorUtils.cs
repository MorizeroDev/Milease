using UnityEngine;

namespace Milease.Utils
{
    public static class ColorUtils
    {
        public static Color RGB(float r, float g, float b)
            => new Color(r / 255f, g / 255f, b / 255f, 1f);
        
        public static Color RGBA(float r, float g, float b, float a)
            => new Color(r / 255f, g / 255f, b / 255f, a);
    }
}