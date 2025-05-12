namespace Milease.Translate
{
    public static class TransformationManagerHolder
    {
        public static ITransformationManager TransformationManager = new SimpleTransformationManager();

        static TransformationManagerHolder()
        {
#if COLOR_TOOL_SETUP && (NET_STANDARD_2_1 || POLYFILL_SETUP)
            TransformationManager.Register(new ColorTransformation());
#endif
        }
    }
}
