#if MILEASE_ENABLE_EXPRESSION
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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

        internal static readonly List<ExprMetaData> MetaData = new();
        
        private static readonly Dictionary<(Type, string), object> expressionMap = new();
        private static readonly Dictionary<(Type, string), object> offsetExpressionMap = new();
        private static readonly Dictionary<(Type, string), object> valueGetterMap = new();
        private static readonly Dictionary<(Type, string), object> valueSetterMap = new();

        public static void WarmUp<T, E>(Expression<Func<T, E>> mbExpr)
        {
            if (mbExpr.Body is not MemberExpression memberExpr)
            {
                throw new Exception("You must pass in a MemberExpression to construct the animator.");
            }
            _ = GetExprWithOffset<T, E>(memberExpr);
            _ = GetExpr<T, E>(memberExpr);
            _ = GetValSetter<T, E>(memberExpr);
            _ = GetValGetter<T, E>(memberExpr);
        }
        
        internal static OffsetAnimatorExpression<T, E> GetExprWithOffset<T, E>(MemberExpression memberExpr)
        {
            var key = (typeof(T), memberExpr.Member.Name);
            if (offsetExpressionMap.TryGetValue(key, out var expr))
            {
                return (OffsetAnimatorExpression<T, E>)expr;
            }

            expr = MakeExprWithOffset<T, E>(memberExpr);
            offsetExpressionMap.Add(key, expr);
            return (OffsetAnimatorExpression<T, E>)expr;
        }

        private static OffsetAnimatorExpression<T, E> MakeExprWithOffset<T, E>(MemberExpression memberExpr)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberExpr.Member);
        
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
        
        internal static AnimatorExpression<T, E> GetExpr<T, E>(MemberExpression memberExpr)
        {
            var key = (typeof(T), memberExpr.Member.Name);
            if (expressionMap.TryGetValue(key, out var expr))
            {
                return (AnimatorExpression<T, E>)expr;
            }

            expr = MakeExpr<T, E>(memberExpr);
            expressionMap.Add(key, expr);
            
#if UNITY_EDITOR
            MetaData.Add(new ExprMetaData()
            {
                TName = typeof(T).FullName,
                EName = typeof(E).FullName,
                MemberName = memberExpr.Member.Name
            });
#endif
            
            return (AnimatorExpression<T, E>)expr;
        }

        private static AnimatorExpression<T, E> MakeExpr<T, E>(MemberExpression memberExpr)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberExpr.Member);
        
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
        
        internal static Func<T, E> GetValGetter<T, E>(MemberExpression memberExpr)
        {
            var key = (typeof(T), memberExpr.Member.Name);
            if (valueGetterMap.TryGetValue(key, out var expr))
            {
                return (Func<T, E>)expr;
            }

            expr = MakeGetter<T, E>(memberExpr);
            valueGetterMap.Add(key, expr);
            return (Func<T, E>)expr;
        }
        
        private static Func<T, E> MakeGetter<T, E>(MemberExpression memberExpr)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberExpr.Member);
            
            return Expression.Lambda<Func<T, E>>(mutableMemberExpr, targetParam).Compile();
        }
        
        internal static Action<T, E> GetValSetter<T, E>(MemberExpression memberExpr)
        {
            var key = (typeof(T), memberExpr.Member.Name);
            if (valueSetterMap.TryGetValue(key, out var expr))
            {
                return (Action<T, E>)expr;
            }

            expr = MakeSetter<T, E>(memberExpr);
            valueSetterMap.Add(key, expr);
            return (Action<T, E>)expr;
        }
        
        private static Action<T, E> MakeSetter<T, E>(MemberExpression memberExpr)
        {
            var targetParam = Expression.Parameter(typeof(T), "target");
            var mutableMemberExpr = Expression.MakeMemberAccess(targetParam, memberExpr.Member);
            var valParam = Expression.Parameter(typeof(E), "value");
            
            return Expression.Lambda<Action<T, E>>(
                    Expression.Assign(mutableMemberExpr, valParam), 
                    targetParam, valParam).Compile();
        }
    }
}
#endif
