using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Enums;

namespace Milease.DSL
{
    // Short written for MilInstantAnimator.Start
    public static class MAni
    {
        public static MilInstantAnimator Make(MilInstantAnimator animator)
        {
            return MilInstantAnimator.Start(animator);
        }
        
        public static MilInstantAnimator Make(params MilInstantAnimator[] animators)
        {
            return MilInstantAnimator.Start(animators);
        }

        private static bool TryParseExprValue(Expression expr, out object val)
        {
            object target = null;
            switch (expr)
            {
                case ConstantExpression constant:
                    val = constant.Value;
                    return true;
                case MemberExpression { Member: FieldInfo fieldInfo } expression:
                    if (expression.Expression != null)
                    {
                        if (!TryParseExprValue(expression.Expression, out target))
                        {
                            throw new Exception();
                        }
                    }
                    val = fieldInfo.GetValue(target);
                    return true;
                case MemberExpression { Member: PropertyInfo propInfo } expression:
                    if (expression.Expression != null)
                    {
                        if (!TryParseExprValue(expression.Expression, out target))
                        {
                            throw new Exception();
                        }
                    }
                    val = propInfo.GetValue(target);
                    return true;
                case BinaryExpression binary:
                    if (!TryParseExprValue(binary.Left, out var left) 
                        || !TryParseExprValue(binary.Right, out var right) 
                        || binary.Method == null)
                    {
                        throw new Exception();
                    }

                    val = binary.Method.Invoke(null, new object[] { left, right });
                    return true;
                case MethodCallExpression methodCall:
                    var arguments = methodCall.Arguments.Select(x =>
                        {
                            if (!TryParseExprValue(x, out var arg))
                            {
                                throw new Exception();
                            }
                            return arg;
                        }
                    );
                    object instance = null;
                    if (methodCall.Object != null && !TryParseExprValue(methodCall.Object, out instance))
                    {
                        throw new Exception();
                    }
                    val = methodCall.Method.Invoke(instance, arguments.ToArray());
                    return true;
                case NewExpression newExpression:
                    var newArgs = newExpression.Arguments.Select(x =>
                        {
                            if (!TryParseExprValue(x, out var arg))
                            {
                                throw new Exception();
                            }
                            return arg;
                        }
                    );
                    val = newExpression.Constructor.Invoke(newArgs.ToArray());
                    return true;
                default:
                    val = null;
                    return false;
            }
        }
        
        public static bool Ease(EaseFunction func, EaseType type = EaseType.In)
        {
            return true;
        }

        private struct StateParseResult
        {
            public object Target;
            public MemberInfo Member;
            public object Value;
            public EasePress Ease;
            
            public struct EasePress
            {
                public EaseFunction EaseFunction;
                public EaseType EaseType;
            }
        }
        
        public static IEnumerable<MilStateParameter> States(Expression<Func<bool>> stateExpr)
        {
            var stack = new Stack<BinaryExpression>();
            var styleSet = new List<StateParseResult>();
            var easeMap = new Dictionary<BinaryExpression, StateParseResult.EasePress>();
            
            if (stateExpr.Body is BinaryExpression body)
            {
                stack.Push(body);
                while (stack.Count > 0)
                {
                    var subExpr = stack.Pop();
                    if (subExpr.Left is MemberExpression mbExpr)
                    {
                        object target = null;
                        if (!TryParseExprValue(subExpr.Right, out var val))
                        {
                            throw new Exception($"Style value for '{mbExpr.Member.Name}' is invalid.");
                        }

                        if (mbExpr.Expression != null)
                        {
                            _ = TryParseExprValue(mbExpr.Expression, out target);
                        }
                        
                        if (!easeMap.TryGetValue(subExpr, out var easeSet))
                        {
                            easeSet = new StateParseResult.EasePress()
                            {
                                EaseFunction = EaseFunction.Quad,
                                EaseType = EaseType.In
                            };
                        }
                        
                        styleSet.Add(new StateParseResult()
                        {
                            Target = target,
                            Member = mbExpr.Member,
                            Value = val,
                            Ease = easeSet
                        });
                        continue;
                    }

                    if (subExpr.NodeType == ExpressionType.AndAlso)
                    {
                        if (subExpr.Right is BinaryExpression be2
                            && subExpr.Left is BinaryExpression be1)
                        {
                            stack.Push(be2);
                            stack.Push(be1);
                        }
                        else
                        {
                            throw new Exception("Unknown style expression.");
                        }
                    } 
                    else if (subExpr.NodeType == ExpressionType.OrElse)
                    {
                        if (subExpr.Left is BinaryExpression be1)
                        {
                            stack.Push(be1);
                            if (subExpr.Right is MethodCallExpression methodCall)
                            {
                                var realArgs = methodCall.Arguments.Select(x =>
                                {
                                    if (!TryParseExprValue(x, out var arg))
                                    {
                                        throw new Exception($"Ease settings is invalid.");
                                    }

                                    return arg;
                                }).ToArray();
                                
                                if (realArgs.Length == 1)
                                {
                                    easeMap.Add(be1, new StateParseResult.EasePress()
                                    {
                                        EaseFunction = (EaseFunction)realArgs[0],
                                        EaseType = EaseType.In
                                    });
                                } 
                                else if (realArgs.Length == 2)
                                {
                                    easeMap.Add(be1, new StateParseResult.EasePress()
                                    {
                                        EaseFunction = (EaseFunction)realArgs[0],
                                        EaseType = (EaseType)realArgs[1]
                                    });
                                }
                            }
                            else
                            {
                                throw new Exception("Unknown style expression.");
                            }
                        }
                        else
                        {
                            throw new Exception("Unknown style expression.");
                        }
                    }
                }
            }
            
            return styleSet.Select(x =>
            {
                var memType = x.Member.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)x.Member).FieldType,
                    MemberTypes.Property => ((PropertyInfo)x.Member).PropertyType,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var type = typeof(MilStateAnimation.AnimationValue<,>).MakeGenericType(x.Target.GetType(), memType);
                var ret = (MilStateParameter)Activator.CreateInstance(type, x.Target, x.Member, x.Value);
                ret.EaseFunction = x.Ease.EaseFunction;
                ret.EaseType = x.Ease.EaseType;
                return ret;
            });
        }
    }
}
