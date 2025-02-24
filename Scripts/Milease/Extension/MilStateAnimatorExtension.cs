using System;
using System.Linq.Expressions;
using System.Reflection;
using Milease.Core;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Milease.Exception;
using UnityEngine;

namespace Milease.Utils
{
    public static class MilStateAnimatorExtension
    {
        public static MilStateParameter MilState<T, E>(this T target, Expression<Func<T, E>> mbExpr, E toValue,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
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
            
            return new MilStateAnimation.AnimationValue<T, E>(target, memberInfo, toValue)
            {
                EaseFunction = easeFunction,
                EaseType = easeType
            };
        }
        
        public static MilStateParameter MilState<T, E>(this T target, Expression<Func<T, E>> mbExpr, E toValue, AnimationCurve curve)
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
            
            return new MilStateAnimation.AnimationValue<T, E>(target, memberInfo, toValue)
            {
                CustomCurve = curve
            };
        }
    }
}
