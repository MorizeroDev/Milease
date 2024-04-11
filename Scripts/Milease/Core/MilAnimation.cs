using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Milease.Utils;
using UnityEngine;
using Object = System.Object;

namespace Milease.Core
{
    [CreateAssetMenu(menuName = "Milease/New MilAnimation", fileName = "New MilAnimation")]
    public class MilAnimation : ScriptableObject
    {
        public class RuntimeAnimationPart
        {
            public enum ValueTypeEnum
            {
                PrimitiveType, CustomType, Other
            }
            public readonly MemberInfo BindMember;
            public readonly MethodInfo AdditionOperator, SubtractionOperator, MultiplyOperator;
            public readonly ValueTypeEnum ValueType;
            public readonly Type ValueTypeInfo;
            public readonly bool Valid;
            public readonly object StartValue, ToValue;
            public readonly object Target;
            public readonly AnimationPart Source;

            public RuntimeAnimationPart(object target, AnimationPart animation, Type baseType, MemberInfo memberInfo = null)
            {
                Source = animation;
                
                var curType = baseType;
                if (memberInfo != null)
                {
                    BindMember = memberInfo;
                    goto skip_seek_member;
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

                if (curType!.IsPrimitive)
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
        }
        [Serializable]
        public class AnimationPart
        {
            public float StartTime;
            public float Duration;
            public EaseUtility.EaseType EaseType;
            public EaseUtility.EaseFunction EaseFunction;
            public AnimationCurve CustomCurve;
            public List<string> Binding;
            public string StartValue;
            public string ToValue;
        }
        [HideInInspector]
        public List<AnimationPart> Parts;

        public static AnimationPart Part(string binding, object startValue, object toValue, float startTime, float duration,
            EaseUtility.EaseType easeType, EaseUtility.EaseFunction easeFunction)
        {
            var type = startValue.GetType();
            return new AnimationPart()
            {
                StartTime = startTime,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction,
                Binding = binding.Split('.').ToList(),
                StartValue = type.IsPrimitive ? startValue.ToString() : JsonUtility.ToJson(startValue),
                ToValue = type.IsPrimitive ? toValue.ToString() : JsonUtility.ToJson(toValue)
            };
        }
        
        public static AnimationPart SimplePart(object startValue, object toValue, float duration,
            EaseUtility.EaseType easeType = EaseUtility.EaseType.In, EaseUtility.EaseFunction easeFunction = EaseUtility.EaseFunction.Quad)
        {
            var type = startValue.GetType();
            return new AnimationPart()
            {
                StartTime = 0f,
                Duration = duration,
                EaseType = easeType,
                EaseFunction = easeFunction,
                StartValue = type.IsPrimitive ? startValue.ToString() : JsonUtility.ToJson(startValue),
                ToValue = type.IsPrimitive ? toValue.ToString() : JsonUtility.ToJson(toValue)
            };
        }
    }
}