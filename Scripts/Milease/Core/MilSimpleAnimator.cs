using System;
using System.Collections.Generic;
using Milease.Utils;
using UnityEngine;

namespace Milease.Core
{
    public class MilSimpleAnimator : MonoBehaviour
    {
        public class MilSimpleAnimation
        {
            public readonly List<List<MilAnimation.RuntimeAnimationPart>> Collection = new();
            internal int PlayIndex = 0;
            internal float Time = 0f;

            public static MilSimpleAnimation Empty()
            {
                return new MilSimpleAnimation();
            }
            
            public MilSimpleAnimation Delayed(float time)
            {
                foreach (var part in Collection[^1])
                {
                    part.Source.StartTime += time;
                }
                return this;
            }
            
            public MilSimpleAnimation While(MilSimpleAnimation animation)
            {
                foreach (var part in animation.Collection)
                {
                    foreach (var ani in part)
                    {
                        Collection[^1].Add(ani);
                    }
                }
                
                return this;
            }
            
            public MilSimpleAnimation Then(MilSimpleAnimation animation)
            {
                foreach (var part in animation.Collection)
                {
                    Collection.Add(part);
                }
                
                return this;
            }

            public void Reset()
            {
                Time = 0f;
                var paths = new List<string>();
                var cnt = Collection.Count;
                for (var i = 0; i <= Math.Min(PlayIndex, cnt - 1); i++)
                {
                    foreach (var ani in Collection[i])
                    {
                        if (paths.Contains(ani.MemberPath))
                            continue;
                        if (ani.Reset())
                        {
                            paths.Add(ani.MemberPath);
                        }
                    }
                }
                PlayIndex = 0;
            }

            public void Pause()
            {
                Instance.Animations.Remove(this);
            }

            public void Start()
            {
                if (Instance.Animations.Contains(this))
                {
                    Reset();
                    return;
                }

                if (PlayIndex >= Collection.Count)
                {
                    Reset();
                }
                Instance.Animations.Add(this);
            }

            public void Stop()
            {
                Reset();
                Pause();
            }
        }
        
        public static readonly MilSimpleAnimator Instance;

        static MilSimpleAnimator()
        {
            var go = new GameObject("[MilSimpleAnimator]", typeof(MilSimpleAnimator));
            DontDestroyOnLoad(go);
            go.SetActive(true);
            Instance = go.GetComponent<MilSimpleAnimator>();
        }

        public readonly List<MilSimpleAnimation> Animations = new();

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
                    var pro = 1f;
                    if (ani.Source.Duration > 0f)
                    {
                        pro = Mathf.Clamp((set.Time - ani.Source.StartTime) / ani.Source.Duration, 0f, 1f);
                    }
                    MilAnimation.RuntimeAnimationPart.SetValue(
                        ani, 
                        EaseUtility.GetEasedProgress(pro, ani.Source.EaseType, ani.Source.EaseFunction)
                        );
                    latestTime = Mathf.Max(latestTime, ani.Source.StartTime + ani.Source.Duration);
                }

                if (set.Time >= latestTime)
                {
                    set.Time -= latestTime;
                    set.PlayIndex++;
                    if (set.PlayIndex >= set.Collection.Count)
                    {
                        Animations.RemoveAt(i);
                        i--;
                        cnt--;
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