using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milease.Enums;
using Milease.Milease.Exception;
using UnityEngine;

namespace Milease.Core.Animation
{
    public class MilStateAnimation
    {
        [Serializable]
        public class AnimationValue
        {
            public EaseType EaseType;
            public EaseFunction EaseFunction;
            public AnimationCurve CustomCurve;
            public object Target;
            public MemberInfo BindMember;
            public string Member;
            public object ToValue, StartValue;
            public readonly ValueTypeEnum ValueType;
            public readonly Type ValueTypeInfo;
            public readonly MethodInfo AdditionOperator, SubtractionOperator, MultiplyOperator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AnimationValue(object target, string member, object toValue)
            {
                Target = target ?? throw new MilTargetNotFoundException();
                Member = member;

                var members = target.GetType().GetMember(member);
                if (members.Length == 0)
                {
                    throw new MilMemberNotFoundException(member);
                }
                BindMember = members[0];
                
                ValueTypeInfo = BindMember.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)BindMember).FieldType,
                    MemberTypes.Property => ((PropertyInfo)BindMember).PropertyType,
                    _ => throw new MilUnsupportedMemberTypeException(BindMember.Name)
                };

                ToValue = toValue;
                if (ValueTypeInfo == typeof(string))
                {
                    ValueType = ValueTypeEnum.Other;
                    return;
                }
                else if (ValueTypeInfo!.IsPrimitive)
                {
                    ValueType = ValueTypeEnum.PrimitiveType;
                    ToValue = Convert.ChangeType(toValue, TypeCode.Double);
                    return;
                }

                var curType = ValueTypeInfo;
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
        }
        [Serializable]
        public class AnimationState
        {
            public int StateID;
            public float Duration;
            public List<AnimationValue> Values = new();
            
            private float lastProgress = -1f;
            public void Reset()
            {
                lastProgress = -1f;
            }
        }

        public static void ApplyState(AnimationValue ani, float pro)
        {
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
            
            if (ani.BindMember.MemberType == MemberTypes.Field)
            {
                ((FieldInfo)ani.BindMember).SetValue(ani.Target, result);
            }
            else
            {
                ((PropertyInfo)ani.BindMember).SetValue(ani.Target,result);
            }
        }

        public static void PrepareState(AnimationValue ani)
        {
            ani.StartValue = ani.BindMember.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)ani.BindMember).GetValue(ani.Target),
                MemberTypes.Property => ((PropertyInfo)ani.BindMember).GetValue(ani.Target),
                _ => null
            };
            if (ani.ValueType == ValueTypeEnum.PrimitiveType)
            {
                ani.StartValue = Convert.ChangeType(ani.StartValue, TypeCode.Double);
            }
        }
    }
}