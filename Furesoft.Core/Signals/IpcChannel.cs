using System;
using System.Collections.Generic;
using System.Reflection;

namespace Furesoft.Core.Signals
{
    public sealed class IpcChannel : IDisposable
    {
        public static IpcChannel operator +(IpcChannel channel, Action<object> callback)
        {
            Signal.Subscribe(channel, callback);
            return channel;
        }

        public void Dispose()
        {
            event_communicator.Dispose();
            func_communicator.Dispose();
        }

        internal ISignalBackend event_communicator;
        internal ISignalBackend func_communicator;
        internal Dictionary<int, MethodInfo> shared_functions = new Dictionary<int, MethodInfo>();
    }
}