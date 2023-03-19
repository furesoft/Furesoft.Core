namespace Furesoft.Core.Componenting;

public class Scene
{
    public readonly List<ComponentObject> Objects = new();

    public void Initialize()
    {
        foreach (var entity in Objects)
            entity.Initialize();
    }
}