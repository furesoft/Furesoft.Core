namespace Furesoft.Core.Componenting;

public sealed class ComponentObject
{
    public string Name { get; }

    public ComponentObject Parent { get; private set; }
    public List<ComponentObject> Children { get; set; }

    public Scene Scene { get; set; }

    private List<Component> _comps;

    public IReadOnlyList<Component> Components => _comps;

    private bool _isInitialized;

    public ComponentObject(string name)
    {
        Children = new();
        _comps = new();

        Name = name;
    }

    public void SetParent(ComponentObject parent)
    {
        Parent?.Children.Remove(this);
        
        Parent = parent;
        
        Parent?.Children.Add(this);
    }

    public ComponentObject GetRootParent()
    {
        ComponentObject current = this;
        ComponentObject parent;
        do
        {
            parent = current.Parent;
            if (parent == null)
                return current;
            current = parent;
        }
        while (parent != null);
        
        return null;
    }

    public void Destroy()
    {
        RemoveAllComponents();

        foreach (var c in Children)
            c.Destroy();
    }

    public void Initialize()
    {
        if (_isInitialized)
            return;

        foreach (var comp in _comps)
            comp.Initialize();
        
        foreach (var comp in _comps)
            comp.Start();

        _isInitialized = true;
    }

    public void AddComponent<T>()
        where T : Component, new()
    {
        var obj = new T
        {
            Object = this
        };

        AddComponent(obj);
    }

    public void AddComponent(Component comp)
    {
        comp.Object = this;
        _comps.Add(comp);

        if (!_isInitialized) return;
        
        comp.Initialize();
        comp.Start();
    }

    public T GetComponent<T>()
    {
        foreach (var comp in _comps)
        {
            if (comp is T matched)
                return matched;
        }
        
        return default;
    }

    public T GetComponentInChildren<T>(bool includeNotEnabled)
        where T : Component
    {
        foreach (var component in Children)
        {
            var childComponent = component.GetComponent<T>();
            if (childComponent is null || childComponent.Enabled != !includeNotEnabled)
            {
                continue;
            }

            return childComponent;
        }

        return null;
    }

    public bool HasComponent<T>() where T : Component
    {
        return GetComponent<T>() != null;
    }

    public bool RemoveComponent(Component comp)
    {
        return _comps.Remove(comp);
    }

    public void RemoveComponents<T>() where T : Component
    {
        foreach (var comp in Enumerable.Reverse(_comps))
        {
            if (comp is T matched)
                RemoveComponent(matched);
        }
    }

    public void RemoveAllComponents()
    {
        foreach (var comp in Enumerable.Reverse(_comps))
            RemoveComponent(comp);
    }
}