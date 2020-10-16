using System;

namespace Furesoft.Core.Activation
{
    public interface IActivationStrategy
    {
        object Activate(Type type, object[] args);
    }
}