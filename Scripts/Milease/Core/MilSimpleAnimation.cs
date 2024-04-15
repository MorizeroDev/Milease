using System;
using System.Collections.Generic;

namespace Milease.Core
{
    public class MilSimpleAnimation
    {
        public readonly List<List<RuntimeAnimationPart>> Collection = new();
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
            MilSimpleAnimator.Instance.Animations.Remove(this);
        }

        public void Start()
        {
            if (MilSimpleAnimator.Instance.Animations.Contains(this))
            {
                Reset();
                return;
            }

            if (PlayIndex >= Collection.Count)
            {
                Reset();
            }
            MilSimpleAnimator.Instance.Animations.Add(this);
        }

        public void Stop()
        {
            Reset();
            Pause();
        }
    }
}