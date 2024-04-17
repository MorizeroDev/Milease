﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Milease.Core
{
    public class RuntimeAnimationPart
    {
        public enum ValueTypeEnum
        {
            PrimitiveType, CustomType, Other, SelfHandle
        }

        public readonly MileaseHandleFunction HandleFunction;
        public readonly MileaseHandleFunction ResetFunction;
        public readonly MemberInfo BindMember;
        public readonly MethodInfo AdditionOperator, SubtractionOperator, MultiplyOperator;
        public readonly ValueTypeEnum ValueType;
        public readonly Type ValueTypeInfo;
        public readonly bool Valid;
        public object StartValue, ToValue, OriginalValue;
        public readonly object Target;
        public readonly MilAnimation.AnimationPart Source;
        public bool IsPrepared { get; private set; }
        public string MemberPath { get; private set; }

        private float lastProgress = -1f;

        public RuntimeAnimationPart(object target, MilAnimation.AnimationPart animation, MileaseHandleFunction handleFunction, MileaseHandleFunction resetFunction = null)
        {
            HandleFunction = handleFunction;
            Source = animation;
            Target = target;
            Valid = true;
            ResetFunction = resetFunction;
            ValueType = ValueTypeEnum.SelfHandle;
            MemberPath = handleFunction.GetHashCode().ToString();
        }

        internal void ResetAnimation()
        {
            if (!IsPrepared)
            {
                return;
            }
            
            IsPrepared = false;
            lastProgress = -1f;
        }
        
        public bool Reset()
        {
            if (!IsPrepared)
            {
                return false;
            }
            
            if (ResetFunction != null)
            {
                ResetFunction(Target, 0f);
                return true;
            }
            if (ValueType == ValueTypeEnum.SelfHandle)
            {
                return true;
            }
            
            if (BindMember.MemberType == MemberTypes.Field)
            {
                ((FieldInfo)BindMember).SetValue(Target, OriginalValue);
            }
            else
            {
                ((PropertyInfo)BindMember).SetValue(Target, OriginalValue);
            }

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RuntimeAnimationPart(object target, MilAnimation.AnimationPart animation, Type baseType, MemberInfo memberInfo = null)
        {
            Source = animation;
            
            var curType = baseType;
            if (memberInfo != null)
            {
                BindMember = memberInfo;
                MemberPath = memberInfo.Name + target.GetHashCode();
                goto skip_seek_member;
            }
            else
            {
                MemberPath = string.Join('.', animation.Binding);
            }
            
            for (var i = 0; i < animation.Binding.Count; i++)
            {
                var list = curType!.GetMember(animation.Binding[i]);
                if (list.Length == 0)
                {
                    Valid = false;
                    return;
                }

                BindMember = list[0];
                curType = BindMember.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)BindMember).FieldType,
                    MemberTypes.Property => ((PropertyInfo)BindMember).PropertyType,
                    _ => null
                };
                
                if (i != animation.Binding.Count - 1)
                {
                    target = BindMember.MemberType switch
                    {
                        MemberTypes.Field => ((FieldInfo)BindMember).GetValue(target),
                        MemberTypes.Property => ((PropertyInfo)BindMember).GetValue(target),
                        _ => null
                    };
                }
            } 
            
            skip_seek_member:

            Target = target;
            Valid = true;
            ValueTypeInfo = curType;

            if (curType == typeof(string))
            {
                StartValue = animation.StartValue;
                ToValue = animation.ToValue;
                ValueType = ValueTypeEnum.Other;
                return;
            }
            else if (curType!.IsPrimitive)
            {
                StartValue = double.Parse(animation.StartValue);
                ToValue = double.Parse(animation.ToValue);
                ValueType = ValueTypeEnum.PrimitiveType;
                return;
            }
            else
            {
                StartValue = JsonUtility.FromJson(animation.StartValue, curType);
                ToValue = JsonUtility.FromJson(animation.ToValue, curType);
            }
            
            var methods = curType!.GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();
            AdditionOperator = methods.Find(x =>
            {
                var param = x.GetParameters();
                return x.Name == "op_Addition" && param[0].ParameterType == curType &&
                       param[1].ParameterType == curType && x.ReturnType == curType;
            });
            SubtractionOperator = methods.Find(x =>
            {
                var param = x.GetParameters();
                return x.Name == "op_Subtraction" && param[0].ParameterType == curType &&
                       param[1].ParameterType == curType && x.ReturnType == curType;
            });
            MultiplyOperator = methods.Find(x =>
            {
                var param = x.GetParameters();
                return x.Name == "op_Multiply" && param[0].ParameterType == curType &&
                       (param[1].ParameterType == typeof(Single) || param[1].ParameterType == typeof(Double)) && x.ReturnType == curType;
            });
            ValueType = AdditionOperator != null && SubtractionOperator != null && MultiplyOperator != null ? ValueTypeEnum.CustomType : ValueTypeEnum.Other;
        }

        public static void SetValue(RuntimeAnimationPart ani, float pro)
        {
            if (pro == ani.lastProgress)
            {
                return;
            }

            ani.lastProgress = pro;
            
            if (!ani.IsPrepared)
            {
                ani.IsPrepared = true;
                if (ani.ValueType != ValueTypeEnum.SelfHandle)
                {
                    ani.OriginalValue = ani.BindMember.MemberType switch
                    {
                        MemberTypes.Field => ((FieldInfo)ani.BindMember).GetValue(ani.Target),
                        MemberTypes.Property => ((PropertyInfo)ani.BindMember).GetValue(ani.Target),
                        _ => null
                    };
                    if (ani.Source.PendingTo)
                    {
                        ani.StartValue = ani.OriginalValue;
                    }
                }
            }
            
            if (ani.ValueType == ValueTypeEnum.SelfHandle)
            {
                ani.HandleFunction(ani.Target, pro);
                return;
            }
            
            var result = ani.ValueType switch
            {
                ValueTypeEnum.PrimitiveType => Convert.ChangeType((double)ani.StartValue + ((double)ani.ToValue - (double)ani.StartValue) * pro, ani.ValueTypeInfo),
                ValueTypeEnum.CustomType => 
                    ani.AdditionOperator.Invoke(null, new object[] 
                    {
                        ani.StartValue, 
                        ani.MultiplyOperator.Invoke(null, new object[]
                        {
                            ani.SubtractionOperator.Invoke(null, new object[]
                            {
                                ani.ToValue, 
                                ani.StartValue
                            }), 
                            pro
                        })
                    }),
                ValueTypeEnum.Other => pro >= 1f ? ani.ToValue : ani.StartValue,
                _ => default
            };

            if (ani.Source.BlendingMode == MilAnimation.BlendingMode.Additive)
            {
                switch (ani.ValueType)
                {
                    case ValueTypeEnum.PrimitiveType:
                        result = Convert.ChangeType((double)result + (double)ani.OriginalValue, ani.ValueTypeInfo);
                        break;
                    case ValueTypeEnum.CustomType:
                        result = ani.AdditionOperator.Invoke(null, new object[]
                        {
                            result,
                            ani.OriginalValue
                        });
                        break;
                    default:
                        Debug.LogWarning($"ValueType {ani.ValueType} doesn't support additive blending mode.");
                        break;
                }
            }
            
            if (ani.BindMember.MemberType == MemberTypes.Field)
            {
                ((FieldInfo)ani.BindMember).SetValue(ani.Target, result);
            }
            else
            {
                ((PropertyInfo)ani.BindMember).SetValue(ani.Target,result);
            }
        }
    }
}