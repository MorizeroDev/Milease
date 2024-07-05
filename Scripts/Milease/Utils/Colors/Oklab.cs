using System;
using System.Linq;
using UnityEngine;

namespace Milease.Utils.Colors
{
    public struct Oklab
    {
        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double Opacity { get; set; }

        public override string ToString()
        {
            if (Opacity > 0.999)
            {
                return $"oklab({L} {A} {B})";
            }
            else
            {
                return $"oklab({L} {A} {B} / {Opacity})";
            }
        }

        public Oklab(double l, double a, double b, double opacity = 1)
        {
            L = l;
            A = a;
            B = b;
            Opacity = opacity;
        }

        public Color ToColor()
        {
            double l_ = L + 0.3963377774 * A + 0.2158037573 * B;
            double m_ = L - 0.1055613458 * A - 0.0638541728 * B;
            double s_ = L - 0.0894841775 * A - 1.2914855480 * B;

            double lCube = l_ * l_ * l_;
            double mCube = m_ * m_ * m_;
            double sCube = s_ * s_ * s_;

            double r = 4.0767416621 * lCube - 3.3077115913 * mCube + 0.2309699292 * sCube;
            double g = -1.2684380046 * lCube + 2.6097574011 * mCube - 0.3413193965 * sCube;
            double b = -0.0041960863 * lCube - 0.7034186147 * mCube + 1.7076147010 * sCube;

            float commitR = (float)GammaCorrect(r);
            float commitG = (float)GammaCorrect(g);
            float commitB = (float)GammaCorrect(b);
            float commitA = (float)Opacity;

            return new Color(commitR, commitG, commitB, commitA);

            double GammaCorrect(double c)
            {
                if (c <= 0.0031308)
                {
                    return 12.92 * c;
                }
                else
                {
                    return 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;
                }
            }
        }


        public static Oklab FromColor(Color color)
        {
            double r = color.r;
            double g = color.g;
            double b = color.b;
            double a = color.a;

            return FromColor(r, g, b, a);
        }

        public static Oklab FromColor(double inputR, double inputG, double inputB, double inputA)
        {
            double r = GammaCorrect(inputR);
            double g = GammaCorrect(inputG);
            double b = GammaCorrect(inputB);
            double a = inputA;

            double l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
            double m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b;
            double s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

            // Apply cubic root
            double l_ = Math.Cbrt(l);
            double m_ = Math.Cbrt(m);
            double s_ = Math.Cbrt(s);

            // Final calculation
            double lCube = 0.2104542553 * l_ + 0.7936177850 * m_ - 0.0040720468 * s_;
            double aCube = 1.9779984951 * l_ - 2.4285922050 * m_ + 0.4505937099 * s_;
            double bCube = 0.0259040371 * l_ + 0.7827717662 * m_ - 0.8086757660 * s_;
            return new Oklab(lCube, aCube, bCube, a);

            double GammaCorrect(double c)
            {
                if (c <= 0.04045)
                {
                    return c / 12.92;
                }
                else
                {
                    return Math.Pow((c + 0.055) / 1.055, 2.4);
                }

                throw new Exception();
            }
        }

        static readonly int OklabPrefixLength = "oklab(".Length;
        static readonly int OklabSuffixLength = ")".Length;

        public static Oklab FromOklch(Oklch oklch)
        {
            var l = oklch.L;
            var c = oklch.C;
            var h = oklch.H;

            double a = Math.Cos(h * Math.PI / 180) * c;
            double b = Math.Sin(h * Math.PI / 180) * c;
            return new Oklab(l, a, b, oklch.Opacity);
        }


        private static Oklab White = FromColor(Color.white);

        public static Oklab ParseOklch(string ssrc)
        {
            var src = ssrc[OklabPrefixLength ..^OklabSuffixLength];
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
            string aStr = lch[1];
            string bStr = lch[2];

            double l = 0;
            double a = 0;
            double b = 0;

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


            if (Tools.TryParsePercentage(aStr, out a))
            {
                a = a * 0.8 - 0.4;
            }
            else if (Tools.TryParseDouble(aStr, out a))
            {
            }
            else
            {
                Tools.LogError($"parsing A error: {ssrc}");
                return White;
            }

            a = Math.Clamp(a, -0.4, 0.4);


            if (Tools.TryParsePercentage(bStr, out b))
            {
                b = b * 0.8 - 0.4;
            }
            else if (Tools.TryParseDouble(bStr, out b))
            {
            }
            else
            {
                Tools.LogError($"parsing B error: {ssrc}");
                return White;
            }

            b = Math.Clamp(b, -0.4, 0.4);

            return new Oklab(l, a, b, opacity);
        }
    }
}
