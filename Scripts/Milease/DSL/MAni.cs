using Milease.Core.Animator;

namespace Milease.DSL
{
    // Short written for MilInstantAnimator.Start
    public static class MAni
    {
        public static MilInstantAnimator Make(MilInstantAnimator animator)
        {
            return MilInstantAnimator.Start(animator);
        }
        
        public static MilInstantAnimator Make(params MilInstantAnimator[] animators)
        {
            return MilInstantAnimator.Start(animators);
        }
    }
}
