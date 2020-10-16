using System;

namespace Furesoft.Core.Signals
{
    public interface ISignalBackend : IDisposable
    {
        event Action<byte[]> OnNewMessage;

        void Initialize(string Name, long capacity, bool isOwner);

        void Write(byte[] data);
    }
}