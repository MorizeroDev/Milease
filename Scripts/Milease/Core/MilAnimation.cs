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
        public class RuntimeAnimationPart<T>
        {
            public readonly MemberInfo BindMember;
            public readonly MethodInfo AdditionOperator, SubtractionOperator, MultiplyOperator;
            public readonly bool OverrideOperators;
            public readonly bool Valid;
            public readonly T StartValue, ToValue;

            public RuntimeAnimationPart(AnimationPart animation)
            {
                var curType = typeof(T);
                for (var i = 0; i < animation.Binding.Count; i++)
                {
                    var list = curType!.GetMember(animation.Binding[i], BindingFlags.GetField | BindingFlags.GetProperty);
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
                }

                Valid = true;

                StartValue = JsonUtility.FromJson<T>(animation.StartValue);
                ToValue = JsonUtility.FromJson<T>(animation.ToValue);
                
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
                OverrideOperators = AdditionOperator != null && SubtractionOperator != null && MultiplyOperator != null;
            }

            public static void SetValue(object target, RuntimeAnimationPart<T> ani, float pro)
            {
                if (ani.BindMember.MemberType == MemberTypes.Field)
                {
                    ((FieldInfo)ani.BindMember).SetValue(target,
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
                        })
                    );
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
    }
}