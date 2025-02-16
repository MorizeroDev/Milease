using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Milease.Enums;
using UnityEngine;

namespace Milease.Core.Animation
{
    public enum BlendingMode
    {
        Default, Additive
    }
    
    public class MilAnimation : ScriptableObject
    {
        public abstract class AnimationPartBase
        {
            public BlendingMode BlendingMode = BlendingMode.Default;
            public float StartTime;
            public float Duration;
            public EaseType EaseType;
            public EaseFunction EaseFunction;
            public AnimationCurve CustomCurve;
            public bool PendingTo = false;
        }
        
        [Serializable]
        public class AnimationPart<E> : AnimationPartBase
        {
            public E StartValue;
            public E ToValue;
        }
        
        public static AnimationPart<E> SimplePartTo<E>(E toValue, float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            return new AnimationPart<E>()
            {
                BlendingMode = blendingMode,
                StartTime = delay,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction,
                ToValue = toValue,
                PendingTo = true
            };
        }
        
        public static AnimationPart<E> SimplePartAdditive<E>(E startValue, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
            => SimplePart<E>(startValue, delay, easeFunction, easeType, BlendingMode.Additive);
        
        public static AnimationPart<E> SimplePart<E>(E startValue, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            return new AnimationPart<E>()
            {
                BlendingMode = blendingMode,
                StartTime = delay,
                Duration = 0f,
                EaseType = easeType,
                EaseFunction = easeFunction,
                StartValue = startValue,
                ToValue = startValue
            };
        }
        
        public static AnimationPart<E> SimplePartAdditive<E>(E startValue, E toValue, float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
            => SimplePart<E>(startValue, toValue, duration, delay, easeFunction, easeType, BlendingMode.Additive);
        
        public static AnimationPart<E> SimplePart<E>(E startValue, E toValue, float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            return new AnimationPart<E>()
            {
                BlendingMode = blendingMode,
                StartTime = delay,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction,
                StartValue = startValue,
                ToValue = toValue
            };
        }

        internal static AnimationPart<E> SimplePart<E>(float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            return new AnimationPart<E>()
            {
                BlendingMode = blendingMode,
                StartTime = delay,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction
            };
        }
    }
}
