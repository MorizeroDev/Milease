#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Milease.Core;
using Milease.Core.Manager;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Milease.Editor
{
    public class InstantAnimatorDebugWindow : EditorWindow
    {
        [Serializable]
        private class EditorStatus
        {
            public bool ViewDetail;
            public bool[] PartView;
        }
        
        [SerializeField]
        private Object _objectSrc;
        
        private List<InstantAnimatorViewer.AnimatorFunction> _functions;

        [SerializeField] 
        private List<EditorStatus> _status;
        
        private Vector2 scrollPos;
        
        internal static bool Playing;

        public static void Show(List<InstantAnimatorViewer.AnimatorFunction> functions, Object obj)
        {
            var window = GetWindow<InstantAnimatorDebugWindow>("Milease Animation Debug");
            window.minSize = new Vector2(620f, 600f);
            window.maxSize = new Vector2(620f, Screen.height);
            window._functions = functions;
            window._objectSrc = obj;
            window._status = functions.Select(x => new EditorStatus()
            {
                ViewDetail = false,
                PartView = new bool[x.ParsedAnimator?.Collection.Count ?? 1]
            }).ToList();
            foreach (var ani in functions)
            {
                if (ani.ParsedAnimator == null)
                {
                    continue;
                }

                ani.ParsedAnimator.Time = 0f;
                ani.ParsedAnimator.PlayIndex = 0;
            }
            window.autoRepaintOnSceneChange = true;
            window.ShowTab();
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneClosing += EditorSceneManagerOnsceneClosing;
            EditorSceneManager.sceneSaving += EditorSceneManagerOnsceneSaving;
        }

        private void EditorSceneManagerOnsceneSaving(Scene scene, string path)
        {
            ResetAnimators();
        }

        private void EditorSceneManagerOnsceneClosing(Scene scene, bool removingscene)
        {
            ResetAnimators();
            Close();
        }

        private void ResetAnimators()
        {
            foreach (var ani in _functions)
            {
                if (ani.ParsedAnimator == null)
                {
                    continue;
                }
                ani.ParsedAnimator.Reset();
                ani.ParsedAnimator.Stop();
            }
        }

        private void OnDisable()
        {
            ResetAnimators();
            EditorSceneManager.sceneClosing -= EditorSceneManagerOnsceneClosing;
            EditorSceneManager.sceneSaving -= EditorSceneManagerOnsceneSaving;
        }

        private void OnGUI()
        {
            var titleStyle = new GUIStyle()
            {
                fontSize = 16,
                normal =
                {
                    textColor = Color.white
                },
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, 0, 8)
            };
            var panelStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 15, 10, 10),
                padding = new RectOffset(16, 16, 16, 16)
            };

            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.BeginVertical(panelStyle);
                
                EditorGUILayout.LabelField("Waiting for the script compilation...");
                
                EditorGUILayout.EndVertical();
                return;
            }

            if (_functions == null && _objectSrc)
            {
                _functions = InstantAnimatorViewer.GetAnimatorFunctions(_objectSrc);
                if (_functions.Count != _status.Count)
                {
                    var newStatus = new EditorStatus[_functions.Count];
                    Array.Copy(_status.ToArray(), newStatus, Math.Min(_status.Count, _functions.Count));
                    for (var i = _status.Count; i < _functions.Count; i++)
                    {
                        newStatus[i] = new EditorStatus()
                        {
                            PartView = new bool[_functions[i].ParsedAnimator?.Collection.Count ?? 1]
                        };
                    }

                    _status = newStatus.ToList();
                }

                for (var i = 0; i < _status.Count; i++)
                {
                    var cnt = _functions[i].ParsedAnimator?.Collection.Count ?? 1;
                    if (_status[i].PartView.Length != cnt)
                    {
                        var newPartView = new bool[cnt];
                        Array.Copy(_status[i].PartView, newPartView, Math.Min(_status[i].PartView.Length, cnt));
                        _status[i].PartView = newPartView; 
                    }
                }
            }
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            scrollPos.x = 0;
            
            if (_functions == null || _functions.Count == 0)
            {
                EditorGUILayout.BeginVertical(panelStyle);
                
                EditorGUILayout.LabelField("Nothing to show", titleStyle);
                EditorGUILayout.LabelField("Select a component that contains the animation function in the inspector, and then click the debug button.", EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.EndVertical();
                return;
            }

            var aniIndex = 0;
            foreach (var ani in _functions)
            {
                EditorGUILayout.BeginVertical(panelStyle);
            
                EditorGUILayout.LabelField(ani.FunctionName, titleStyle);

                if (ani.ParseException != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(EditorGUIUtility.IconContent("console.warningicon").image, new GUIStyle()
                    {
                        fixedWidth = 24f,
                        fixedHeight = 24f
                    });
                    EditorGUILayout.LabelField("The function cannot be debugged, possibly due to reliance on dynamic resources or other issues.", new GUIStyle(EditorStyles.boldLabel)
                    {
                        wordWrap = true
                    });
                    EditorGUILayout.EndHorizontal();

                    _status[aniIndex].ViewDetail = EditorGUILayout.Foldout(_status[aniIndex].ViewDetail, "View Exception");
                    if (_status[aniIndex].ViewDetail)
                    {
                        EditorGUILayout.LabelField(ani.ParseException.Message);
                        EditorGUILayout.LabelField(ani.ParseException.StackTrace);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(ani.ParsedAnimator.Collection.Count + " parts included.");
                    
                    _status[aniIndex].ViewDetail = EditorGUILayout.Foldout(_status[aniIndex].ViewDetail, "Debug");
                    if (_status[aniIndex].ViewDetail)
                    {
                        EditorGUILayout.Slider($"{ani.ParsedAnimator.Time:F3}s/{ani.AnimationLength:F3}s",
                            ani.ParsedAnimator.Time, 0f, ani.AnimationLength);
                        
                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("Play"))
                        {
                            Playing = true;
                            MilInstantAnimatorManager.ResetEditorTime();
                            ani.ParsedAnimator.Play();
                        }
                        if (GUILayout.Button("Pause"))
                        {
                            Playing = true;
                            ani.ParsedAnimator.Pause();
                        }
                        if (GUILayout.Button("Reset"))
                        {
                            Playing = true;
                            ani.ParsedAnimator.Reset();
                        }
                        
                        EditorGUILayout.EndHorizontal();
                        
                        var index = 0;
                        var time = 0f;
                        foreach (var part in ani.ParsedAnimator.Collection)
                        {
                            EditorGUILayout.BeginVertical(panelStyle);
                            _status[aniIndex].PartView[index] = EditorGUILayout.Foldout(_status[aniIndex].PartView[index], $"<b>Part {index}</b>: <color=cyan>+{time}s</color>, {part.Count} animations", new GUIStyle(EditorStyles.foldout)
                            {
                                normal =
                                {
                                    textColor = ani.ParsedAnimator.PlayIndex == index ? Color.cyan : Color.gray
                                },
                                fontSize = 14,
                                fontStyle = FontStyle.Bold,
                                richText = true
                            });

                            if (_status[aniIndex].PartView[index])
                            {
                                foreach (var clip in part)
                                {
                                    var color = Color.white;
                                    if (ani.ParsedAnimator.PlayIndex == index)
                                    {
                                        if (ani.ParsedAnimator.Time >= clip.GetStartTime() && ani.ParsedAnimator.Time <= clip.GetStartTime() + clip.GetDuration())
                                        {
                                            color = Color.cyan;
                                        }
                                    }
                                    clip.DrawWindow(time, color);
                                }
                            }

                            var newTime = 0f;
                            foreach (var clip in part)
                            {
                                newTime = Math.Max(newTime, time + clip.GetStartTime());
                            }

                            time = newTime;
                            
                            index++;
                            EditorGUILayout.EndVertical();
                        }
                    }
                    
                }
            
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();

            if (Playing)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                Repaint();
            }
        }
    }
}
#endif
