using Microsoft.Xna.Framework;

namespace Furesoft.Core.Componenting.MonoGame.Components;

public class Collider : GameComponent
{
    public override void Update(GameTime gameTime)
    {
        foreach (var obj in Object.Scene.Objects)
        {
            var collider = obj.GetComponent<Collider>();

            if (collider is not {Enabled: true})
            {
                continue;
            }

            var collisionRec = Object.GetComponent<TransformComponent>().Bounds;
            var otherCollisionRec = obj.GetComponent<TransformComponent>().Bounds;
            
            if (!collisionRec.Intersects(otherCollisionRec))
            {
                continue;
            }
            
            obj.GetComponent<ICollision>()?.OnCollide(Object);
            
            Object.GetComponent<ICollision>()?.OnCollide(obj);
        }
    }
}