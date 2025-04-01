using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milease.Core.Animation;
using Milease.Core.Manager;
using Milease.Enums;
using Milease.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milease.Core.Animator
{
    public abstract class MilStateParameter
    {
        public EaseType EaseType;
        public EaseFunction EaseFunction;
        public AnimationCurve CustomCurve;
        
        internal string MemberHash;
        
        internal abstract void Prepare();
        internal abstract void ApplyState(float pro);
    }
    
    public class MilStateAnimator
    {
        public readonly List<MilStateAnimation.AnimationState> StateList = new List<MilStateAnimation.AnimationState>();
        public MilStateAnimation.AnimationState CurrentAnimationState;
        public int CurrentState;
        
        public TimeSource TimeSource { get; set; } = TimeSource.UnScaledTime;
        
        internal float Time;
        internal string ActiveScene;
        internal bool dontStopOnLoad = false;
        
        public bool IsWorking()
        {
            return Time < CurrentAnimationState.Duration;
        }
        
        public MilStateAnimator DontStopOnLoad()
        {
            dontStopOnLoad = true;
            return this;
        }
        
        public MilStateAnimator SetTimeSource(TimeSource timeSource)
        {
            TimeSource = timeSource;
            return this;
        }
        
        /// <summary>
        /// Set the state, but without transition, it will be changed immediately.
        /// </summary>
        /// <param name="state">target state</param>
        public void SetState<I>(I state) where I : Enum
        {
            CurrentState = Convert.ToInt32(state);
            CurrentAnimationState = StateList.Find(x => x.StateID == CurrentState);
            foreach (var val in CurrentAnimationState.Values)
            {
                val.Prepare();
                val.ApplyState(1f);
            }
            Time = CurrentAnimationState.Duration;
        }
        
        /// <summary>
        /// Transform to the target state with animations
        /// </summary>
        /// <param name="state">target state</param>
        public void Transition<I>(I state) where I : Enum
        {
            CurrentState = Convert.ToInt32(state);
            CurrentAnimationState = StateList.Find(x => x.StateID == CurrentState);
            foreach (var val in CurrentAnimationState.Values)
            {
                val.Prepare();
            }
            Time = 0f;
        }

        public bool IsStateEmpty<I>(I state) where I : Enum
        {
            return StateList.Find(x => x.StateID == Convert.ToInt32(state)).Values.Count == 0;
        }
        
        public MilStateAnimator ModifyState<I, T, E>(I stateID, T target, string member, E value) where I : Enum
        {
            var id = Convert.ToInt32(stateID);
            var state = StateList.Find(x => x.StateID == id);
            if (state == null)
            {
                LogUtils.Warning($"Required state {stateID} not found.");
                return this;
            }

            var hash = target.GetHashCode() + member;
            var index = state.Values.FindIndex(x => x.MemberHash == hash);
            if (index == -1)
            {
                LogUtils.Warning($"Required member {member} not found.");
            }
            else
            {
                ((MilStateAnimation.AnimationValue<T, E>)state.Values[index]).ToValue = value;
            }
            
            return this;
        }
        
        public MilStateAnimator ModifyState<I>(I stateID, IEnumerable<MilStateParameter> states) where I : Enum
        {
            var id = Convert.ToInt32(stateID);
            var state = StateList.Find(x => x.StateID == id);
            if (state == null)
            {
                LogUtils.Warning($"Required state {stateID} not found.");
                return this;
            }
            foreach (var val in states)
            {
                var index = state.Values.FindIndex(x => x.MemberHash == val.MemberHash);
                if (index == -1)
                {
                    state.Values.Add(val);
                }
                else
                {
                    state.Values[index] = val;
                }
            }
            return this;
        }
        
        public MilStateAnimator AddState<I>(I stateID, float duration, IEnumerable<MilStateParameter> states) where I : Enum
        {
            var state = new MilStateAnimation.AnimationState()
            {
                StateID = Convert.ToInt32(stateID),
                Duration = duration
            };
            state.Values.AddRange(states);
            StateList.Add(state);
            return this;
        }

        public MilStateAnimator SetDefaultState<I>(I state) where I : Enum
        {
            CurrentState = Convert.ToInt32(state);
            CurrentAnimationState = StateList.Find(x => x.StateID == CurrentState);
            foreach (var val in CurrentAnimationState.Values)
            {
                val.Prepare();
                val.ApplyState(1f);
            }
            Time = CurrentAnimationState.Duration;
            MilStateAnimatorManager.EnsureInitialized();
            if (!MilStateAnimatorManager.Animators.Contains(this))
            {
                MilStateAnimatorManager.Animators.Add(this);
                ActiveScene = SceneManager.GetActiveScene().name;
            }
            return this;
        }

        public void Stop()
        {
            MilStateAnimatorManager.Animators.Remove(this);
        }
    }
}
