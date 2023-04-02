using Microsoft.Xna.Framework;

namespace Furesoft.Core.Componenting.MonoGame.Components;

public class Collider : GameComponent
{
    public Rectangle CollisionRec { get; set; }

    public Collider(Rectangle collisionRec)
    {
        CollisionRec = collisionRec;
    }

    public Collider()
    {
        
    }

    public override void Initialize()
    {
        if (CollisionRec == default)
        {
            CollisionRec = Object.GetComponent<TransformComponent>().Bounds;
        }
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var obj in Object.Scene.Objects)
        {
            var collider = obj.GetComponent<Collider>();

            if (collider == null && !collider.Enabled)
            {
                continue;
            }

            if (!CollisionRec.Intersects(collider.CollisionRec))
            {
                continue;
            }
            
            obj.GetComponent<ICollision>()?.OnCollide(Object);
            
            Object.GetComponent<ICollision>().OnCollide(obj);
        }
    }
}