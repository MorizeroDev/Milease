﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milease.CodeGen;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Milease.Exception;
using Milease.Utils;
using UnityEngine;

namespace Milease.Core
{
    public abstract class RuntimeAnimationBase
    {
        internal MilAnimation.AnimationPartBase Source;
    }
    
    public class RuntimeAnimationPart<T, E> : RuntimeAnimationBase, IAnimationController
    {
        public readonly MileaseHandleFunction<T, E> HandleFunction;
        public readonly MileaseHandleFunction<T, E> ResetFunction;

#if MILEASE_ENABLE_EXPRESSION
        private readonly OffsetAnimatorExpression<T, E> offsetExpression;
        private readonly AnimatorExpression<T, E> expression;
#else
        private readonly OffsetCalculateFunction<E> offsetCalcFunc;
        private readonly CalculateFunction<E> calcFunc;
        
        private readonly MemberInfo BindMember;
#endif
        public readonly Action<T, E> ValueSetter;
        public readonly Func<T, E> ValueGetter;
        
        public string MemberPath { get; }
        public readonly ValueTypeEnum ValueType;
        
        public E StartValue, ToValue, OriginalValue;
        public readonly T Target;
        
        public readonly MilInstantAnimator ParentAnimator;
        
        public bool IsPrepared { get; internal set; }

        private float lastProgress = -1f;
        
        public RuntimeAnimationPart(T target, MilInstantAnimator animator, 
            MilAnimation.AnimationPart<E> animation, 
            MileaseHandleFunction<T, E> handleFunction, MileaseHandleFunction<T, E> resetFunction = null)
        {
            HandleFunction = handleFunction;
            Source = animation;
            Target = target;
            ResetFunction = resetFunction;
            ValueType = ValueTypeEnum.SelfHandle;
            ParentAnimator = animator;
            MemberPath = handleFunction.GetHashCode().ToString();
        }

        public RuntimeAnimationPart(T target, MilInstantAnimator animator, 
            MilAnimation.AnimationPart<E> animation, MemberInfo member, 
            MileaseHandleFunction<T, E> handleFunction = null)
        {
            if (target == null)
            {
                throw new MilTargetNotFoundException();
            }
            
            HandleFunction = handleFunction;
            Source = animation;
            ParentAnimator = animator;
            
            if (member != null)
            {
                MemberPath = member.Name + target.GetHashCode();
#if MILEASE_ENABLE_EXPRESSION
                ValueGetter = AnimatorExprManager.GetValGetter<T, E>(member);
                ValueSetter = AnimatorExprManager.GetValSetter<T, E>(member);
            
                expression = AnimatorExprManager.GetExpr<T, E>(member);
                offsetExpression = AnimatorExprManager.GetExprWithOffset<T, E>(member);
#else
                BindMember = member;
                
                offsetCalcFunc = RuntimeBridge.GetOffsetFunc<E>();
                calcFunc = RuntimeBridge.GetFunc<E>();

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
            else
            {
                MemberPath = target.GetHashCode().ToString();
            }
            
            Target = target;
            
            if (!animation.PendingTo)
            {
                StartValue = animation.StartValue;
            }
            ToValue = animation.ToValue;
        }

#if !MILEASE_ENABLE_EXPRESSION
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

        void IAnimationController.ResetAnimation()
        {
            if (!IsPrepared)
            {
                return;
            }
            
            IsPrepared = false;
            lastProgress = -1f;
        }
        
        public bool Reset(AnimationResetMode resetMode, bool revertToChanges = true)
        {
            if (!revertToChanges && Source.PendingTo)
            {
                return false;
            }
            
            if (!IsPrepared && resetMode == AnimationResetMode.ResetToOriginalState)
            {
                // If the original state hasn't been calculated yet
                return false;
            }
            
            if (ResetFunction != null)
            {
                ResetFunction(new MilHandleFunctionArgs<T, E>()
                {
                    Animation = this,
                    Target = Target,
                    Progress = 0f,
                    Animator = ParentAnimator
                });
                return true;
            }
            
            if (ValueType == ValueTypeEnum.SelfHandle)
            {
                return true;
            }

            if (Source.PendingTo || Source.BlendingMode == BlendingMode.Additive)
            {
                // MileaseTo or additive blending animation doesn't have a initial state,
                // force to reset to original state instead.
                resetMode = AnimationResetMode.ResetToOriginalState;
                if (!IsPrepared)
                {
                    // If the original state hasn't been calculated yet, skip resetting,
                    // but prevent resetting by subsequent animations.
                    return true;
                }
            }

            var targetValue = resetMode switch
            {
                AnimationResetMode.ResetToOriginalState => OriginalValue,
                AnimationResetMode.ResetToInitialState => StartValue,
                _ => throw new ArgumentOutOfRangeException(nameof(resetMode), resetMode, null)
            };
            
            ValueSetter.Invoke(Target, targetValue);
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.Apply(float progress)
        {
#if MILEASE_ENABLE_EXPRESSION
            if (expression == null)
            {
                return;
            }

            if (Source.BlendingMode == MilAnimation.BlendingMode.Additive)
            {
#if UNITY_EDITOR
                if (offsetExpression == null)
                {
                    throw new Exception(
                        $"The type {typeof(E).Name} doesn't meet the requirements for additive animation mode.");
                }                
#endif
                offsetExpression.Invoke(Target, StartValue, ToValue, progress, OriginalValue);
            }
            else
            {
                expression.Invoke(Target, StartValue, ToValue, progress);
            }
#else
            if (calcFunc == null)
            {
                return;
            }
            
            var targetValue = Source.BlendingMode == BlendingMode.Additive
                ? offsetCalcFunc.Invoke(StartValue, ToValue, progress, OriginalValue)
                : calcFunc.Invoke(StartValue, ToValue, progress);
                
            ValueSetter.Invoke(Target, targetValue);
#endif
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.SetOriginalValue()
        {
            if (IsPrepared)
            {
                return;
            }
            IsPrepared = true;
            
            if (ValueGetter == null)
            {
                return;
            }
            OriginalValue = ValueGetter.Invoke(Target);
            
            if (Source.PendingTo)
            {
                StartValue = OriginalValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IAnimationController.InvokeSelfHandling(float progress)
        {
            if (ValueType == ValueTypeEnum.SelfHandle || HandleFunction != null)
            {
                HandleFunction(new MilHandleFunctionArgs<T, E>()
                {
                    Animation = this,
                    Target = Target,
                    Progress = progress,
                    Animator = ParentAnimator
                });
                return true;
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IAnimationController.ShouldUpdate(float progress)
        {
            if (progress == lastProgress)
            {
                return false;
            }

            lastProgress = progress;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.Delay(float time)
        {
            Source.StartTime += time;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.SetDuration(float duration)
        {
            Source.Duration = duration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.SetBlendingMode(BlendingMode mode)
        {
            Source.BlendingMode = mode;
        }
    }
}
