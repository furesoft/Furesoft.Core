namespace Furesoft.Core.Activation.Strategies;

public class SlowActivation : IActivationStrategy
{
    public object Activate(Type type, object[] args)
    {
        return Activator.CreateInstance(type, args);
    }
}