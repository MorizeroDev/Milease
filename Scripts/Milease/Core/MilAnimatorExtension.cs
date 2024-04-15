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
        public static MilSimpleAnimation MileaseAdditive(this object target, MileaseHandleFunction handleFunction,
            MileaseHandleFunction resetFunction, float duration, float delay = 0f,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
            => Milease(target, handleFunction, resetFunction, duration, delay, easeFunction, easeType,
                MilAnimation.BlendingMode.Additive);
        public static MilSimpleAnimation Milease(this object target, MileaseHandleFunction handleFunction, 
            MileaseHandleFunction resetFunction, float duration, float delay = 0f, 
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, 
            MilAnimation.BlendingMode blendingMode = MilAnimation.BlendingMode.Default)
        {
            var animation = new MilSimpleAnimation();
            var ani = MilAnimation.SimplePart(handleFunction, resetFunction, duration, delay, easeFunction, easeType, blendingMode);
            animation.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new (target, ani, handleFunction, resetFunction)
            });
            return animation;
        }
        
        public static MilSimpleAnimation MileaseTo(this object target, string memberName, object toValue, 
            float duration, float delay = 0f, EaseFunction easeFunction = EaseFunction.Quad, 
            EaseType easeType = EaseType.In)
        {
            var animation = new MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            animation.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new (target, MilAnimation.SimplePartTo(toValue, duration, delay, easeFunction, easeType), info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType,
                    MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                    _ => null
                }, info)
            });
            return animation;
        }
        
        public static MilSimpleAnimation MileaseAdditive(this object target, string memberName, object startValue, 
            float delay = 0f, EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
            => Milease(target, memberName, startValue, delay, easeFunction, easeType,
                MilAnimation.BlendingMode.Additive);
        
        public static MilSimpleAnimation Milease(this object target, string memberName, object startValue, 
            float delay = 0f, EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In, 
            MilAnimation.BlendingMode blendingMode = MilAnimation.BlendingMode.Default)
        {
            var animation = new MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            animation.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new (target, MilAnimation.SimplePart(startValue, delay, easeFunction, easeType, blendingMode), info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType,
                    MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                    _ => null
                }, info)
            });
            return animation;
        }
        
        public static MilSimpleAnimation MileaseAdditive(this object target, string memberName, object startValue, 
            object toValue, float duration, float delay = 0f, EaseFunction easeFunction = EaseFunction.Quad, 
            EaseType easeType = EaseType.In)
            => Milease(target, memberName, startValue, toValue, duration, delay, easeFunction, easeType,
                MilAnimation.BlendingMode.Additive);
        
        public static MilSimpleAnimation Milease(this object target, string memberName, object startValue, 
            object toValue, float duration, float delay = 0f, EaseFunction easeFunction = EaseFunction.Quad, 
            EaseType easeType = EaseType.In, MilAnimation.BlendingMode blendingMode = MilAnimation.BlendingMode.Default)
        {
            var animation = new MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            animation.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new (target, MilAnimation.SimplePart(startValue, toValue, duration, delay, easeFunction, easeType, blendingMode), info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType,
                    MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                    _ => null
                }, info)
            });
            return animation;
        }
        
        public static MilSimpleAnimation Milease(this object target, string memberName, params MilAnimation.AnimationPart[] animations)
        {
            var animation = new MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            animation.Collection.Add(
                animations.Select(x => 
                        new RuntimeAnimationPart(target, x, info.MemberType switch
                        {
                            MemberTypes.Field => ((FieldInfo)info).FieldType,
                            MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                            _ => null
                        }, info)
                    ).ToList()
                );
            return animation;
        }
    }
}