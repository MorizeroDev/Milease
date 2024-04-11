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
            public readonly List<MilAnimation.RuntimeAnimationPart> Collection = new();
            internal float Time = 0f;

            public MilSimpleAnimation Then(MilSimpleAnimation animation)
            {
                foreach (var part in animation.Collection)
                {
                    Collection.Add(part);
                }

                Instance.Animations.Remove(animation);
                return this;
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
                var ani = set.Collection[0];
                MilAnimation.RuntimeAnimationPart.SetValue(ani, EaseUtility.GetEasedProgress(set.Time, ani.Source.EaseType, ani.Source.EaseFunction));
                set.Time += deltaTime;
                if (set.Time > ani.Source.Duration)
                {
                    set.Time -= ani.Source.Duration;
                    set.Collection.RemoveAt(0);
                }

                if (set.Collection.Count == 0)
                {
                    Animations.RemoveAt(i);
                    i--;
                    cnt--;
                }
            }
        }
    }
}