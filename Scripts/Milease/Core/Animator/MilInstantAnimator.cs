﻿using System;
using System.Collections.Generic;
using Milease.Core.Manager;

namespace Milease.Core.Animator
{
    public class MilInstantAnimator
    {
        public readonly List<List<RuntimeAnimationPart>> Collection = new();

        public RuntimeAnimationPart.AnimationResetMode DefaultResetMode =
            RuntimeAnimationPart.AnimationResetMode.ResetToOriginalState;
        
        internal int PlayIndex = 0;
        internal float Time = 0f;

        public static MilInstantAnimator Empty()
        {
            return new MilInstantAnimator();
        }
        
        public MilInstantAnimator UsingResetMode(RuntimeAnimationPart.AnimationResetMode mode)
        {
            DefaultResetMode = mode;
            return this;
        }
        
        public MilInstantAnimator Delayed(float time)
        {
            foreach (var part in Collection[^1])
            {
                part.Source.StartTime += time;
            }
            return this;
        }
        
        public MilInstantAnimator While(params MilInstantAnimator[] animations)
        {
            foreach (var ani in animations)
            {
                While(ani);
            }
            
            return this;
        }
        
        public MilInstantAnimator While(MilInstantAnimator animation)
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
        
        public MilInstantAnimator ThenOneByOne(params MilInstantAnimator[] animations)
        {
            foreach (var ani in animations)
            {
                Then(ani);
            }
            
            return this;
        }
        
        public MilInstantAnimator Then(params MilInstantAnimator[] animations)
        {
            Then(animations[0]);
            var cnt = animations.Length;
            for (var i = 1; i < cnt; i++)
            {
                While(animations[i]);
            }
            
            return this;
        }
        
        public MilInstantAnimator Then(MilInstantAnimator animation)
        {
            foreach (var part in animation.Collection)
            {
                Collection.Add(part);
            }
            
            return this;
        }
        
        public void Reset(RuntimeAnimationPart.AnimationResetMode mode = RuntimeAnimationPart.AnimationResetMode.ResetToOriginalState)
        {
            Time = 0f;
            var paths = new List<string>();
            var cnt = Collection.Count;
            var resetCount = mode switch
            {
                RuntimeAnimationPart.AnimationResetMode.ResetToOriginalState => Math.Min(PlayIndex + 1, cnt),
                RuntimeAnimationPart.AnimationResetMode.ResetToInitialState => cnt,
                _ => 0
            };
            
            for (var i = 0; i < resetCount; i++)
            {
                foreach (var ani in Collection[i])
                {
                    if (paths.Contains(ani.MemberPath))
                        continue;
                    if (ani.Reset(mode))
                    {
                        paths.Add(ani.MemberPath);
                    }
                }
            }
            for (var i = 0; i < cnt; i++)
            {
                foreach (var ani in Collection[i])
                {
                    ani.ResetAnimation();
                }
            }
            PlayIndex = 0;
        }

        public void Pause()
        {
            MilInstantAnimatorManager.Animations.Remove(this);
        }

        public void Play()
        {
            if (MilInstantAnimatorManager.Animations.Contains(this))
            {
                Reset(DefaultResetMode);
                return;
            }

            if (PlayIndex >= Collection.Count || DefaultResetMode == RuntimeAnimationPart.AnimationResetMode.ResetToInitialState)
            {
                Reset(DefaultResetMode);
            }
            MilInstantAnimatorManager.EnsureInitialized();
            MilInstantAnimatorManager.Animations.Add(this);
        }

        public void Stop()
        {
            Reset(DefaultResetMode);
            Pause();
        }
    }
}