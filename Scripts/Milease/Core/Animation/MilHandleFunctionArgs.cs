using Milease.Core.Animator;

namespace Milease.Core.Animation
{
    public struct MilHandleFunctionArgs<T, E>
    {
        public T Target { get; internal set; }
        public float Progress { get; internal set; }
        public RuntimeAnimationPart<T, E> Animation { get; internal set; }
        public MilInstantAnimator Animator { get; internal set; }
    }

    public delegate void MileaseHandleFunction<T, E>(MilHandleFunctionArgs<T, E> e);
}
