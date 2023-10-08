namespace Furesoft.Core.Componenting;

public class Scene
{
    private readonly List<ComponentObject> _objects = new();
    public IReadOnlyList<ComponentObject> Objects => _objects;

    public void Initialize()
    {
        for (var index = 0; index < _objects.Count; index++)
        {
            var entity = _objects[index];
            entity.Initialize();

            for (var i = 0; i < entity.Children.Count; i++)
            {
                var child = entity.Children[i];

                child.Initialize();
            }
        }
    }

    public void Add(ComponentObject obj)
    {
        obj.Scene = this;

        _objects.Add(obj);
    }

    public ComponentObject CreateComponent(string name)
    {
        var co = new ComponentObject(name);

        Add(co);

        return co;
    }
}