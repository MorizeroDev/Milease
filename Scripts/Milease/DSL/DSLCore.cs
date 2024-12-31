using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Milease.Core;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Translate;

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
            T target, System.Linq.Expressions.Expression<Func<T, E>> mbExpr, 
            AniExpression<E> aniExpr,
            EaseType easeType, EaseFunction easeFunction
        )
        {
            if (mbExpr.Body is not MemberExpression memberExpr)
            {
                throw new Exception("You must pass in a MemberExpression to construct the animator.");
            }
            
            var animator = new MilInstantAnimator();
            var info = memberExpr.Member;
            
            MileaseHandleFunction handleFunction = null;
            // TODO complex transformation provider manager
            if (ColorTransformation.CanTranslate(info))
            {
                handleFunction = ColorTransformation.MakeTransformation(aniExpr.BlendingMode);
            }

            var animationPart = aniExpr.ToOnly
                ? MilAnimation.SimplePartTo(aniExpr.To, aniExpr.Duration, aniExpr.StartTime, easeFunction, easeType)
                : MilAnimation.SimplePart(aniExpr.From, aniExpr.To, aniExpr.Duration,
                    aniExpr.StartTime, easeFunction, easeType, aniExpr.BlendingMode);
            
            animator.Collection.Add(new List<RuntimeAnimationPart>()
            {
                new(target, animator,
                    animationPart,
                    info.MemberType switch
                    {
                        MemberTypes.Field => ((FieldInfo)info).FieldType,
                        MemberTypes.Property => ((PropertyInfo)info).PropertyType,
                        _ => null
                    }, info, handleFunction)
            });

            return animator;
        }
    }
}
