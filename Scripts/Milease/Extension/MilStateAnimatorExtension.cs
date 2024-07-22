using Milease.Core;
using Milease.Core.Animator;
using Milease.Enums;
using UnityEngine;

namespace Milease.Utils
{
    public static class MilStateAnimatorExtension
    {
        public static MilStateParameter MilState(this object target, string member, object toValue,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
        {
            return new MilStateParameter()
            {
                Target = target,
                Member = member,
                EaseFunction = easeFunction,
                EaseType = easeType,
                ToValue = toValue
            };
        }
        
        public static MilStateParameter MilState(this object target, string member, object toValue, AnimationCurve curve)
        {
            return new MilStateParameter()
            {
                Target = target,
                Member = member,
                CustomCurve = curve,
                ToValue = toValue
            };
        }
    }
}