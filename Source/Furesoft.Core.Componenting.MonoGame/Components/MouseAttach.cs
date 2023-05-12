using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Furesoft.Core.Componenting.MonoGame.Components;

public class MouseAttach : GameComponent
{
    public override void Update(GameTime gameTime)
    {
        var state = Mouse.GetState();

        Object.GetComponent<TransformComponent>().Position = state.Position.ToVector2();
    }
}