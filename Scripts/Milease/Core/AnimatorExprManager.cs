#if MILEASE_ENABLE_EXPRESSION
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Milease.Core
{
    internal delegate void AnimatorExpression<in T, in E>(T target, E start, E end, float progress);
    internal delegate void OffsetAnimatorExpression<in T, in E>(T target, E start, E end, float progress, E offset);
    
    public static class AnimatorExprManager
    {
        internal class ExprMetaData
        {
            public string TName, EName, MemberName;
        }

        internal static readonly List<ExprMetaData> MetaData = new List<ExprMetaData>();
        
        private static readonly Dictionary<(Type, string), object> expressionMap = new Dictionary<(Type, string), object>();
        private static readonly Dictionary<(Type, string), object> offsetExpressionMap = new Dictionary<(Type, string), object>();
        private static readonly Dictionary<(Type, string), object> valueGetterMap = new Dictionary<(Type, string), object>();
        private static readonly Dictionary<(Type, string), object> valueSetterMap = new Dictionary<(Type, string), object>();

        public static void WarmUp<T, E>(Expression<Func<T, E>> mbExpr)
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
            _ = GetExprWithOffset<T, E>(memberInfo);
            _ = GetExpr<T, E>(memberInfo);
            _ = GetValSetter<T, E>(memberInfo);
            _ = GetValGetter<T, E>(memberInfo);
        }
        
        internal static OffsetAnimatorExpression<T, E> GetExprWithOffset<T, E>(MemberInfo memberInfo)
        {
            var key = (typeof(T), memberInfo.Name);
            if (offsetExpressionMap.TryGetValue(key, out var expr))
            {
                return (OffsetAnimatorExpression<T, E>)expr;
            }

            expr = MakeExprWithOffset<T, E>(memberInfo);
            offsetExpressionMap.Add(key, expr);
            return (OffsetAnimatorExpression<T, E>)expr;
        }

        private static OffsetAnimatorExpression<T, E> MakeExprWithOffset<T, E>(MemberInfo memberInfo)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberInfo);
        
            var startParam = Expression.Parameter(typeof(E), "start");
            var endParam = Expression.Parameter(typeof(E), "end");
            var offsetParam = Expression.Parameter(typeof(E), "offset");
            
            var progressParam = Expression.Parameter(typeof(float), "progress");
            
            try
            {
                var callExpr = 
                    Expression.Assign(
                        mutableMemberExpr,
                        Expression.Add(
                            Expression.Add(startParam, 
                                Expression.Multiply(Expression.Subtract(endParam, startParam), progressParam)
                            ),
                            offsetParam
                        )
                    );
                
                return Expression.Lambda<OffsetAnimatorExpression<T, E>>
                        (callExpr, targetParam, startParam, endParam, progressParam, offsetParam)
                        .Compile();
            }
            catch
            {
                // ignored
                return null;
            }
        }
        
        internal static AnimatorExpression<T, E> GetExpr<T, E>(MemberInfo memberInfo)
        {
            var key = (typeof(T), memberInfo.Name);
            if (expressionMap.TryGetValue(key, out var expr))
            {
                return (AnimatorExpression<T, E>)expr;
            }

            expr = MakeExpr<T, E>(memberInfo);
            expressionMap.Add(key, expr);
            
#if UNITY_EDITOR
            MetaData.Add(new ExprMetaData()
            {
                TName = typeof(T).FullName,
                EName = typeof(E).FullName,
                MemberName = memberInfo.Name
            });
#endif
            
            return (AnimatorExpression<T, E>)expr;
        }

        private static AnimatorExpression<T, E> MakeExpr<T, E>(MemberInfo memberInfo)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberInfo);
        
            var startParam = Expression.Parameter(typeof(E), "start");
            var endParam = Expression.Parameter(typeof(E), "end");
        
            var progressParam = Expression.Parameter(typeof(float), "progress");
            
            try
            {
                var callExpr = 
                    Expression.Assign(
                        mutableMemberExpr,
                        Expression.Add(startParam, 
                            Expression.Multiply(Expression.Subtract(endParam, startParam), progressParam)
                        )
                    );
                
                return Expression.Lambda<AnimatorExpression<T, E>>
                        (callExpr, targetParam, startParam, endParam, progressParam)
                        .Compile();
            }
            catch
            {
                var defaultExpr =
                    Expression.IfThenElse(
                        Expression.GreaterThanOrEqual(progressParam, Expression.Constant(1f)),
                        Expression.Assign(mutableMemberExpr, endParam),
                        Expression.Assign(mutableMemberExpr, startParam)
                    );
                return Expression.Lambda<AnimatorExpression<T, E>>
                        (defaultExpr, targetParam, startParam, endParam, progressParam)
                        .Compile();
            }
        }
        
        internal static Func<T, E> GetValGetter<T, E>(MemberInfo memberInfo)
        {
            var key = (typeof(T), memberInfo.Name);
            if (valueGetterMap.TryGetValue(key, out var expr))
            {
                return (Func<T, E>)expr;
            }

            expr = MakeGetter<T, E>(memberInfo);
            valueGetterMap.Add(key, expr);
            return (Func<T, E>)expr;
        }
        
        private static Func<T, E> MakeGetter<T, E>(MemberInfo memberInfo)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberInfo);
            
            return Expression.Lambda<Func<T, E>>(mutableMemberExpr, targetParam).Compile();
        }
        
        internal static Action<T, E> GetValSetter<T, E>(MemberInfo memberInfo)
        {
            var key = (typeof(T), memberInfo.Name);
            if (valueSetterMap.TryGetValue(key, out var expr))
            {
                return (Action<T, E>)expr;
            }

            expr = MakeSetter<T, E>(memberInfo);
            valueSetterMap.Add(key, expr);
            return (Action<T, E>)expr;
        }
        
        private static Action<T, E> MakeSetter<T, E>(MemberInfo memberInfo)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberInfo);
            var valParam = Expression.Parameter(typeof(E), "value");
            
            return Expression.Lambda<Action<T, E>>(
                    Expression.Assign(mutableMemberExpr, valParam), 
                    targetParam, valParam).Compile();
        }
    }
}
#endif
