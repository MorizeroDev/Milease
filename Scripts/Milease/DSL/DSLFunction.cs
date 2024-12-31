using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Milease.Core;
using Milease.Core.Animation;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Translate;

namespace Milease.DSL
{
    public static partial class DSL
    {

        public static MilInstantAnimator MLinear<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Linear);

        public static MilInstantAnimator MLinear<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Linear);


        public static MilInstantAnimator MSine<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Sine);

        public static MilInstantAnimator MSine<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Sine);


        public static MilInstantAnimator MSineOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Sine);

        public static MilInstantAnimator MSineOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Sine);


        public static MilInstantAnimator MSineIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Sine);

        public static MilInstantAnimator MSineIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Sine);


        public static MilInstantAnimator MQuad<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Quad);

        public static MilInstantAnimator MQuad<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Quad);


        public static MilInstantAnimator MQuadOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Quad);

        public static MilInstantAnimator MQuadOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Quad);


        public static MilInstantAnimator MQuadIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Quad);

        public static MilInstantAnimator MQuadIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Quad);


        public static MilInstantAnimator MCubic<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Cubic);

        public static MilInstantAnimator MCubic<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Cubic);


        public static MilInstantAnimator MCubicOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Cubic);

        public static MilInstantAnimator MCubicOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Cubic);


        public static MilInstantAnimator MCubicIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Cubic);

        public static MilInstantAnimator MCubicIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Cubic);


        public static MilInstantAnimator MQuart<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Quart);

        public static MilInstantAnimator MQuart<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Quart);


        public static MilInstantAnimator MQuartOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Quart);

        public static MilInstantAnimator MQuartOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Quart);


        public static MilInstantAnimator MQuartIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Quart);

        public static MilInstantAnimator MQuartIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Quart);


        public static MilInstantAnimator MQuint<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Quint);

        public static MilInstantAnimator MQuint<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Quint);


        public static MilInstantAnimator MQuintOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Quint);

        public static MilInstantAnimator MQuintOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Quint);


        public static MilInstantAnimator MQuintIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Quint);

        public static MilInstantAnimator MQuintIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Quint);


        public static MilInstantAnimator MExpo<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Expo);

        public static MilInstantAnimator MExpo<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Expo);


        public static MilInstantAnimator MExpoOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Expo);

        public static MilInstantAnimator MExpoOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Expo);


        public static MilInstantAnimator MExpoIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Expo);

        public static MilInstantAnimator MExpoIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Expo);


        public static MilInstantAnimator MCirc<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Circ);

        public static MilInstantAnimator MCirc<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Circ);


        public static MilInstantAnimator MCircOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Circ);

        public static MilInstantAnimator MCircOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Circ);


        public static MilInstantAnimator MCircIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Circ);

        public static MilInstantAnimator MCircIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Circ);


        public static MilInstantAnimator MBack<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Back);

        public static MilInstantAnimator MBack<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Back);


        public static MilInstantAnimator MBackOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Back);

        public static MilInstantAnimator MBackOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Back);


        public static MilInstantAnimator MBackIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Back);

        public static MilInstantAnimator MBackIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Back);


        public static MilInstantAnimator MElastic<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Elastic);

        public static MilInstantAnimator MElastic<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Elastic);


        public static MilInstantAnimator MElasticOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Elastic);

        public static MilInstantAnimator MElasticOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Elastic);


        public static MilInstantAnimator MElasticIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Elastic);

        public static MilInstantAnimator MElasticIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Elastic);


        public static MilInstantAnimator MBounce<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Bounce);

        public static MilInstantAnimator MBounce<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Bounce);


        public static MilInstantAnimator MBounceOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Bounce);

        public static MilInstantAnimator MBounceOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Bounce);


        public static MilInstantAnimator MBounceIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Bounce);

        public static MilInstantAnimator MBounceIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Bounce);


        public static MilInstantAnimator MBezier<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.In, EaseFunction.Bezier);

        public static MilInstantAnimator MBezier<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.In, EaseFunction.Bezier);


        public static MilInstantAnimator MBezierOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.Out, EaseFunction.Bezier);

        public static MilInstantAnimator MBezierOut<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.Out, EaseFunction.Bezier);


        public static MilInstantAnimator MBezierIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, E from, E to)
        => generate(target, mbExpr, from.To(to), EaseType.IO, EaseFunction.Bezier);

        public static MilInstantAnimator MBezierIO<T, E>(
            this T target, Expression<Func<T, E>> mbExpr, AniExpression<E> aniExpr)
        => generate(target, mbExpr, aniExpr, EaseType.IO, EaseFunction.Bezier);

    }
}
