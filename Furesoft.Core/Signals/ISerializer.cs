using System;

namespace Furesoft.Core.Signals
{
    public interface ISerializer
    {
        object Deserialize(byte[] raw, Type type);

        byte[] Serialize(object obj);
    }

    public static class ISerializerExtensions
    {
        public static T Deserialize<T>(this ISerializer s, byte[] raw)
        {
            return (T)s.Deserialize(raw, typeof(T));
        }
    }
}