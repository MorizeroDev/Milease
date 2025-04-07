using System.Runtime.CompilerServices;
using Milease.Core.Animation;
using UnityEngine;

namespace Milease.Core
{
    public interface IAnimationController
    {
        public string MemberPath { get; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Apply(float progress);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Delay(float time);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetDuration(float duration);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetBlendingMode(BlendingMode mode);

        public bool Reset(AnimationResetMode resetMode, bool revertToChanges = true);

        internal void ResetAnimation();

#if UNITY_EDITOR
        internal void DrawWindow(float time, Color color);

        internal float GetStartTime();
        
        internal float GetDuration();
#endif
    }
}
