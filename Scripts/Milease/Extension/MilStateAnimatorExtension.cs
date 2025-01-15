using System;
using System.Linq.Expressions;
using Milease.Core;
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
            if (mbExpr.Body is not MemberExpression memberExpr)
            {
                throw new Exception("You must pass in a MemberExpression to construct the animator.");
            }
            
            return new MilStateParameter()
            {
                Target = target,
                Member = memberExpr.Member,
                EaseFunction = easeFunction,
                EaseType = easeType,
                ToValue = toValue
            };
        }
        
        public static MilStateParameter MilState<T, E>(this T target, Expression<Func<T, E>> mbExpr, E toValue, AnimationCurve curve)
        {
            if (mbExpr.Body is not MemberExpression memberExpr)
            {
                throw new Exception("You must pass in a MemberExpression to construct the animator.");
            }
            
            return new MilStateParameter()
            {
                Target = target,
                Member = memberExpr.Member,
                CustomCurve = curve,
                ToValue = toValue
            };
        }
        
        public static MilStateParameter MilState(this object target, string member, object toValue,
            EaseFunction easeFunction = EaseFunction.Quad, EaseType easeType = EaseType.In)
        {
            var members = target.GetType().GetMember(member);
            if (members.Length == 0)
            {
                throw new MilMemberNotFoundException(member);
            }
            
            return new MilStateParameter()
            {
                Target = target,
                Member = members[0],
                EaseFunction = easeFunction,
                EaseType = easeType,
                ToValue = toValue
            };
        }
        
        public static MilStateParameter MilState(this object target, string member, object toValue, AnimationCurve curve)
        {
            var members = target.GetType().GetMember(member);
            if (members.Length == 0)
            {
                throw new MilMemberNotFoundException(member);
            }
            
            return new MilStateParameter()
            {
                Target = target,
                Member = members[0],
                CustomCurve = curve,
                ToValue = toValue
            };
        }
    }
}
