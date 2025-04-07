#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Milease.Core;
using Milease.Core.Animator;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Milease.Editor
{
    [CustomEditor(typeof(object), true)]
    public class InstantAnimatorViewer : UnityEditor.Editor
    {
        public class AnimatorFunction
        {
            public string FunctionName;
            public MilInstantAnimator ParsedAnimator;
            public Exception ParseException;
            public float AnimationLength;
            public float[] ClipStartTime;
        }

        private List<AnimatorFunction> _functions;

        internal static List<AnimatorFunction> GetAnimatorFunctions(Object obj)
        {
            return obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Default | BindingFlags.Instance)
                    .Where(x => x.ReturnType == typeof(MilInstantAnimator) && x.GetParameters().Length == 0)
                    .Select(x =>
                    {
                        try
                        {
                            var ret = new AnimatorFunction()
                            {
                                FunctionName = x.Name,
                                ParsedAnimator = (MilInstantAnimator)x.Invoke(obj, null)
                            };
                            ret.ClipStartTime = new float[ret.ParsedAnimator.Collection.Count];
                            var index = 0;
                            foreach (var ani in ret.ParsedAnimator.Collection)
                            {
                                var duration = 0f;
                                foreach (var part in ani)
                                {
                                    duration = Mathf.Max(duration, part.GetStartTime() + part.GetDuration());
                                }

                                ret.ClipStartTime[index] = ret.AnimationLength;
                                index++;

                                ret.AnimationLength += duration;
                            }
                            return ret;
                        }
                        catch (Exception err)
                        {
                            return new AnimatorFunction()
                            {
                                FunctionName = x.Name,
                                ParseException = err
                            };
                        }
                    }).ToList();   
        }
        
        private void OnEnable()
        {
            _functions = GetAnimatorFunctions(target);
        }

        public override void OnInspectorGUI()
        {
            if (_functions.Count > 0)
            {
                EditorGUILayout.LabelField("Milease Animation", EditorStyles.boldLabel);
                if (GUILayout.Button("Debug ↗"))
                {
                    InstantAnimatorDebugWindow.Show(_functions, target);
                }
                EditorGUILayout.Separator();
            }
            base.OnInspectorGUI();
        }
    }
}
#endif
