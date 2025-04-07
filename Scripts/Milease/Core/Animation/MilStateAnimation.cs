using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milease.CodeGen;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Milease.Exception;
using Milease.Utils;
using UnityEngine;

namespace Milease.Core.Animation
{
    public static class MilStateAnimation
    {
        [Serializable]
        public class AnimationValue<T, E> : MilStateParameter
        {
            public T Target;
            
            public string Member;
            public E ToValue, StartValue, DeltaValue;

#if MILEASE_ENABLE_EXPRESSION
            private readonly AnimatorExpression<T, E> expression;
#elif MILEASE_ENABLE_CODEGEN
            private readonly CalculateFunction<E> calcFunc;
            private readonly DeltaCalculateFunction<E> deltaFunc;
        
            private readonly MemberInfo BindMember;
#endif
            public readonly Action<T, E> ValueSetter;
            public readonly Func<T, E> ValueGetter;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AnimationValue(T target, MemberInfo member, E toValue)
            {
                Target = target ?? throw new MilTargetNotFoundException();
                Member = member.Name;

                ToValue = toValue;
                MemberHash = target.GetHashCode() + member.Name;
                
#if MILEASE_ENABLE_EXPRESSION
                ValueGetter = AnimatorExprManager.GetValGetter<T, E>(member);
                ValueSetter = AnimatorExprManager.GetValSetter<T, E>(member);
            
                expression = AnimatorExprManager.GetExpr<T, E>(member);
#elif MILEASE_ENABLE_CODEGEN
                BindMember = member;

                calcFunc = RuntimeBridge.GetFunc<E>();
                deltaFunc = RuntimeBridge.GetDeltaFunc<E>();

                if (!RuntimeBridge.TryGetGetter(BindMember.Name, out ValueGetter))
                {
                    ValueGetter = GetValueByReflection;
                    LogUtils.Warning($"Accessors for {typeof(T).FullName}.{member} is not generated, " +
                                     "use Reflection instead, which would lower the performance!\n" +
                                     "Check AccessorGenerationList.cs and include this member, then re-generate code in the 'Milease' menu.");
                }
                
                if (!RuntimeBridge.TryGetSetter(BindMember.Name, out ValueSetter))
                {
                    ValueSetter = SetValueByReflection;
                }
#endif
            }
            
#if MILEASE_ENABLE_CODEGEN
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private E GetValueByReflection(T _)
            {
                return BindMember.MemberType switch
                {
                    MemberTypes.Field => (E)((FieldInfo)BindMember).GetValue(Target),
                    MemberTypes.Property => (E)((PropertyInfo)BindMember).GetValue(Target),
                    _ => throw new MilUnsupportedMemberTypeException(BindMember.Name)
                };
            }
        
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SetValueByReflection(T _, E value)
            {
                if (BindMember.MemberType == MemberTypes.Field)
                {
                    ((FieldInfo)BindMember).SetValue(Target, value);
                }
                else
                {
                    ((PropertyInfo)BindMember).SetValue(Target, value);
                }
            }
#endif
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override void Prepare()
            {
                StartValue = ValueGetter.Invoke(Target);
                DeltaValue = deltaFunc.Invoke(StartValue, ToValue);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal override void ApplyState(float pro)
            {
#if MILEASE_ENABLE_EXPRESSION
                expression.Invoke(Target, StartValue, ToValue, pro);
#elif MILEASE_ENABLE_CODEGEN
                var targetValue = calcFunc.Invoke(StartValue, DeltaValue, pro);
                ValueSetter.Invoke(Target, targetValue);
#endif
            }
        }
        
        [Serializable]
        public class AnimationState
        {
            public int StateID;
            public float Duration;
            public List<MilStateParameter> Values = new List<MilStateParameter>();
        }
    }
}
