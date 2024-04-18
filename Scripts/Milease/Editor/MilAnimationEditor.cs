using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Milease.Core;
using Milease.Core.Animation;
using Milease.Utils;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Milease.Editor
{
    public class MilAnimationEditor : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero; // 滚动位置
        private float timeLineLength = 2f; // 时间轴长度
        private float boxWidth = 50f; // 矩形宽度
        private float boxHeight = 20f; // 矩形高度
        private float timeInterval = 0.1f; // 时间刻度间隔

        private EaseType easeType;
        private EaseFunction easeFunction;

        private MilAnimation editingAnimation;

        private List<List<string>> reflected;
        private string currentDepth;

        private static Stopwatch watch = new();
        
        //[MenuItem("Milease/Open MilAnimation Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MilAnimationEditor), false, "MilAnimation Editor");
            watch.Restart();
        }

        private void ShowMemberMenu()
        {
            IEnumerable<string> fields;
            if (currentDepth == "")
            {
                fields = reflected.Select(x => x[0]).Distinct();
            }
            else
            {
                var depth = currentDepth.Split('.').Length;
                fields = reflected.Where(x => x.Count > depth && string.Join('.', x).StartsWith(currentDepth))
                                  .Select(x => x[depth]).Distinct();
            }
            
            var menu = new GenericMenu();
            foreach (var field in fields)
            {
                menu.AddItem(new GUIContent(field), false, () =>
                {
                    currentDepth += (currentDepth != "" ? "." : "") + field;
                    ShowMemberMenu();
                });
            }

            if (currentDepth == "")
            {
                menu.ShowAsContext();
            }
            else
            {
                menu.DropDown(new Rect(0f, 0f, 0, 0));
            }
        }
        
        private void OnGUI()
        {
            if (Selection.objects[0] is MilAnimation)
            {
                editingAnimation = (MilAnimation)Selection.objects[0];
            }

            if (!editingAnimation)
            {
                return;
            }

            var ani = editingAnimation;
            var timeLen = 1f;
            if (ani.Parts.Count > 0)
            {
                timeLen = ani.Parts.Max(x => x.StartTime + x.Duration);
            }
            var keys = ani.Parts.Select(x => x.Binding).Distinct().ToList();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true, 
                GUILayout.Height(position.height - 160f));
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Label("");
                            foreach (var key in keys)
                            {
                                GUILayout.Label(string.Join('.', key), GUILayout.MaxWidth(160f));
                            }
                            if (GUILayout.Button("+", GUILayout.MaxWidth(160f)) && Selection.objects[0] is GameObject go)
                            {
                                reflected = EditorUtils.GetAnimatableFields(go);
                                currentDepth = "";
                                ShowMemberMenu();
                            }
                        }
                        GUILayout.EndVertical();
                        
                        for (float i = 0; i < timeLen; i += timeInterval)
                        {
                            GUILayout.Label(i.ToString("F1"), GUILayout.Width(boxWidth));

                            var buttonRect = GUILayoutUtility.GetLastRect();
                            buttonRect.y += boxHeight;
                            buttonRect.height = boxHeight;
                            buttonRect.width = boxWidth;
                            if (GUI.Button(buttonRect, ""))
                            {
                                Debug.Log("Button clicked at time " + i);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            var curve = new AnimationCurve();
            for (var i = 0; i <= 50; i++)
            {
                var t = i * 1f / 50f;
                curve.AddKey(t, EaseUtility.GetEasedProgress(t, easeType, easeFunction));
            }

            EditorGUILayout.Space(15f);
            
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("", GUILayout.Width(20f));
                EditorGUILayout.CurveField(curve, GUILayout.Width(120f), GUILayout.Height(120f));
                EditorGUILayout.LabelField("", GUILayout.Width(20f));
                
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Ease Type");
                    easeType = (EaseType)EditorGUILayout.EnumPopup(easeType);
                    EditorGUILayout.LabelField("Ease Function");
                    easeFunction = (EaseFunction)EditorGUILayout.EnumPopup(easeFunction);
                }
                GUILayout.EndVertical();

                if (!watch.IsRunning)
                {
                    watch.Restart();
                }
                var sPro = (watch.ElapsedMilliseconds % 3000) / 3000f;
                var pro = EaseUtility.GetEasedProgress(sPro, easeType, easeFunction);
                
                EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(160f), GUILayout.Height(140f));
                {
                    Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                        new (20f, sPro * 120f),
                        new (20f + 20f, sPro * 120f),
                        new (20f + 20f, sPro * 120f + 20f),
                        new (20f, sPro * 120f + 20f)
                    }, Color.clear, new Color(0.8f, 0.8f, 0.8f, 0.5f));
                    Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                        new (60f, 60f),
                        new (60f + 20f, 60f),
                        new (60f + 20f, 80f),
                        new (60f, 80f)
                    }, Color.clear, new Color(0.8f, 0.8f, 0.8f, sPro * 0.5f));
                    var size = 20f * (1f + sPro);
                    Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                        new (120f - size / 2f, 20f - size / 2f),
                        new (120f + size / 2f, 20f - size / 2f),
                        new (120f + size / 2f, 20f + size / 2f),
                        new (120f - size / 2f, 20f + size / 2f)
                    }, Color.clear, new Color(0.8f, 0.8f, 0.8f, 0.5f));
                    
                    Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                        new (20f, pro * 120f),
                        new (20f + 20f, pro * 120f),
                        new (20f + 20f, pro * 120f + 20f),
                        new (20f, pro * 120f + 20f)
                    }, new Color(0f, 0.4f, 0.8f, 0.5f), Color.clear);
                    Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                        new (60f, 0f),
                        new (60f + 20f, 0f),
                        new (60f + 20f, 20f),
                        new (60f, 20f)
                    }, new Color(0f, 0.4f, 0.8f, pro * 0.5f), Color.clear);
                    size = 20f * (1f + pro);
                    Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                        new (120f - size / 2f, 20f - size / 2f),
                        new (120f + size / 2f, 20f - size / 2f),
                        new (120f + size / 2f, 20f + size / 2f),
                        new (120f - size / 2f, 20f + size / 2f)
                    }, new Color(0f, 0.4f, 0.8f, 0.3f), Color.clear);
                }
                EditorGUILayout.EndScrollView();
                
            }
            GUILayout.EndHorizontal();
            
            Repaint();
        } 
    }
}