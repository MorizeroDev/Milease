namespace Milease.Milease.Exception
{
    public class MilTargetNotFoundException : System.Exception
    {
        public MilTargetNotFoundException() 
            : base("Target object is null, is it destroyed or you forget to config it in the inspector?")
        {
            
        }
    }
}