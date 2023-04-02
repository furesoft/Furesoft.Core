using Microsoft.Xna.Framework;

namespace Furesoft.Core.Componenting.MonoGame.Components;

public class TransformComponent : Component
{
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

    public TransformComponent(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }
}
