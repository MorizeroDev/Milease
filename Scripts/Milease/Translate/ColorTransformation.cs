#if COLOR_TOOL_SETUP && (NET_STANDARD_2_1 || POLYFILL_SETUP)
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milease.Configuration;
using Milease.Core.Animation;
using Paraparty.Colors;
using UnityEngine;

#if NET_STANDARD_2_1
using MathPolyfill = System.Math;

#else
using MathPolyfill = Paraparty.UnityPolyfill.MathPolyfill;
#endif

namespace Milease.Translate
{
    public class ColorTransformation : ITransformation
    {
        public bool CanTranslate<E>()
        {
            var type = typeof(E);
            if (type == typeof(Color))
            {
                return true;
            }

            if (type == typeof(Oklab))
            {
                return true;
            }

            if (type == typeof(Oklch))
            {
                return true;
            }

            return false;
        }

        public MileaseHandleFunction<T, E> MakeTransformation<T, E>(BlendingMode blendingMode)
        {
            return MakeTransformation<T, E>(MileaseConfiguration.Configuration.DefaultColorTransformationType,
                blendingMode);
        }

        public MileaseHandleFunction<T, E> MakeTransformation<T, E>(ColorTransformationType colorTransformation,
            BlendingMode blendingMode)
        {
            return blendingMode switch
            {
                BlendingMode.Default => colorTransformation switch
                {
                    ColorTransformationType.RGB => MakeTransformation(
                        (Func<MilHandleFunctionArgs<T, E>, Color>)RgbNormalTransformation),
                    ColorTransformationType.OKLAB => MakeTransformation(
                        (Func<MilHandleFunctionArgs<T, E>, Color>)OklabNormalTransformation),
                    ColorTransformationType.OKLCH => MakeTransformation(
                        (Func<MilHandleFunctionArgs<T, E>, Color>)OklchNormalTransformation),
                    _ => throw new ArgumentOutOfRangeException(nameof(colorTransformation), colorTransformation, null)
                },
                BlendingMode.Additive => colorTransformation switch
                {
                    ColorTransformationType.RGB => MakeTransformation(
                        (Func<MilHandleFunctionArgs<T, E>, Color>)RgbAdditiveTransformation),
                    ColorTransformationType.OKLAB => MakeTransformation(
                        (Func<MilHandleFunctionArgs<T, E>, Color>)OklabAdditiveTransformation),
                    ColorTransformationType.OKLCH => MakeTransformation(
                        (Func<MilHandleFunctionArgs<T, E>, Color>)OklchAdditiveTransformation),
                    _ => throw new ArgumentOutOfRangeException(nameof(colorTransformation), colorTransformation, null)
                },
                _ => throw new ArgumentException("unsupported color transformation mode")
            };
        }

        private MileaseHandleFunction<T, E> MakeTransformation<T, E>(
            Func<MilHandleFunctionArgs<T, E>, Color> colorTransformation)
        {
            return e =>
            {
                var result = colorTransformation(e);
                e.Animation.ValueSetter.Invoke(e.Target, (E)(object)result);
            };
        }

        private Oklab GetOklab(object obj)
        {
            return obj switch
            {
                Color color => Oklab.FromColor(color),
                Oklab oklab => oklab,
                Oklch oklch => Oklab.FromOklch(oklch),
                _ => throw new ArgumentException($"unsupported color type: {obj.GetType().Name}")
            };
        }

        private Oklch GetOklch(object obj)
        {
            return obj switch
            {
                Color color => Oklch.FromColor(color),
                Oklab oklab => Oklch.FromOklab(oklab),
                Oklch oklch => oklch,
                _ => throw new ArgumentException($"unsupported color type: {obj.GetType().Name}")
            };
        }

        private Color GetColor(object obj)
        {
            return obj switch
            {
                Color color => color,
                Oklab oklab => oklab.ToColor(),
                Oklch oklch => oklch.ToColor(),
                _ => throw new ArgumentException($"unsupported color type: {obj.GetType().Name}")
            };
        }

        private Color RgbNormalTransformation<T, E>(MilHandleFunctionArgs<T, E> e)
        {
            var ani = e.Animation;
            var pro = e.Progress;

            var fromColor = GetColor(ani.StartValue);
            var toColor = GetColor(ani.ToValue);
            return fromColor + (toColor - fromColor) * pro;
        }

