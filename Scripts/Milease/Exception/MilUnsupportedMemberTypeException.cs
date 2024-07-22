namespace Milease.Milease.Exception
{
    public class MilUnsupportedMemberTypeException : System.Exception
    {
        public MilUnsupportedMemberTypeException(string memberName) 
            : base($"Target member '{memberName}' isn't a field or property.")
        {
            
        }
    }
}