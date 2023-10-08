namespace Furesoft.Core.Storage;

public interface ISerializer<K>
{
    bool IsFixedSize { get; }

    int Length { get; }

    byte[] Serialize(K value);

    K Deserialize(byte[] buffer, int offset, int length);
}