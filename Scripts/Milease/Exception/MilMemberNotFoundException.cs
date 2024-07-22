namespace Milease.Milease.Exception
{
    public class MilMemberNotFoundException : System.Exception
    {
        public MilMemberNotFoundException(string memberName) 
            : base($"Target object doesn't have a field/property of '{memberName}', " +
                   $"please check the spelling, member type, upper/lower cases, and accessibility.")
        {
            
        }
    }
}