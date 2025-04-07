using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Utils;
#if UNITY_EDITOR
using Milease.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Milease.Core.Manager
{
    [ExecuteAlways]
    public class MilInstantAnimatorManager : MonoBehaviour
    {
        public static MilInstantAnimatorManager Instance;
        internal static string CurrentScene;

        public static void EnsureInitialized()
        {
            if (Instance)
            {
                return;
            }
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                var editorGo = new GameObject("[MilInstantAnimatorManager]", typeof(MilInstantAnimatorManager))
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                editorGo.SetActive(true);
                Instance = editorGo.GetComponent<MilInstantAnimatorManager>();
                Instance.EditorMode = true;
                lstTime = (float)EditorApplication.timeSinceStartup;
                return;
            }
#endif
            var go = new GameObject("[MilInstantAnimatorManager]", typeof(MilInstantAnimatorManager));
            DontDestroyOnLoad(go);
            go.SetActive(true);
            Instance = go.GetComponent<MilInstantAnimatorManager>();
            SceneManager.sceneUnloaded += (scene) =>
            {
                _animations.RemoveAll(x => !x.dontStopOnLoad && x.ActiveScene == scene.name);
            };
        }

        private static readonly List<MilInstantAnimator> _animations = new List<MilInstantAnimator>();
        private static readonly HashSet<MilInstantAnimator> _aniHashSet = new HashSet<MilInstantAnimator>();

        internal static bool IsPlayTaskActive(MilInstantAnimator animator)
            => _aniHashSet.Contains(animator);
        
        internal static void SubmitPlayTask(MilInstantAnimator animator)
        {
            if (_aniHashSet.Contains(animator))
            {
                return;
            }
            _animations.Add(animator);
            _aniHashSet.Add(animator);
        }
        
        internal static void CancelPlayTask(MilInstantAnimator animator)
        {
            if (!_aniHashSet.Contains(animator))
            {
                return;
            }
            _animations.Remove(animator);
            _aniHashSet.Remove(animator);
        }

#if UNITY_EDITOR
        internal static void ResetEditorTime()
        {
            lstTime = (float)EditorApplication.timeSinceStartup;
        }
#endif
        
        internal bool EditorMode;
        private static float lstTime;
        
        private void Update()
        {
            var cnt = _animations.Count;
            var scaledDeltaTime = Time.deltaTime;
            var unscaledDeltaTime = Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);

#if UNITY_EDITOR
            if (Instance != this)
            {
                return;
            }
            if (EditorMode)
            {
                scaledDeltaTime = unscaledDeltaTime = (float)EditorApplication.timeSinceStartup - lstTime;
                lstTime = (float)EditorApplication.timeSinceStartup;
                //Debug.Log("Tick " + scaledDeltaTime);
            }
#endif
            
            for (var i = 0; i < cnt; i++)
            {
                var set = _animations[i];
                var collection = set.Collection[set.PlayIndex];
                var cCnt = collection.Count;
                var latestTime = 0f;
                set.Time += (set.TimeSource == TimeSource.UnScaledTime ? unscaledDeltaTime : scaledDeltaTime);
                for (var j = 0; j < cCnt; j++)
                {
                    var ani = (RuntimeAnimationBase)collection[j];

                    latestTime = Mathf.Max(latestTime, ani.ControlInfo.StartTime + ani.ControlInfo.Duration);
                    if (set.Time < ani.ControlInfo.StartTime)
                    {
                        continue;
                    }
                    
                    var pro = 1f;
                    if (ani.ControlInfo.Duration > 0f)
                    {
                        pro = Mathf.Clamp((set.Time - ani.ControlInfo.StartTime) / ani.ControlInfo.Duration, 0f, 1f);
                    }
                    var easedPro = EaseUtility.GetEasedProgress(pro, ani.ControlInfo.EaseType, ani.ControlInfo.EaseFunction);
                    collection[j].Apply(easedPro);
                }

                if (set.Time >= latestTime)
                {
                    set.Time -= latestTime;
                    set.PlayIndex++;
                    if (set.PlayIndex >= set.Collection.Count)
                    {
                        set.PlayCallback?.Invoke();
                        _aniHashSet.Remove(_animations[i]);
                        _animations.RemoveAt(i);
                        i--;
                        cnt--;
                        if (set.Loop)
                        {
                            set.Play(set.PlayCallback);
                        }
                    }
                    else
                    {
                        i--;
                        continue;
                    }
                }
            }
            
#if UNITY_EDITOR
            if (EditorMode)
            {
                SceneView.RepaintAll();
            }

            if (_animations.Count == 0)
            {
                InstantAnimatorDebugWindow.Playing = false;
            }
#endif
        }
    }
}
