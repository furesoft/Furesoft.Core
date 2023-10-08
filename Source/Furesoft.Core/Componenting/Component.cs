namespace Furesoft.Core.Componenting;

public abstract class Component
{
    public ComponentObject Object { get; set; }

    public virtual bool Enabled { get; } = true;

    public virtual void Initialize()
    {
    }

    public virtual void Start()
    {
    }
}