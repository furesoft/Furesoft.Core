using Furesoft.Core.Componenting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Furesoft.Core.Componenting.MonoGame;

public static class SceneExtensions
{
    public static void Update(this Scene scene, GameTime gameTime)
    {
        foreach (var entity in scene._objects)
            entity.Update(gameTime);
    }

    public static void Draw(this Scene scene, SpriteBatch sb, GameTime gameTime)
    {
        foreach (var entity in scene._objects)
            entity.Draw(sb, gameTime);
    }
}