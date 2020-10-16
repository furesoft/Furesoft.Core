using Furesoft.Core.Signals.Core;
using System;

namespace Furesoft.Core.Signals
{
    public class MmfCommunicatorBackend : ISignalBackend
    {
        public event Action<byte[]> OnNewMessage;

        public void Dispose()
        {
            communicator.Dispose();
        }

        public void Initialize(string name, long capacity, bool isOwner)
        {
            communicator = new MemoryMappedFileCommunicator(name, capacity);
            if (isOwner)
            {
                communicator.ReadPosition = 2000;
                communicator.WritePosition = 0;
            }
            else
            {
                communicator.WritePosition = 2000;
                communicator.ReadPosition = 0;
            }
            communicator.StartReader();

            communicator.DataReceived += Communicator_DataReceived;
        }

        public void Write(byte[] data)
        {
            communicator.Write(data);
        }

        internal MemoryMappedFileCommunicator communicator;

        private void Communicator_DataReceived(object sender, DataReceivedEventArgs e)
        {
            OnNewMessage?.Invoke(e.Data);
        }
    }
}