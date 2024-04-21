﻿using System;
using System.Collections.Generic;
using Milease.Core.Animation;
using Milease.Core.Manager;
using Milease.Enums;
using UnityEngine;

namespace Milease.Core.Animator
{
    public class MilStateParameter
    {
        public object Target;
        public string Member;
        public EaseFunction EaseFunction;
        public EaseType EaseType;
        public AnimationCurve CustomCurve;
        public object ToValue;
    }
    public class MilStateAnimator
    {
        public readonly List<MilStateAnimation.AnimationState> StateList = new();
        public MilStateAnimation.AnimationState CurrentAnimationState;
        public int CurrentState;
        internal float Time;
        
        /// <summary>
        /// Set the state, but without transition, it will be changed immediately.
        /// </summary>
        /// <param name="state">target state</param>
        public void SetState<T>(T state) where T : Enum
        {
            CurrentState = Convert.ToInt32(state);
            CurrentAnimationState = StateList.Find(x => x.StateID == CurrentState);
            foreach (var val in CurrentAnimationState.Values)
            {
                MilStateAnimation.PrepareState(val);
                MilStateAnimation.ApplyState(val, 1f);
            }
            Time = CurrentAnimationState.Duration;
        }
        
        /// <summary>
        /// Transform to the target state with animations
        /// </summary>
        /// <param name="state">target state</param>
        public void Transition<T>(T state) where T : Enum
        {
            CurrentState = Convert.ToInt32(state);
            CurrentAnimationState = StateList.Find(x => x.StateID == CurrentState);
            foreach (var val in CurrentAnimationState.Values)
            {
                MilStateAnimation.PrepareState(val);
            }
            Time = 0f;
        }
        
        public MilStateAnimator ModifyState<T>(T stateID, object target, string member, object value) where T : Enum
        {
            var id = Convert.ToInt32(stateID);
            var state = StateList.Find(x => x.StateID == id);
            var index = state.Values.FindIndex(x => x.Target == target && x.Member == member);
            if (index == -1)
            {
                Debug.LogWarning($"Required member {member} not found.");
            }
            else
            {
                state.Values[index].ToValue = value;
            }
            
            return this;
        }
        
        public MilStateAnimator ModifyState<T>(T stateID, IEnumerable<MilStateParameter> states) where T : Enum
        {
            var id = Convert.ToInt32(stateID);
            var state = StateList.Find(x => x.StateID == id);
            foreach (var val in states)
            {
                var ani = new MilStateAnimation.AnimationValue(val.Target, val.Member)
                {
                    EaseType = val.EaseType,
                    EaseFunction = val.EaseFunction,
                    CustomCurve = val.CustomCurve,
                    ToValue = val.ToValue
                };
                var index = state.Values.FindIndex(x => x.Target == val.Target && x.Member == val.Member);
                if (index == -1)
                {
                    state.Values.Add(ani);
                }
                else
                {
                    state.Values[index] = ani;
                }
            }
            return this;
        }
        
        public MilStateAnimator AddState<T>(T stateID, float duration, IEnumerable<MilStateParameter> states) where T : Enum
        {
            var state = new MilStateAnimation.AnimationState()
            {
                StateID = Convert.ToInt32(stateID),
                Duration = duration
            };
            foreach (var val in states)
            {
                state.Values.Add(new MilStateAnimation.AnimationValue(val.Target, val.Member)
                {
                    EaseType = val.EaseType,
                    EaseFunction = val.EaseFunction,
                    CustomCurve = val.CustomCurve,
                    ToValue = val.ToValue
                });
            }
            StateList.Add(state);
            return this;
        }

        public MilStateAnimator SetDefaultState<T>(T state) where T : Enum
        {
            CurrentState = Convert.ToInt32(state);
            CurrentAnimationState = StateList.Find(x => x.StateID == CurrentState);
            foreach (var val in CurrentAnimationState.Values)
            {
                MilStateAnimation.PrepareState(val);
                MilStateAnimation.ApplyState(val, 1f);
            }
            Time = CurrentAnimationState.Duration;
            if (!MilStateAnimatorManager.Animators.Contains(this))
                MilStateAnimatorManager.Animators.Add(this);
            return this;
        }
    }
}