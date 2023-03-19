using Furesoft.Core.Componenting;
using Furesoft.Core.Componenting.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Furesoft.Core.Componenting.MonoGame.Components;

public class Selectable : GameComponent
{
    public bool IsSelected { get; private set; }

    public event Action<ComponentObject> OnSelect;

    public override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();

        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            if (!Object.IsMouseOverGameObject())
            {
                return;
            }

            var texture = Object.GetComponent<TextureComponent>().Texture;

            Color[] pixels = new Color[texture.Height * texture.Width];
            texture.GetData(pixels);

            var mouseVector = new Vector2(mouseState.Position.X, mouseState.Position.Y);
            var position = Object.GetComponent<Transform>();
            var relativePosition = mouseVector - position.Position;
            var index = (int)(relativePosition.Y / position.Size.Y * texture.Height * texture.Width +
                               relativePosition.X / position.Size.X * texture.Width);

            var color = pixels[index % pixels.Length];

            IsSelected = color.A >= 200;

            if (!IsSelected) return;

            OnSelect?.Invoke(Object);
        }
    }
}