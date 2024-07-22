namespace Milease.Enums
{
    /// <summary>
    /// Determines how the ease function behaves.
    /// Learn more: https://easings.net/
    /// </summary>
    public enum EaseType
    {
        /// <summary>
        /// This easing type produces an effect where the animation starts slow, then speeds up.
        /// </summary>
        In, 
        /// <summary>
        /// This easing type produces an effect where the animation starts fast, then slows down.
        /// </summary>
        Out, 
        /// <summary>
        /// This easing type produces an effect where the animation starts slow, then speeds up, and finally slows down.
        /// </summary>
        IO
    }
    /// <summary>
    /// The function controlling the animation progress.
    /// Learn more: https://easings.net/
    /// </summary>
    public enum EaseFunction
    {
        Linear, Sine, Quad, Cubic, Quart, Quint, Expo, Circ, Back, Elastic, Bounce, Bezier
    }
}