using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Milease.Core.Animation;
using Milease.Core.Manager;
using Milease.Enums;
using UnityEngine.SceneManagement;

namespace Milease.Core.Animator
{
    public class MilInstantAnimator
    {
        public readonly List<List<IAnimationController>> Collection = new List<List<IAnimationController>>();

        public AnimationResetMode DefaultResetMode = AnimationResetMode.ResetToOriginalState;

        public bool Loop { get; set; } = false;

        public TimeSource TimeSource { get; set; } = TimeSource.UnScaledTime;

        internal Action PlayCallback;
        
        internal int PlayIndex = 0;
        internal float Time = 0f;
        internal string ActiveScene;
        internal bool dontStopOnLoad = false;

        public static MilInstantAnimator Empty()
        {
            return new MilInstantAnimator()
            {
                Collection = { new List<IAnimationController>() }
            };
        }
        
        public static MilInstantAnimator Start(MilInstantAnimator animator)
        {
            return Empty().And(animator);
        }
        
        public static MilInstantAnimator Start(params MilInstantAnimator[] animators)
        {
            return Empty().And(animators);
        }
        
        public MilInstantAnimator DontStopOnLoad()
        {
            dontStopOnLoad = true;
            return this;
        }
        
        public MilInstantAnimator SetTimeSource(TimeSource timeSource)
        {
            TimeSource = timeSource;
            return this;
        }
        
        public MilInstantAnimator UsingResetMode(AnimationResetMode mode)
        {
            DefaultResetMode = mode;
            return this;
        }
        
        public MilInstantAnimator EnableLooping()
        {
            Loop = true;
            return this;
        }
        
        public MilInstantAnimator Delayed(float time)
        {
            foreach (var part in Collection[Collection.Count - 1])
            {
                part.Delay(time);
            }
            return this;
        }

        [Obsolete]
        public MilInstantAnimator While(params MilInstantAnimator[] animations)
        {
            foreach (var ani in animations)
            {
                And(ani);
            }
            
            return this;
        }
        
        [Obsolete]
        public MilInstantAnimator While(MilInstantAnimator animation)
            => And(animation);
        
        public MilInstantAnimator And(params MilInstantAnimator[] animations)
        {
            foreach (var ani in animations)
            {
                And(ani);
            }
            
            return this;
        }
        
        public MilInstantAnimator And(MilInstantAnimator animation)
        {
            foreach (var part in animation.Collection)
            {
                foreach (var ani in part)
                {
                    Collection[Collection.Count - 1].Add(ani);
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
                And(animations[i]);
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
        
        /// <summary>
        /// Revert changes by the animator
        /// </summary>
        /// <param name="mode">Reset mode</param>
        /// <param name="revertToChanges">Also revert MileaseTo changes</param>
        public void Reset(
            AnimationResetMode mode = AnimationResetMode.ResetToOriginalState,
            bool revertToChanges = true)
        {
            Time = 0f;
            var paths = new List<string>();
            var cnt = Collection.Count;
            var resetCount = mode switch
            {
                AnimationResetMode.ResetToOriginalState => Math.Min(PlayIndex + 1, cnt),
                AnimationResetMode.ResetToInitialState => cnt,
                _ => 0
            };
            
            for (var i = 0; i < resetCount; i++)
            {
                foreach (var ani in Collection[i])
                {
                    if (paths.Contains(ani.MemberPath))
                        continue;
                    if (ani.Reset(mode, revertToChanges))
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
            MilInstantAnimatorManager.CancelPlayTask(this);
        }

        /// <summary>
        /// Play the animation
        /// </summary>
        /// <param name="callback">Callback for when the animation finishes playing. Please <b>NOTE</b>, if you call
        /// this function again and pass a new callback before the animation has finished,
        /// the original callback will be overwritten.</param>
        public void Play(Action callback = null, bool revertToChanges = true)
            => Play(false, callback, revertToChanges);
        
        /// <summary>
        /// Play the animation right now
        /// Comparing to Play(), it will start animation right away, instead of waiting for the next frame.
        /// </summary>
        /// <param name="callback">Callback for when the animation finishes playing. Please <b>NOTE</b>, if you call
        /// this function again and pass a new callback before the animation has finished,
        /// the original callback will be overwritten.</param>
        public void PlayImmediately(Action callback = null, bool revertToChanges = true)
            => Play(true, callback, revertToChanges);
        
        private void Play(bool immediate, Action callback = null, bool revertToChanges = true)
        {
            PlayCallback = callback;
            
            if (MilInstantAnimatorManager.IsPlayTaskActive(this))
            {
                Reset(DefaultResetMode, revertToChanges);
                return;
            }

            if (PlayIndex >= Collection.Count || DefaultResetMode == AnimationResetMode.ResetToInitialState)
            {
                Reset(DefaultResetMode, revertToChanges);
            }
            MilInstantAnimatorManager.EnsureInitialized();
            if (immediate && Collection.Count > 0)
            {
                foreach (var ani in Collection[0])
                {
                    ApplyAnimation(ani, 0f);
                }
            }
            MilInstantAnimatorManager.SubmitPlayTask(this);
            ActiveScene = SceneManager.GetActiveScene().name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyAnimation(IAnimationController ani, float pro)
        {
            if (!ani.ShouldUpdate(pro))
            {
                return;
            }
            
            ani.SetOriginalValue();
            
            if (ani.InvokeSelfHandling(pro))
            {
                return;
            }
            
            ani.Apply(pro);
        }
        
        public void Stop()
        {
            Reset(DefaultResetMode);
            Pause();
        }

        #region DSL Support

        /// <summary>
        /// To set the duration of an animation set
        /// </summary>
        public static MilInstantAnimator operator /(float duration, MilInstantAnimator animator)
        {
            if (animator.Collection.Count == 0)
            {
                return animator;
            }
            
            foreach (var ani in animator.Collection[0])
            {
                ani.SetDuration(duration);
            }

            return animator;
        }
        
        /// <summary>
        /// To delay an animation set
        /// </summary>
        public static MilInstantAnimator operator +(float delay, MilInstantAnimator animator)
        {
            return animator.Delayed(delay);
        }

        public static MilInstantAnimator operator -(MilInstantAnimator animator, BlendingMode blendingMode)
        {
            if (animator.Collection.Count == 0)
            {
                return animator;
            }
            
            foreach (var ani in animator.Collection[0])
            {
                ani.SetBlendingMode(blendingMode);
            }

            return animator;
        }
        
        #endregion
    }
}
