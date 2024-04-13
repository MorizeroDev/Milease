using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Milease.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Milease.Core
{
    public static class MilAnimatorExtension
    {
        public static MilSimpleAnimator.MilSimpleAnimation Milease(this object target, string memberName, object toValue, float duration, float delay = 0f, EaseUtility.EaseType easeType = EaseUtility.EaseType.In, EaseUtility.EaseFunction easeFunction = EaseUtility.EaseFunction.Quad)
        {
            var animation = new MilSimpleAnimator.MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            var ani = MilAnimation.SimplePart(toValue, duration, delay, easeType, easeFunction);
            var startValue = info.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)info).GetValue(target),
                MemberTypes.Property => ((PropertyInfo)info).GetValue(target),
                _ => 0
            };
            ani.StartValue = type.IsPrimitive ? startValue.ToString() : JsonUtility.ToJson(startValue);
            animation.Collection.Add(new List<MilAnimation.RuntimeAnimationPart>()
            {
                new (target, ani, info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType,
                    MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                    _ => null
                }, info)
            });
            MilSimpleAnimator.Instance.Animations.Add(animation);
            return animation;
        }
        
        public static MilSimpleAnimator.MilSimpleAnimation Milease(this object target, string memberName, object startValue, float delay = 0f, EaseUtility.EaseType easeType = EaseUtility.EaseType.In, EaseUtility.EaseFunction easeFunction = EaseUtility.EaseFunction.Quad)
        {
            var animation = new MilSimpleAnimator.MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            animation.Collection.Add(new List<MilAnimation.RuntimeAnimationPart>()
            {
                new (target, MilAnimation.SimplePart(startValue, delay, easeType, easeFunction), info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType,
                    MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                    _ => null
                }, info)
            });
            MilSimpleAnimator.Instance.Animations.Add(animation);
            return animation;
        }
        
        public static MilSimpleAnimator.MilSimpleAnimation Milease(this object target, string memberName, object startValue, object toValue, float duration, float delay = 0f, EaseUtility.EaseType easeType = EaseUtility.EaseType.In, EaseUtility.EaseFunction easeFunction = EaseUtility.EaseFunction.Quad)
        {
            var animation = new MilSimpleAnimator.MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            animation.Collection.Add(new List<MilAnimation.RuntimeAnimationPart>()
            {
                new (target, MilAnimation.SimplePart(startValue, toValue, duration, delay, easeType, easeFunction), info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType,
                    MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                    _ => null
                }, info)
            });
            MilSimpleAnimator.Instance.Animations.Add(animation);
            return animation;
        }
        
        public static MilSimpleAnimator.MilSimpleAnimation Milease(this object target, string memberName, params MilAnimation.AnimationPart[] animations)
        {
            var animation = new MilSimpleAnimator.MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            animation.Collection.Add(
                animations.Select(x => 
                        new MilAnimation.RuntimeAnimationPart(target, x, info.MemberType switch
                        {
                            MemberTypes.Field => ((FieldInfo)info).FieldType,
                            MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                            _ => null
                        }, info)
                    ).ToList()
                );
            MilSimpleAnimator.Instance.Animations.Add(animation);
            return animation;
        }
    }
}