namespace Furesoft.Core.Componenting;

public abstract class Component
{
    public ComponentObject GameObject { get; set; }

    public virtual void Initialize() { }

    public virtual void Start() { }


    public virtual bool Enabled { get; } = true;

    public virtual bool Visible { get; } = true;

}