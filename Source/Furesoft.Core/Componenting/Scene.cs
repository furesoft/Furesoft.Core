namespace Furesoft.Core.Componenting;

public class Scene
{
    private readonly List<ComponentObject> _objects = new();
    public IReadOnlyList<ComponentObject> Objects => _objects;

    public void Initialize()
    {
        foreach (var entity in _objects)
            entity.Initialize();
    }

    public void Add(ComponentObject obj)
    {
        obj.Scene = this;
        
        _objects.Add(obj);
    }
}