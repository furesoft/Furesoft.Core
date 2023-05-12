using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Furesoft.Core.Componenting.MonoGame.Components;

public class TextureRenderer : GameComponent
{
    private readonly Color _color;

    public TextureRenderer()
    {
        _color = Color.White;
    }

    public TextureRenderer(Color color)
    {
        _color = color;
    }

    public override void Render(SpriteBatch sb, GameTime gameTime)
    {
        sb.Begin();

        var position = Object.GetComponent<TransformComponent>();
        var texture = Object.GetComponent<TextureComponent>().Texture;

        sb.Draw(texture, position.Bounds, _color);

        sb.End();
    }
}