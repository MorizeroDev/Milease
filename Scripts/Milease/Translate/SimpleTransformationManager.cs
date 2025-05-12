using System.Collections.Generic;
using Milease.Core.Animation;

namespace Milease.Translate
{
    public class SimpleTransformationManager : ITransformationManager
    {
        private readonly List<ITransformation> _transformationList = new List<ITransformation>();

        public void Register(ITransformation transformation)
        {
            _transformationList.Add(transformation);
        }


        public MileaseHandleFunction<T, E> GetTransformation<T, E>(BlendingMode blendingMode)
        {
            foreach (var item in _transformationList)
            {
                if (item.CanTranslate<E>())
                {
                    return item.MakeTransformation<T, E>(blendingMode);
                }
            }

            return null;
        }
    }
}