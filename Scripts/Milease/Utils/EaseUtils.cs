using System;
using System.Runtime.CompilerServices;
using Milease.Enums;
using UnityEngine;

namespace Milease.Utils
{        
    public static class EaseUtility
    {
        private const float c1 = 1.70158f;
        private const float c2 = c1 * 1.525f;
        private const float c3 = c1 + 1f;
        private const float c4 = (2 * Mathf.PI) / 3f;
        private const float c5 = (2 * Mathf.PI) / 4.5f;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float BezierCubic(float t, float a, float b, float c, float d)
        {
            return ((a * Mathf.Pow(1 - t, 3)) + (3 * b * t * Mathf.Pow(1 - t, 2)) + (3 * c * (1 - t) * Mathf.Pow(t, 2)) + (d * Mathf.Pow(t, 3)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float BounceOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1)
                return n1 * t * t;
            if (t < 2 / d1)
                return n1 * ((t - 1.5f / d1)) * (t - 1.5f / d1) + 0.75f;
            if (t < 2.5 / d1)
                return n1 * ((t - 2.25f / d1)) * (t - 2.25f / d1) + 0.9375f;
            return n1 * ((t - 2.625f / d1)) * (t - 2.625f / d1) + 0.984375f;
        }

        /// <summary>
        /// Obtain eased progress within the range of [0, 1] using specific easing settings.
        /// In this scenario, ensure you have a time system implemented, and your animations should synchronize with this system.
        /// </summary>
        /// <param name="currentTime">The current time of your time system</param>
        /// <param name="startTime">The start time of the animation relative to your time system</param>
        /// <param name="duration">The duration of the animation</param>
        /// <param name="type">The easing type</param>
        /// <param name="function">The easing function</param>
        /// <returns>The eased progress value within the range [0, 1]</returns>
#if MILEASE_AGGRESSIVE_INLINING_EASING_FUNCTION
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static float GetEasedProgress(float currentTime, float startTime, float duration, EaseType type, EaseFunction function)
            => GetEasedProgress(currentTime - startTime, duration, type, function);
        
        /// <summary>
        /// Obtain eased progress within the range of [0, 1] using specific easing settings.
        /// </summary>
        /// <param name="elapsedTime">The time elapsed in your animation</param>
        /// <param name="duration">The duration of the animation</param>
        /// <param name="type">The easing type</param>
        /// <param name="function">The easing function</param>
        /// <returns>The eased progress value within the range [0, 1]</returns>
#if MILEASE_AGGRESSIVE_INLINING_EASING_FUNCTION
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static float GetEasedProgress(float elapsedTime, float duration, EaseType type, EaseFunction function)
            => GetEasedProgress(Mathf.Clamp(elapsedTime / duration, 0f, 1f), type, function);
        
        /// <summary>
        /// Obtain eased progress within the range of [0, 1] using specific easing settings.
        /// </summary>
        /// <param name="progress">Original animation progress within the range of [0, 1]</param>
        /// <param name="type">The easing type</param>
        /// <param name="function">The easing function</param>
        /// <returns>The eased progress value within the range [0, 1]</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported ease function</exception>
#if MILEASE_AGGRESSIVE_INLINING_EASING_FUNCTION
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static float GetEasedProgress(float progress, EaseType type, EaseFunction function)
        {
            var index = (int)function * 3 + (int)type;
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                    break;
                case 3:
                    progress = 1 - Mathf.Cos((progress * Mathf.PI) / 2);
                    break;
                case 4:
                    progress = Mathf.Sin((progress * Mathf.PI) / 2);
                    break;
                case 5:
                    progress = -(Mathf.Cos(Mathf.PI * progress) - 1) / 2;
                    break;
                case 6:
                    progress = progress * progress;
                    break;
                case 7:
                    progress = 1 - (1 - progress) * (1 - progress);
                    break;
                case 8:
                    progress = progress < 0.5 ? 2 * progress * progress : 1 - Mathf.Pow(-2 * progress + 2, 2) / 2;
                    break;
                case 9:
                    progress = progress * progress * progress;
                    break;
                case 10:
                    progress = 1 - Mathf.Pow(1 - progress, 3);
                    break;
                case 11:
                    progress = progress < 0.5 ? 4 * progress * progress * progress : 1 - Mathf.Pow(-2 * progress + 2, 3) / 2;
                    break;
                case 12:
                    progress = progress * progress * progress * progress;
                    break;
                case 13:
                    progress = 1 - Mathf.Pow(1 - progress, 4);
                    break;
                case 14:
                    progress = progress < 0.5 ? 8 * progress * progress * progress * progress : 1 - Mathf.Pow(-2 * progress + 2, 4) / 2;
                    break;
                case 15:
                    progress = progress * progress * progress * progress * progress;
                    break;
                case 16:
                    progress = 1 - Mathf.Pow(1 - progress, 5);
                    break;
                case 17:
                    progress = progress < 0.5 ? 16 * progress * progress * progress * progress * progress : 1 - Mathf.Pow(-2 * progress + 2, 5) / 2;
                    break;
                case 18:
                    progress = progress == 0 ? 0 : Mathf.Pow(2, 10 * progress - 10);
                    break;
                case 19:
                    progress = Math.Abs(progress - 1) < 0.0001f ? 1 : 1 - Mathf.Pow(2, -10 * progress);
                    break;
                case 20:
                    progress = progress == 0
                        ? 0
                        : Math.Abs(progress - 1) < 0.0001f
                            ? 1
                            : progress < 0.5
                                ? Mathf.Pow(2, 20 * progress - 10) / 2
                                : (2 - Mathf.Pow(2, -20 * progress + 10)) / 2;
                    break;
                case 21:
                    progress = 1 - Mathf.Sqrt(1 - Mathf.Pow(progress, 2));
                    break;
                case 22:
                    progress = Mathf.Sqrt(1 - Mathf.Pow(progress - 1, 2));
                    break;
                case 23:
                    progress = progress < 0.5
                        ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * progress, 2))) / 2
                        : (Mathf.Sqrt(1 - Mathf.Pow(-2 * progress + 2, 2)) + 1) / 2;
                    break;
                case 24:
                    progress = c3 * progress * progress * progress - c1 * progress * progress;
                    break;
                case 25:
                    progress = 1 + c3 * Mathf.Pow(progress - 1, 3) + c1 * Mathf.Pow(progress - 1, 2);
                    break;
                case 26:
                    progress = progress < 0.5
                        ? (Mathf.Pow(2 * progress, 2) * ((c2 + 1) * 2 * progress - c2)) / 2
                        : (Mathf.Pow(2 * progress - 2, 2) * ((c2 + 1) * (progress * 2 - 2) + c2) + 2) / 2;
                    break;
                case 27:
                    progress = progress == 0
                        ? 0
                        : Math.Abs(progress - 1) < 0.0001f
                            ? 1
                            : -Mathf.Pow(2, 10 * progress - 10) * Mathf.Sin((progress * 10 - 10.75f) * c4);
                    break;
                case 28:
                    progress = progress == 0
                        ? 0
                        : Math.Abs(progress - 1) < 0.0001f
                            ? 1
                            : Mathf.Pow(2, -10 * progress) * Mathf.Sin((progress * 10 - 0.75f) * c4) + 1;
                    break;
                case 29:
                    progress = progress == 0
                        ? 0
                        : Math.Abs(progress - 1) < 0.0001f
                            ? 1
                            : progress < 0.5
                                ? -(Mathf.Pow(2, 20 * progress - 10) * Mathf.Sin((20 * progress - 11.125f) * c5)) / 2
                                : (Mathf.Pow(2, -20 * progress + 10) * Mathf.Sin((20 * progress - 11.125f) * c5)) / 2 + 1;
                    break;
                case 30:
                    progress = 1 - BounceOut(1 - progress);
                    break;
                case 31:
                    progress = BounceOut(progress);
                    break;
                case 32:
                    progress = progress < 0.5
                        ? (1 - BounceOut(1 - 2 * progress)) / 2
                        : (1 + BounceOut(2 * progress - 1)) / 2;
                    break;
                case 33:
                    progress = BezierCubic(progress, 0, 0, 0, 1);
                    break;
                case 34:
                    progress = BezierCubic(progress, 0, 1, 1, 1);
                    break;
                case 35:
                    progress = BezierCubic(progress, 0, 0, 1, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return progress;
        }
    }
}

