using Milease.Core.Animation;

namespace Milease.Translate
{
    public interface ITransformation
    {
        bool CanTranslate<E>();

        MileaseHandleFunction<T, E> MakeTransformation<T, E>(BlendingMode blendingMode);
    }
}
