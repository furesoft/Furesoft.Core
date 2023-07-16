namespace Furesoft.Core.Platforming;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PlattformImplementationAttribute : Attribute
{
    public PlattformImplementationAttribute(OSName platform)
    {
        Platform = platform;
    }

    public OSName Platform { get; set; }
}