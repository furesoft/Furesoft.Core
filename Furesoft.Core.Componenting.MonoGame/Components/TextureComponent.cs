using Microsoft.Xna.Framework.Graphics;

namespace Furesoft.Core.Componenting.MonoGame.Components;

public class TextureComponent : Component
{
    private readonly string _name;

    public TextureComponent(string name)
    {
        _name = name;
        Texture = GameComponent.Content.Load<Texture2D>(_name);
    }

    public override void Initialize()
    {
        Texture = GameComponent.Content.Load<Texture2D>(_name);
    }

    public Texture2D Texture { get; set; }
}