        private Color RgbAdditiveTransformation<T, E>(MilHandleFunctionArgs<T, E> e)
        {
            var ani = e.Animation;
            var pro = e.Progress;

            var originalColor = GetColor(ani.OriginalValue);
            var fromColor = GetColor(ani.StartValue);
            var toColor = GetColor(ani.ToValue);
            return originalColor + fromColor + (toColor - fromColor) * pro;
        }

        private Color OklabNormalTransformation<T, E>(MilHandleFunctionArgs<T, E> e)
        {
            var ani = e.Animation;
            var pro = e.Progress;

            var fromColor = GetOklab(ani.StartValue);
            var toColor = GetOklab(ani.ToValue);

            var l = (toColor.L - fromColor.L) * pro + fromColor.L;
            var a = (toColor.A - fromColor.A) * pro + fromColor.A;
            var b = (toColor.B - fromColor.B) * pro + fromColor.B;
            var opacity = (toColor.Opacity - fromColor.Opacity) * pro + fromColor.Opacity;
            return new Oklab(l, a, b, opacity).ToColor();
        }

        private Color OklabAdditiveTransformation<T, E>(MilHandleFunctionArgs<T, E> e)
        {
            var ani = e.Animation;
            var pro = e.Progress;

            var originalColor = GetOklab(ani.OriginalValue);
            var fromColor = GetOklab(ani.StartValue);
            var toColor = GetOklab(ani.ToValue);

            var l = originalColor.L + (toColor.L - fromColor.L) * pro + fromColor.L;
            l = MathPolyfill.Clamp(l, 0.0, 1.0);
            var a = originalColor.A + (toColor.A - fromColor.A) * pro + fromColor.A;
            a = MathPolyfill.Clamp(a, -0.4, 0.4);
            var b = originalColor.B + (toColor.B - fromColor.B) * pro + fromColor.B;
            b = MathPolyfill.Clamp(b, -0.4, 0.4);
            var opacity = originalColor.Opacity + (toColor.Opacity - fromColor.Opacity) * pro + fromColor.Opacity;
            return new Oklab(l, a, b, opacity).ToColor();
        }

        private Color OklchNormalTransformation<T, E>(MilHandleFunctionArgs<T, E> e)
        {
            var ani = e.Animation;
            var pro = e.Progress;

            var fromColor = GetOklch(ani.StartValue);
            var toColor = GetOklch(ani.ToValue);

            var fromH = fromColor.H;
            var toH = toColor.H;
            var hueDiff = NormalizeHue(toH - fromH);

            var l = (toColor.L - fromColor.L) * pro + fromColor.L;
            var c = (toColor.C - fromColor.C) * pro + fromColor.C;
            var h = fromH + hueDiff * pro;
            if (h < 0)
                h += 360;
            else if (h > 360)
                h -= 360;

            var opacity = (toColor.Opacity - fromColor.Opacity) * pro + fromColor.Opacity;
            return new Oklch(l, c, h, opacity).ToColor();
        }

        private Color OklchAdditiveTransformation<T, E>(MilHandleFunctionArgs<T, E> e)
        {
            var ani = e.Animation;
            var pro = e.Progress;

            var originalColor = GetOklch(ani.OriginalValue);
            var fromColor = GetOklch(ani.StartValue);
            var toColor = GetOklch(ani.ToValue);

            var fromH = fromColor.H;
            var toH = toColor.H;
            var hueDiff = NormalizeHue(toH - fromH);

            var l = originalColor.L + (toColor.L - fromColor.L) * pro + fromColor.L;
            l = MathPolyfill.Clamp(l, 0.0, 1.0);
            var c = originalColor.C + (toColor.C - fromColor.C) * pro + fromColor.C;
            var h = originalColor.H + fromH + hueDiff * pro;
            h = NormalizeHue(h);

            var opacity = (toColor.Opacity - fromColor.Opacity) * pro + fromColor.Opacity;
            opacity = MathPolyfill.Clamp(opacity, 0.0, 1.0);

            return new Oklch(l, c, h, opacity).ToColor();
        }

        private double NormalizeHue(double hue)
        {
            while (hue > 180)
            {
                hue -= 360;
            }

            while (hue < -180)
            {
                hue += 360;
            }

            return hue;
        }
    }
}
#endif