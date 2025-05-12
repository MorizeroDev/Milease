using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Milease.Core;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Enums;
#if COLOR_TOOL_SETUP && (NET_STANDARD_2_1 || POLYFILL_SETUP)
using Milease.Translate;
#endif
using Milease.Utils;

namespace Milease.DSL
{
    public static partial class DSL
    {
        public static AniExpression<T> ToThis<T>(this T obj)
        {
            return new AniExpression<T>()
            {
                To = obj,
                ToOnly = true
            };
        }
        
        public static AniExpression<T> To<T>(this T obj, T to)
        {
            return new AniExpression<T>()
            {
                From = obj,
                To = to
            };
        }
        
        public static AniExpression<T> AsInstant<T>(this T obj)
        {
            return new AniExpression<T>()
            {
                From = obj,
                To = obj
            };
        }
        
        public static MilInstantAnimator Milease<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
            => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Quad);
        
        public static MilInstantAnimator Milease<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
            => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Quad);
        
        private static MilInstantAnimator generate<T, E>(
            T target, Expression<Func<T, E>> mbExpr, 
            AniExpression<E> aniExpr,
            EaseType easeType, EaseFunction easeFunction
        )
        {
            MemberInfo memberInfo = null;
            if (mbExpr.Body is MemberExpression memberExpr)
            {
                memberInfo = memberExpr.Member;
            }
            else if (mbExpr.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression propExpr)
            {
                memberInfo = propExpr.Member;
            }
            else
            {
                throw new Exception("You must pass in a MemberExpression to construct the animator.");
            }
            
#if UNITY_EDITOR
            var memberType = memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => throw new ArgumentException(nameof(mbExpr))
            };
            if (memberType != typeof(E))
            {
                LogUtils.Throw($"Type mismatch, expected '{memberType.FullName}', actual '{typeof(E).FullName}'");
            }
#endif
            
            var animator = new MilInstantAnimator();
            
            MileaseHandleFunction<T, E> handleFunction = null;
            
#if COLOR_TOOL_SETUP && (NET_STANDARD_2_1 || POLYFILL_SETUP)
            // TODO complex transformation provider manager
            if (ColorTransformation.CanTranslate<E>())
            {
                handleFunction = ColorTransformation.MakeTransformation<T, E>(aniExpr.BlendingMode);
            }
#endif

            var animationPart = aniExpr.ToOnly
                ? MilAnimation.SimplePartTo(aniExpr.To, aniExpr.Duration, aniExpr.StartTime, easeFunction, easeType)
                : MilAnimation.SimplePart(aniExpr.From, aniExpr.To, aniExpr.Duration,
                    aniExpr.StartTime, easeFunction, easeType, aniExpr.BlendingMode);
            
            animator.Collection.Add(new List<IAnimationController>()
            {
                new RuntimeAnimationPart<T,E>(target, animator, animationPart, memberInfo, handleFunction)
            });

            return animator;
        }
    }
}
