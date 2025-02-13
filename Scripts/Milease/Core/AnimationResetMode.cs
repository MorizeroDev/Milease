namespace Milease.Core
{
    public enum AnimationResetMode
    {
        /// <summary>
        /// Resets to the state of the target object before it was affected by the animator (undo animator changes)
        /// </summary>
        ResetToOriginalState,

        /// <summary>
        /// Resets the target's state to the initial state as defined by the animation settings (ready to start playing)
        /// </summary>
        ResetToInitialState
    }
}
