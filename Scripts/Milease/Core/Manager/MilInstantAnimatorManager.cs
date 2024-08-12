using System.Collections.Generic;
using Milease.Core.Animator;
using Milease.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milease.Core.Manager
{
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
            var go = new GameObject("[MilInstantAnimatorManager]", typeof(MilInstantAnimatorManager));
            DontDestroyOnLoad(go);
            go.SetActive(true);
            Instance = go.GetComponent<MilInstantAnimatorManager>();
            SceneManager.sceneUnloaded += (scene) =>
            {
                Animations.RemoveAll(x => !x.dontStopOnLoad && x.ActiveScene == scene.name);
            };
        }

        public static readonly List<MilInstantAnimator> Animations = new();

        private void Update()
        {
            var cnt = Animations.Count;
            var deltaTime = Time.deltaTime;
            
            for (var i = 0; i < cnt; i++)
            {
                var set = Animations[i];
                var collection = set.Collection[set.PlayIndex];
                var cCnt = collection.Count;
                var latestTime = 0f;
                set.Time += deltaTime;
                for (var j = 0; j < cCnt; j++)
                {
                    var ani = collection[j];

                    latestTime = Mathf.Max(latestTime, ani.Source.StartTime + ani.Source.Duration);
                    if (set.Time < ani.Source.StartTime)
                    {
                        continue;
                    }
                    
                    var pro = 1f;
                    if (ani.Source.Duration > 0f)
                    {
                        pro = Mathf.Clamp((set.Time - ani.Source.StartTime) / ani.Source.Duration, 0f, 1f);
                    }

                    var easedPro = ani.Source.CustomCurve?.Evaluate(pro) ?? EaseUtility.GetEasedProgress(pro, ani.Source.EaseType, ani.Source.EaseFunction);
                    RuntimeAnimationPart.SetValue(ani, easedPro);
                }

                if (set.Time >= latestTime)
                {
                    set.Time -= latestTime;
                    set.PlayIndex++;
                    if (set.PlayIndex >= set.Collection.Count)
                    {
                        set.PlayCallback?.Invoke();
                        Animations.RemoveAt(i);
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
        }
    }
}
