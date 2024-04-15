using System;
using System.Collections.Generic;
using Milease.Utils;
using UnityEngine;

namespace Milease.Core
{
    public class MilSimpleAnimator : MonoBehaviour
    {
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

                    var easedPro = ani.Source.CustomCurve?.Evaluate(pro) ?? EaseUtility.GetEasedProgress(pro, ani.Source.EaseType, ani.Source.EaseFunction);
                    RuntimeAnimationPart.SetValue(ani, easedPro);
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