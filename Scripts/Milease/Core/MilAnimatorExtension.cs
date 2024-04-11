using System;
using System.Diagnostics;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace Milease.Core
{
    public static class MilAnimatorExtension
    {
        public static MilSimpleAnimator.MilSimpleAnimation MilAnimate(this object target, string memberName, params MilAnimation.AnimationPart[] animations)
        {
            var animation = new MilSimpleAnimator.MilSimpleAnimation();
            var type = target.GetType();
            var info = type.GetMember(memberName)[0];
            foreach (var ani in animations)
            {
                animation.Collection.Add(new MilAnimation.RuntimeAnimationPart(target, ani, info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType,
                    MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                    _ => null
                }, info));
            }
            MilSimpleAnimator.Instance.Animations.Add(animation);
            return animation;
        }
    }
}