namespace Furesoft.Core;

public class Freezable<T>
{
    public bool IsFrozen { get; private set; }
    public T Value { get; set; }

    public void Freeze()
    {
        IsFrozen = true;
    }

    public static implicit operator T(Freezable<T> obj)
    {
        return obj.Value;
    }

    public void Set(T obj)
    {
        if (IsFrozen)
        {
            throw new InvalidOperationException("Frozen object cannot be set");
        }

        Value = obj;
    }
}