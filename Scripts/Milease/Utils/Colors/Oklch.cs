using System;
using System.Linq;
using UnityEngine;

namespace Milease.Utils.Colors
{
    public struct Oklch
    {
        public double L { get; set; }
        public double C { get; set; }
        public double H { get; set; }
        public double Opacity { get; set; }

        public override string ToString()
        {
            if (Opacity > 0.999)
            {
                return $"oklch({L} {C} {H})";
            }
            else
            {
                return $"oklch({L} {C} {H} / {Opacity})";
            }
        }

        public Oklch(double l, double c, double h, double opacity = 1)
        {
            L = l;
            C = c;
            H = h;
            Opacity = opacity;
        }


        public Color ToColor()
        {
            var oklab = Oklab.FromOklch(this);
            return oklab.ToColor();
        }

        public static Oklch FromColor(Color color)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;
            float a = color.a;

            return FromColor(r, g, b, a);
        }

        private static Oklch FromColor(float r, float g, float b, float a)
        {
            var oklab = Oklab.FromColor(r, g, b, a);
            return FromOklab(oklab);
        }

        private static Oklch FromOklab(Oklab oklab)
        {
            var l = oklab.L;
            var a = oklab.A;
            var b = oklab.B;

            double c = Math.Sqrt(a * a + b * b);
            double h = Math.Atan2(b, a);
            if (h < 0) h += 2 * Math.PI;
            return new Oklch(l, c, h * 180 / Math.PI, oklab.Opacity);
        }

        static readonly int OklchPrefixLength = "oklch(".Length;
        static readonly int OklchSuffixLength = ")".Length;

        private static Oklch White = FromColor(Color.white);

        internal static Oklch ParseOklch(string ssrc)
        {
            var src = ssrc[OklchPrefixLength ..^OklchSuffixLength];
            if (string.IsNullOrWhiteSpace(src))
            {
                Tools.LogError($"empty Oklch: {ssrc}");
                return White;
            }

            var colorAlpha = src.Split("/");
            if (colorAlpha.Length > 2)
            {
                Tools.LogError($"multi slash: {ssrc}");
                return White;
            }

            double opacity = 1;
            if (colorAlpha.Length == 2)
            {
                var alpha = colorAlpha[1];
                if (Tools.TryParsePercentage(alpha, out opacity))
                {
                }
                else if (Tools.TryParseDouble(alpha, out opacity))
                {
                }
                else
                {
                    Tools.LogError($"parsing Alpha error: {ssrc}");
                    return White;
                }

                opacity = Math.Clamp(opacity, 0.0, 1.0);
            }

            var lch = colorAlpha[0].Split(" ").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (lch.Length != 3)
            {
                Tools.LogError($"lch elements mismatched: {ssrc}");
                return White;
            }

            string lStr = lch[0];
            string cStr = lch[1];
            string hStr = lch[2];

            double l = 0;
            double c = 0;
            double h = 0;

            if (Tools.TryParsePercentage(lStr, out l))
            {
            }
            else if (Tools.TryParseDouble(lStr, out l))
            {
            }
            else
            {
                Tools.LogError($"parsing L error: {ssrc}");
                return White;
            }

            l = Math.Clamp(l, 0.0, 1.0);

            if (Tools.TryParsePercentage(cStr, out c))
            {
                c = c * 0.4;
            }
            else if (Tools.TryParseDouble(cStr, out c))
            {
            }
            else
            {
                Tools.LogError($"parsing C error: {ssrc}");
                return White;
            }


            if (Tools.TryParseAngle(hStr, out h))
            {
            }
            else
            {
                Tools.LogError($"parsing H error: {ssrc}");
                return White;
            }

            return new Oklch(l, c, h, opacity);
        }
    }
}
