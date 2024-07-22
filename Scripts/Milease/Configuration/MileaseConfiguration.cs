namespace Milease.Configuration
{
    public class MileaseConfiguration
    {
        public static MileaseConfiguration Configuration { get; set; } = new MileaseConfiguration();

        public ColorTransformationType DefaultColorTransformationType { get; set; } = ColorTransformationType.OKLCH;
    }
}