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
            public float EditorTime;
        }
        
        [SerializeField]
        private Object _objectSrc;
        
        private List<InstantAnimatorViewer.AnimatorFunction> _functions;

        [SerializeField] 
        private List<EditorStatus> _status;
        
        [SerializeField] 
        private Vector2 scrollPos;

        [SerializeField]
        private float speedFactor = 1f;

        internal static float SpeedFactor = 1f;
        
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
                ani.ParsedAnimator.Pause();
            }
        }

        private void OnDisable()
        {
            ResetAnimators();
            EditorSceneManager.sceneClosing -= EditorSceneManagerOnsceneClosing;
            EditorSceneManager.sceneSaving -= EditorSceneManagerOnsceneSaving;
        }

        private void SetAnimatorTime(InstantAnimatorViewer.AnimatorFunction func, float time)
        {
            var startIndex = 0;
            for (; startIndex < func.ClipStartTime.Length; startIndex++)
            {
                if (func.ClipStartTime[startIndex] > time)
                {
                    break;
                }
            }

            func.ParsedAnimator.EditorEndPlayIndex = -1;
            MilInstantAnimatorManager.ResetEditorTime();
            func.ParsedAnimator.SetTime(startIndex - 1, time, func.ClipStartTime);
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
            
            EditorGUILayout.BeginVertical(panelStyle);
            
            EditorGUILayout.LabelField("General", titleStyle);
            speedFactor = EditorGUILayout.Slider("Play Speed", speedFactor, 0f, 2f);
            SpeedFactor = speedFactor;
            
            EditorGUILayout.EndVertical();

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

                    if (_status[i].EditorTime > 0f)
                    {
                        SetAnimatorTime(_functions[i], _status[i].EditorTime);
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
                        var time = 
                            ani.ParsedAnimator.PlayIndex >= ani.ParsedAnimator.Collection.Count
                            ? ani.AnimationLength
                            : ani.ClipStartTime[ani.ParsedAnimator.PlayIndex] + ani.ParsedAnimator.Time;
                        
                        EditorGUILayout.BeginVertical(panelStyle);
                        
                        EditorGUILayout.LabelField($"Playing part {ani.ParsedAnimator.PlayIndex}/{ani.ClipStartTime.Length}...", new GUIStyle(EditorStyles.boldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter
                        });
                        
                        var newTime = EditorGUILayout.Slider(
                            $"{time:F3}s/{ani.AnimationLength:F3}s",
                            time, 0f, ani.AnimationLength);
                        _status[aniIndex].EditorTime = newTime;
                        if (!Mathf.Approximately(time, newTime))
                        {
                            SetAnimatorTime(ani, newTime);
                        }
                        
                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button("Play"))
                        {
                            Playing = true;
                            ani.ParsedAnimator.EditorEndPlayIndex = -1;
                            MilInstantAnimatorManager.ResetEditorTime();
                            ani.ParsedAnimator.Play();
                        }
                        if (GUILayout.Button("Pause"))
                        {
                            Playing = true;
                            ani.ParsedAnimator.Pause();
                        }
                        if (GUILayout.Button("Stop"))
                        {
                            Playing = true;
                            ani.ParsedAnimator.Reset();
                        }
                        
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUILayout.EndVertical();
                        
                        EditorGUILayout.Separator();
                        
                        var index = 0;
                        foreach (var part in ani.ParsedAnimator.Collection)
                        {
                            EditorGUILayout.BeginVertical(panelStyle);
                            
                            EditorGUILayout.BeginHorizontal();
                            _status[aniIndex].PartView[index] = EditorGUILayout.Foldout(_status[aniIndex].PartView[index], 
                                (ani.ParsedAnimator.PlayIndex == index ? "<color=yellow>" : "") +
                                $"<b>Part {index}</b>: <color=cyan>+{ani.ClipStartTime[index]:F3}s</color>, {part.Count} animations"
                                + (ani.ParsedAnimator.PlayIndex == index ? "</color>" : ""), 
                                new GUIStyle(EditorStyles.foldout)
                                {
                                    fontSize = 14,
                                    fontStyle = FontStyle.Bold,
                                    richText = true
                                });

                            if (GUILayout.Button("...", new GUIStyle(EditorStyles.miniButton)
                                {
                                    fixedWidth = 40f
                                }))
                            {
                                var menu = new GenericMenu();
                                var i = index;
                                menu.AddItem(new GUIContent("Play from this part..."), false, () =>
                                {
                                    ani.ParsedAnimator.EditorEndPlayIndex = -1;
                                    MilInstantAnimatorManager.ResetEditorTime();
                                    ani.ParsedAnimator.SetTime(i, ani.ClipStartTime[i], ani.ClipStartTime, true);
                                });
                                menu.AddItem(new GUIContent("Play this part only..."), false, () =>
                                {
                                    ani.ParsedAnimator.EditorEndPlayIndex = i + 1;
                                    MilInstantAnimatorManager.ResetEditorTime();
                                    ani.ParsedAnimator.SetTime(i, ani.ClipStartTime[i], ani.ClipStartTime, true);
                                });
                                menu.ShowAsContext();
                            }
                            
                            EditorGUILayout.EndHorizontal();
                            
                            EditorGUILayout.Separator();

                            if (_status[aniIndex].PartView[index])
                            {
                                foreach (var clip in part)
                                {
                                    var color = Color.white;
                                    if (ani.ParsedAnimator.PlayIndex == index)
                                    {
                                        if (ani.ParsedAnimator.Time >= clip.GetStartTime() && ani.ParsedAnimator.Time <= clip.GetStartTime() + clip.GetDuration())
                                        {
                                            color = Color.yellow;
                                        }
                                    }
                                    clip.DrawWindow(ani.ClipStartTime[index], color);
                                }
                            }
                            
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
