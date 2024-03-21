using System;
using System.Collections.Generic;
using Milease.Utils;
using UnityEngine;

namespace Milease.Core
{
    [CreateAssetMenu(menuName = "Milease/New MilAnimation", fileName = "New MilAnimation")]
    public class MilAnimation : ScriptableObject
    {
        [Serializable]
        public struct AnimationPart
        {
            public float StartTime;
            public float Duration;
            public EaseUtility.EaseType EaseType;
            public EaseUtility.EaseFunction EaseFunction;
            public AnimationCurve CustomCurve;
            public List<string> Binding;
            public string StartValue;
            public string ToValue;
        }
        [HideInInspector]
        public List<AnimationPart> Parts;
    }
}