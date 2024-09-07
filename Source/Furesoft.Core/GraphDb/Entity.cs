namespace Furesoft.Core.GraphDb;

public abstract class Entity
{
    public DbEngine Db;
    public EntityState State = EntityState.Unchanged;

    public void Delete()
    {
        Db.Delete(this);
    }
}