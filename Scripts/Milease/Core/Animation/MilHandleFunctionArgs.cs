using Milease.Core.Animator;

namespace Milease.Core.Animation
{
    public struct MilHandleFunctionArgs
    {
        internal object target;
        public float Progress { get; internal set; }
        public RuntimeAnimationPart Animation { get; internal set; }
        public MilInstantAnimator Animator { get; internal set; }

        public T GetTarget<T>()
        {
            return (T)target;
        }

        public object GetTarget()
        {
            return target;
        }
    }

    public delegate void MileaseHandleFunction(MilHandleFunctionArgs e);
}