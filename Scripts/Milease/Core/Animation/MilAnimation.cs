using System;
using System.Collections.Generic;
using System.Linq;
using Milease.Enums;
using UnityEngine;

namespace Milease.Core.Animation
{
    public class MilAnimation : ScriptableObject
    {
        public enum BlendingMode
        {
            Default, Additive
        }
        [Serializable]
        public class AnimationPart
        {
            public BlendingMode BlendingMode = BlendingMode.Default;
            public float StartTime;
            public float Duration;
            public EaseType EaseType;
            public EaseFunction EaseFunction;
            public AnimationCurve CustomCurve;
            public List<string> Binding;
            public string StartValue;
            public string ToValue;
            public bool PendingTo = false;
        }
        [HideInInspector]
        public List<AnimationPart> Parts;

        public static AnimationPart PartAdditive(string binding, object startValue, object toValue, float startTime,
            float duration, EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
            => Part(binding, startValue, toValue, startTime, duration, easeFunction, easeType, BlendingMode.Additive);
        
        public static AnimationPart Part(string binding, object startValue, object toValue, float startTime, float duration,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            var type = startValue.GetType();
            return new AnimationPart()
            {
                BlendingMode = blendingMode,
                StartTime = startTime,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction,
                Binding = binding.Split('.').ToList(),
                StartValue = type.IsPrimitive ? startValue.ToString() : JsonUtility.ToJson(startValue),
                ToValue = type.IsPrimitive ? toValue.ToString() : JsonUtility.ToJson(toValue)
            };
        }

        public static AnimationPart SimplePartTo(object toValue, float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            var type = toValue.GetType();
            return new AnimationPart()
            {
                BlendingMode = blendingMode,
                StartTime = delay,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction,
                ToValue = type.IsPrimitive ? toValue.ToString() : JsonUtility.ToJson(toValue),
                PendingTo = true
            };
        }
        
        public static AnimationPart SimplePartAdditive(object startValue, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
            => SimplePart(startValue, delay, easeFunction, easeType, BlendingMode.Additive);
        
        public static AnimationPart SimplePart(object startValue, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            var type = startValue.GetType();
            return new AnimationPart()
            {
                BlendingMode = blendingMode,
                StartTime = delay,
                Duration = 0f,
                EaseType = easeType,
                EaseFunction = easeFunction,
                StartValue = type.IsPrimitive || type == typeof(string) ? startValue.ToString() : JsonUtility.ToJson(startValue),
                ToValue = type.IsPrimitive || type == typeof(string) ? startValue.ToString() : JsonUtility.ToJson(startValue)
            };
        }
        
        public static AnimationPart SimplePartAdditive(object startValue, object toValue, float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
            => SimplePart(startValue, toValue, duration, delay, easeFunction, easeType, BlendingMode.Additive);
        
        public static AnimationPart SimplePart(object startValue, object toValue, float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            var type = startValue.GetType();
            return new AnimationPart()
            {
                BlendingMode = blendingMode,
                StartTime = delay,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction,
                StartValue = type.IsPrimitive || type == typeof(string) ? startValue.ToString() : JsonUtility.ToJson(startValue),
                ToValue = type.IsPrimitive || type == typeof(string) ? toValue.ToString() : JsonUtility.ToJson(toValue)
            };
        }

        internal static AnimationPart SimplePart(float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, BlendingMode blendingMode = BlendingMode.Default)
        {
            return new AnimationPart()
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