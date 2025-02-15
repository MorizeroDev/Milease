using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
        {
            var animator = new MilInstantAnimator();
            var animationPart = MilAnimation.SimplePart(action, delay);
            animator.Collection.Add(new List<IAnimationController>()
            {
                new RuntimeAnimationPart<Action, Action>(default, animator, animationPart, 
                    (_) => action.Invoke(), 
                    null)
            });

            return animator;
        }

        public static MilInstantAnimator AsMileaseHandleFunction<E>(
            this MileaseHandleFunction<E, E> func,
            float duration,
            float delay = 0f)        
        {
            var animator = new MilInstantAnimator();
            var animationPart = MilAnimation.SimplePart<E>(duration, delay);
            animator.Collection.Add(new List<IAnimationController>()
            {
                new RuntimeAnimationPart<E, E>(default, animator, animationPart, func, null)
            });
            return animator;
        }

        public static MilInstantAnimator Milease<T>(
            this T target,
            MileaseHandleFunction<T, T> handleFunction,
            MileaseHandleFunction<T, T> resetFunction,
            float duration, float delay = 0f,
            ////////
            EaseFunction easeFunction = EaseFunction.Quad,
            EaseType easeType = EaseType.In,
            MilAnimation.BlendingMode blendingMode = MilAnimation.BlendingMode.Default
        )
        {
            var animator = new MilInstantAnimator();
            var ani = MilAnimation.SimplePart<T>(duration, delay, easeFunction, easeType, blendingMode);
            animator.Collection.Add(new List<IAnimationController>()
            {
                new RuntimeAnimationPart<T, T>(target, animator, ani, handleFunction, resetFunction)
            });
            return animator;
        }
    }
}
