using System;
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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Milease.Core
{
    public abstract class RuntimeAnimationBase
    {
        internal MilAnimation.AnimationControlInfo ControlInfo;
    }
    
    public class RuntimeAnimationPart<T, E> : RuntimeAnimationBase, IAnimationController
    {
        public readonly MileaseHandleFunction<T, E> HandleFunction;
        public readonly MileaseHandleFunction<T, E> ResetFunction;

#if MILEASE_ENABLE_EXPRESSION
        private readonly OffsetAnimatorExpression<T, E> offsetExpression;
        private readonly AnimatorExpression<T, E> expression;
#elif MILEASE_ENABLE_CODEGEN
        private readonly OffsetCalculateFunction<E> offsetCalcFunc;
        private readonly CalculateFunction<E> calcFunc;
        private readonly DeltaCalculateFunction<E> deltaFunc;

        private E delta;
#endif
        private readonly MemberInfo BindMember;
        
        public readonly Action<T, E> ValueSetter;
        public readonly Func<T, E> ValueGetter;
        
        private MilAnimation.AnimationPart<E> _animationData;
        
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
            ControlInfo = animation.ControlInfo;
            _animationData = animation;
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
            ControlInfo = animation.ControlInfo;
            _animationData = animation;
            ParentAnimator = animator;
            BindMember = member;
            
            if (member != null)
            {
                MemberPath = member.Name + target.GetHashCode();
#if MILEASE_ENABLE_EXPRESSION
                ValueGetter = AnimatorExprManager.GetValGetter<T, E>(member);
                ValueSetter = AnimatorExprManager.GetValSetter<T, E>(member);
            
                expression = AnimatorExprManager.GetExpr<T, E>(member);
                offsetExpression = AnimatorExprManager.GetExprWithOffset<T, E>(member);
#elif MILEASE_ENABLE_CODEGEN
                
                offsetCalcFunc = RuntimeBridge.GetOffsetFunc<E>();
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
            else
            {
                MemberPath = target.GetHashCode().ToString();
            }
            
            Target = target;
            
            if (!ControlInfo.PendingTo)
            {
                StartValue = animation.StartValue;
            }
            ToValue = animation.ToValue;
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

        void IAnimationController.ResetAnimation()
        {
            if (!IsPrepared)
            {
                return;
            }
            
            IsPrepared = false;
            lastProgress = -1f;
        }

#if UNITY_EDITOR
        private void PrintEditorField(string text, E value)
        {
            if (value is Color color)
            {
                EditorGUILayout.ColorField(text, color);
            }
            else if (value is Vector2 vector2)
            {
                EditorGUILayout.Vector2Field(text, vector2);
            }
            else if (value is Vector3 vector3)
            {
                EditorGUILayout.Vector3Field(text, vector3);
            }
            else if (value is float f)
            {
                EditorGUILayout.FloatField(text, f);
            }
            else if (value is int i)
            {
                EditorGUILayout.IntField(text, i);
            }
            else if (value is string s)
            {
                EditorGUILayout.TextField(text, s);
            }
        }
        
        void IAnimationController.DrawWindow(float time, Color color)
        {
            EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 6, 6)
            });
            
            if (BindMember != null)
            {
                EditorGUILayout.LabelField($"<color=cyan>{time + ControlInfo.StartTime:F3}s -> {time + ControlInfo.StartTime + ControlInfo.Duration:F3}s</color>: {BindMember.Name} ({typeof(T).FullName} {typeof(E).FullName})", new GUIStyle(EditorStyles.boldLabel)
                {
                    normal =
                    {
                        textColor = color
                    },
                    richText = true
                });
            }
            else
            {
                EditorGUILayout.LabelField($"<color=cyan>{time + ControlInfo.StartTime:F3}s -> {time + ControlInfo.StartTime + ControlInfo.Duration:F3}s</color>: <Event>", new GUIStyle(EditorStyles.boldLabel)
                {
                    normal =
                    {
                        textColor = color
                    },
                    richText = true
                });
            }
            EditorGUILayout.LabelField($"Duration: <b><color=cyan>{ControlInfo.Duration:F3}s</color></b>, Self Delay: <b><color=cyan>{ControlInfo.StartTime:F3}s</color></b>", new GUIStyle(EditorStyles.boldLabel)
            {
                normal =
                {
                    textColor = color
                },
                richText = true
            });

            GUI.enabled = false;
            PrintEditorField("Start Value", StartValue);
            PrintEditorField("End Value", ToValue);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Easing Function:  {ControlInfo.EaseFunction} {ControlInfo.EaseType}");
            EditorGUILayout.LabelField($"Blending:  {ControlInfo.BlendingMode}");
            EditorGUILayout.EndHorizontal();
            if (ControlInfo.PendingTo)
            {
                EditorGUILayout.HelpBox("This is a 'To' animation.", MessageType.Info);
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndVertical();
        }

        float IAnimationController.GetStartTime()
        {
            return ControlInfo.StartTime;
        }
        
        float IAnimationController.GetDuration()
        {
            return ControlInfo.Duration;
        }
#endif

        public bool Reset(AnimationResetMode resetMode, bool revertToChanges = true)
        {
            if (!revertToChanges && ControlInfo.PendingTo)
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

            if (ControlInfo.PendingTo || ControlInfo.BlendingMode == BlendingMode.Additive)
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
            if (lastProgress == progress)
            {
                return;
            }

            lastProgress = progress;
            
            if (!IsPrepared)
            {
                IsPrepared = true;
            
                if (ValueGetter != null)
                {
                    OriginalValue = ValueGetter.Invoke(Target);
            
                    if (ControlInfo.PendingTo)
                    {
                        StartValue = OriginalValue;
                    }

                    delta = deltaFunc.Invoke(StartValue, ToValue);
                }
            }

            if (ValueType == ValueTypeEnum.SelfHandle || HandleFunction != null)
            {
                HandleFunction(new MilHandleFunctionArgs<T, E>()
                {
                    Animation = this,
                    Target = Target,
                    Progress = progress,
                    Animator = ParentAnimator
                });
                return;
            }
#if MILEASE_ENABLE_EXPRESSION
            if (expression == null)
            {
                return;
            }

            if (Source.BlendingMode == BlendingMode.Additive)
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
#elif MILEASE_ENABLE_CODEGEN
            if (calcFunc == null)
            {
                return;
            }
            
            var targetValue = ControlInfo.BlendingMode == BlendingMode.Additive
                ? offsetCalcFunc.Invoke(StartValue, delta, progress, OriginalValue)
                : calcFunc.Invoke(StartValue, delta, progress);
                
            ValueSetter.Invoke(Target, targetValue);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.Delay(float time)
        {
            ControlInfo.StartTime += time;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.SetDuration(float duration)
        {
            ControlInfo.Duration = duration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAnimationController.SetBlendingMode(BlendingMode mode)
        {
            ControlInfo.BlendingMode = mode;
        }
    }
}
