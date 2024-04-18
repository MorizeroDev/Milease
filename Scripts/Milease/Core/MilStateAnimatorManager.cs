using System;
using System.Collections.Generic;
using Milease.Utils;
using UnityEngine;

namespace Milease.Core
{
    public class MilStateAnimatorManager : MonoBehaviour
    {
        public static readonly MilStateAnimatorManager Instance;
        public static readonly List<MilStateAnimator> Animators = new();
        
        static MilStateAnimatorManager()
        {
            var go = new GameObject("[MilStateAnimatorManager]", typeof(MilStateAnimatorManager));
            DontDestroyOnLoad(go);
            go.SetActive(true);
            Instance = go.GetComponent<MilStateAnimatorManager>();
        }
        
        private void Update()
        {
            var cnt = Animators.Count;
            for (var i = 0; i < cnt; i++)
            {
                var animator = Animators[i];
                if (animator.Time >= animator.CurrentAnimationState.Duration)
                    continue;
                animator.Time += Time.deltaTime;
                var pro = Mathf.Min(1f, animator.Time / animator.CurrentAnimationState.Duration);
                foreach (var val in animator.CurrentAnimationState.Values)
                {
                    var easedPro = val.CustomCurve?.Evaluate(pro) ?? EaseUtility.GetEasedProgress(pro, val.EaseType, val.EaseFunction);
                    MilStateAnimation.ApplyState(val, easedPro);
                }
            }
        }
    }
}