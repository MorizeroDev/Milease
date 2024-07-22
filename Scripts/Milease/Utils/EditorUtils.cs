using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace Milease.Utils
{
    public static class EditorUtils
    {
        public static List<List<string>> GetAnimatableFields(GameObject go)
        {
            var fields = GetAnimatableFields(typeof(GameObject), new List<Type>() { typeof(GameObject) });
            foreach (var component in go.GetComponents<Component>())
            {
                foreach (var child in GetAnimatableFields(component.GetType(), new List<Type>() { component.GetType() }))
                {
                    child.Insert(0, "[" + component.GetType().Name + "]");
                    fields.Add(child);
                }
            }

            return fields;
        }
        
        public static List<List<string>> GetAnimatableFields(Type type, List<Type> formerTypes)
        {
            var list = new List<List<string>>();
            var fields = type.GetMembers();
            formerTypes.Add(type);
            
            foreach (var field in fields)
            {
                if (field.MemberType != MemberTypes.Field && field.MemberType != MemberTypes.Property)
                {
                    continue;
                }

                if (Attribute.IsDefined(field, typeof(ObsoleteAttribute)))
                {
                    continue;
                }

                var t = field.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)field).FieldType,
                    MemberTypes.Property => ((PropertyInfo)field).PropertyType,
                    _ => null
                };

                var writable = field.MemberType switch
                {
                    MemberTypes.Field => !(((FieldInfo)field).IsInitOnly || ((FieldInfo)field).IsLiteral),
                    MemberTypes.Property => ((PropertyInfo)field).CanWrite,
                    _ => false
                };
                
                if (t!.IsValueType)
                {
                    if (writable)
                    {
                        list.Add(new List<string>(){field.Name});
                    }
                }
                else if (!formerTypes.Contains(t))
                {
                    foreach (var child in GetAnimatableFields(t, new List<Type>(formerTypes)))
                    {
                        child.Insert(0, field.Name);
                        list.Add(child);
                    }
                }
            }

            return list;
        }
    }
}