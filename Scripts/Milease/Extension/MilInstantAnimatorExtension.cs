using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Milease.Core;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Milease.Exception;
using Milease.Translate;
using Milease.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Milease.Utils
{
    public static class MilInstantAnimatorExtension
    {
        public static MilInstantAnimator AsMileaseKeyEvent(this Action action, float delay = 0f)
            => Milease(action, (_) => action.Invoke(), null, 0f, delay);

        public static MilInstantAnimator AsMileaseHandleFunction(
            this MileaseHandleFunction func,
            float duration,
            float delay = 0f) => Milease(func, func, null, duration, delay);

        public static MilInstantAnimator MileaseAdditive(
            this object target,
            MileaseHandleFunction handleFunction,
            MileaseHandleFunction resetFunction,
            float duration, float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In
        ) => Milease(target, handleFunction, resetFunction, duration, delay, easeFunction, easeType,
            MilAnimation.BlendingMode.Additive);

        public static MilInstantAnimator Milease(
            this object target,
            MileaseHandleFunction handleFunction,
            MileaseHandleFunction resetFunction,
            float duration, float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In,
            MilAnimation.BlendingMode blendingMode = MilAnimation.BlendingMode.Default
        )
        {
            var animator = new MilInstantAnimator();
            var ani = MilAnimation.SimplePart(duration, delay, easeFunction, easeType, blendingMode);
            animator.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new(target, animator, ani, handleFunction, resetFunction)
            });
            return animator;
        }

        public static MilInstantAnimator MileaseTo(
            this object target, string memberName,
            object toValue,
            float duration, float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In)
        {
            var animator = new MilInstantAnimator();
            var type = target.GetType();
            var members = type.GetMember(memberName);
            if (members.Length == 0)
            {
                throw new MilMemberNotFoundException(memberName);
            }

            var info = members[0];
            MileaseHandleFunction handleFunction = null;
            // TODO complex transformation provider manager
            if (ColorTransformation.CanTranslate(info))
            {
                handleFunction = ColorTransformation.MakeTransformation(MilAnimation.BlendingMode.Default);
            }

            animator.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new(target, animator, MilAnimation.SimplePartTo(toValue, duration, delay, easeFunction, easeType),
                    info.MemberType switch
                    {
                        MemberTypes.Field => ((FieldInfo)info).FieldType,
                        MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                        _ => null
                    }, info, handleFunction)
            });
            return animator;
        }

        public static MilInstantAnimator MileaseAdditive(
            this object target, string memberName,
            object startValue,
            float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In) =>
            Milease(target, memberName, startValue, delay, easeFunction, easeType, MilAnimation.BlendingMode.Additive);

        public static MilInstantAnimator Milease(
            this object target, string memberName,
            object startValue,
            float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In,
            MilAnimation.BlendingMode blendingMode = MilAnimation.BlendingMode.Default)
        {
            var animator = new MilInstantAnimator();
            var type = target.GetType();
            var members = type.GetMember(memberName);
            if (members.Length == 0)
            {
                throw new MilMemberNotFoundException(memberName);
            }

            var info = members[0];
            MileaseHandleFunction handleFunction = null;
            // TODO complex transformation provider manager
            if (ColorTransformation.CanTranslate(info))
            {
                handleFunction = ColorTransformation.MakeTransformation(blendingMode);
            }

            animator.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new(target, animator, MilAnimation.SimplePart(startValue, delay, easeFunction, easeType, blendingMode),
                    info.MemberType switch
                    {
                        MemberTypes.Field => ((FieldInfo)info).FieldType,
                        MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                        _ => null
                    }, info, handleFunction)
            });
            return animator;
        }

        public static MilInstantAnimator MileaseAdditive(
            this object target, string memberName,
            object startValue, object toValue,
            float duration, float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In) =>
            Milease(target, memberName, startValue, toValue,
                duration, delay,
                easeFunction, easeType, MilAnimation.BlendingMode.Additive);

        public static MilInstantAnimator Milease(
            this object target, string memberName,
            object startValue, object toValue,
            float duration, float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In,
            MilAnimation.BlendingMode blendingMode = MilAnimation.BlendingMode.Default)
        {
            var animator = new MilInstantAnimator();
            var type = target.GetType();
            var members = type.GetMember(memberName);
            if (members.Length == 0)
            {
                throw new MilMemberNotFoundException(memberName);
            }

            var info = members[0];
            MileaseHandleFunction handleFunction = null;
            // TODO complex transformation provider manager
            if (ColorTransformation.CanTranslate(info))
            {
                handleFunction = ColorTransformation.MakeTransformation(blendingMode);
            }

            animator.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new(target, animator,
                    MilAnimation.SimplePart(startValue, toValue, duration, delay, easeFunction, easeType, blendingMode),
                    info.MemberType switch
                    {
                        MemberTypes.Field => ((FieldInfo)info).FieldType,
                        MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                        _ => null
                    }, info, handleFunction)
            });
            return animator;
        }

        public static MilInstantAnimator Milease(
            this object target, string memberName,
            params MilAnimation.AnimationPart[] animations)
        {
            var animator = new MilInstantAnimator();
            var type = target.GetType();
            var members = type.GetMember(memberName);
            if (members.Length == 0)
            {
                throw new MilMemberNotFoundException(memberName);
            }

            var info = members[0];
            animator.Collection.Add(
                animations.Select(x =>
                    new RuntimeAnimationPart(target, animator, x, info.MemberType switch
                    {
                        MemberTypes.Field => ((FieldInfo)info).FieldType,
                        MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                        _ => null
                    }, info)
                ).ToList()
            );
            return animator;
        }
    }
}