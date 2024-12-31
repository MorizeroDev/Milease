using System;
using Milease.Core.Animation;

namespace Milease.DSL
{
    public class AniExpression<T>
    {
        internal T From, To;
        internal float Duration = 0f;
        internal float StartTime = 0f;
        internal bool ToOnly = false;
        internal MilAnimation.BlendingMode BlendingMode = MilAnimation.BlendingMode.Default;
        
        public static AniExpression<T> operator /(float duration, AniExpression<T> expr)
        {
            expr.Duration = duration;
            return expr;
        }
        
        public static AniExpression<T> operator +(float delay, AniExpression<T> expr)
        {
            expr.StartTime += delay;
            return expr;
        }
        
        public static AniExpression<T> operator -(AniExpression<T> expr, MilAnimation.BlendingMode blendingMode)
        {
            expr.BlendingMode = blendingMode;
            return expr;
        }
    }
}
