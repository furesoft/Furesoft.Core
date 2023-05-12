using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Furesoft.Core.Componenting.MonoGame;

public class GameComponent : Component
{
    public static ContentManager Content;
    public virtual void Render(SpriteBatch sb, GameTime gameTime) { }

    public virtual void Update(GameTime gameTime)
    {

    }
}