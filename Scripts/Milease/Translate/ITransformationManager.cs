using JetBrains.Annotations;
using Milease.Core.Animation;

namespace Milease.Translate
{
    public interface ITransformationManager
    {
        public void Register(ITransformation transformation);

        [CanBeNull]
        public MileaseHandleFunction<T, E> GetTransformation<T, E>(
            BlendingMode blendingMode /* 直觉告诉我这里迟早会换成 full context */
        );
    }
}
