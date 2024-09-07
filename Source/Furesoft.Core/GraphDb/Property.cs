using Furesoft.Core.GraphDb.IO;
using Furesoft.Core.GraphDb.IO.Blocks;

namespace Furesoft.Core.GraphDb;

public abstract class Property : Entity
{
    private static readonly List<Type> SupportedTypes = [typeof(int), typeof(string), typeof(bool), typeof(float)];
    private object _value;

    public string Key;

    internal Entity Parent;

    public int PropertyId;

    protected Property(Entity parent, string key, object value)
    {
        if (!SupportedTypes.Contains(value.GetType()))
            throw new NotSupportedException("Cannot store properties with type " + value.GetType());

        PropertyId = 0;

        Parent = parent ?? throw new ArgumentNullException(nameof(parent));

        if (string.IsNullOrEmpty(key)) throw new ArgumentException(nameof(key) + " cannot be null or empty string");

        Key = key;

        _value = value;

        Db = parent.Db;

        State |= EntityState.Added;
        Db.ChangedEntities.Add(this);
    }

    protected Property(Entity parent, PropertyBlock propertyBlock)
    {
        PropertyId = propertyBlock.PropertyId;
        Parent = parent;
        Db = parent.Db;

        Key = DbReader.ReadGenericStringBlock(DbControl.PropertyNamePath, propertyBlock.PropertyNameId).Data;

        switch (propertyBlock.PropertyType)
        {
            case PropertyType.Int:
                _value = BitConverter.ToInt32(propertyBlock.Value, 0);
                break;
            case PropertyType.String:
                _value = DbReader.ReadGenericStringBlock(DbControl.StringPath,
                    BitConverter.ToInt32(propertyBlock.Value, 0)).Data;
                break;
            case PropertyType.Bool:
                _value = BitConverter.ToBoolean(propertyBlock.Value, 3);
                break;
            case PropertyType.Float:
                _value = BitConverter.ToSingle(propertyBlock.Value, 0);
                break;
            default:
                throw new NotSupportedException("Unrecognized Property Type");
        }
    }

    public PropertyType PropertyType =>
        _value switch
        {
            int _ => PropertyType.Int,
            bool _ => PropertyType.Bool,
            float _ => PropertyType.Float,
            string _ => PropertyType.String,
            _ => throw new NotSupportedException()
        };

    public object Value
    {
        get => _value;
        set
        {
            if (!SupportedTypes.Contains(value.GetType()))
                throw new NotSupportedException("Cannot store properties with type " + value.GetType());

            _value = value;

            State |= EntityState.Modified;
            Db.ChangedEntities.Add(this);
        }
    }
}