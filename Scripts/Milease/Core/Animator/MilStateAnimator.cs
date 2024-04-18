using System.Collections.Generic;
using Milease.Core.Animation;
using Milease.Core.Manager;
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
        
        public void Transition(int state)
        {
            CurrentState = state;
            CurrentAnimationState = StateList.Find(x => x.StateID == state);
            foreach (var val in CurrentAnimationState.Values)
            {
                MilStateAnimation.PrepareState(val);
            }
            Time = 0f;
        }

        public MilStateAnimator AddState(int stateID, float duration, MilStateParameter[] states)
        {
            var state = new MilStateAnimation.AnimationState()
            {
                StateID = stateID,
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

        public MilStateAnimator SetDefaultState(int state)
        {
            CurrentState = state;
            CurrentAnimationState = StateList.Find(x => x.StateID == state);
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