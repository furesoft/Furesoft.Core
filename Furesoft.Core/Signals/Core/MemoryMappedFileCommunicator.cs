using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Furesoft.Core.Signals.Core
{
    internal class MemoryMappedFileCommunicator : IDisposable
    {
        #region Constants

        public MemoryMappedFile MappedFile { get; set; }
        public int ReadPosition { get; set; }

        public int WritePosition
        {
            get { return writePosition; }
            set
            {
                if (value != writePosition)
                {
                    writePosition = value;
                    view.Write(WritePosition + READ_CONFIRM_OFFSET, true);
                }
            }
        }

        public MemoryMappedFileCommunicator(string mapName, long capacity)
            : this(MemoryMappedFile.CreateOrOpen(mapName, capacity), 0, 0, MemoryMappedFileAccess.ReadWrite)
        { }

        public MemoryMappedFileCommunicator(string mapName, long capacity, long offset, long size)
            : this(MemoryMappedFile.CreateOrOpen(mapName, capacity), offset, size, MemoryMappedFileAccess.ReadWrite)
        { }

        public MemoryMappedFileCommunicator(string mapName, long capacity, long offset, long size, MemoryMappedFileAccess access)
            : this(MemoryMappedFile.CreateOrOpen(mapName, capacity), offset, size, access)
        { }

        public MemoryMappedFileCommunicator(MemoryMappedFile mappedFile)
            : this(mappedFile, 0, 0, MemoryMappedFileAccess.ReadWrite)
        { }

        public MemoryMappedFileCommunicator(MemoryMappedFile mappedFile, long offset, long size)
            : this(mappedFile, offset, size, MemoryMappedFileAccess.ReadWrite)
        { }

        public MemoryMappedFileCommunicator(MemoryMappedFile mappedFile, long offset, long size, MemoryMappedFileAccess access)
        {
            MappedFile = mappedFile;
            view = mappedFile.CreateViewAccessor(offset, size, access);

            ReadPosition = -1;
            writePosition = -1;
            dataToSend = new List<byte[]>();

            callback = new SendOrPostCallback(OnDataReceivedInternal);
            operation = AsyncOperationManager.CreateOperation(null);
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public void CloseReader()
        {
            started = false;
        }

        public void Dispose()
        {
            started = false;
            if (view != null)
            {
                try
                {
                    view.Dispose();
                    view = null;
                }
                catch { }
            }

            if (MappedFile != null)
            {
                try
                {
                    MappedFile.Dispose();
                    MappedFile = null;
                }
                catch { }
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }

        public void StartReader()
        {
            if (started)
                return;

            if (ReadPosition < 0 || writePosition < 0)
                throw new ArgumentException();

            Thread t = new Thread(ReaderThread);
            t.IsBackground = true;
            t.Start();
            started = true;
        }

        public void Write(string message)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(message);
            this.Write(data);
        }

        public void Write(byte[] data)
        {
            if (ReadPosition < 0 || writePosition < 0)
                throw new ArgumentException();

            lock (dataToSend)
                dataToSend.Add(data);

            if (!writerThreadRunning)
            {
                writerThreadRunning = true;
                writerThread = new Thread(WriterThread);
                writerThread.IsBackground = true;
                writerThread.Start();
            }
        }

        public void WriterThread(object stateInfo)
        {
            while (dataToSend.Count > 0 && !disposed)
            {
                byte[] data = null;
                lock (dataToSend)
                {
                    data = dataToSend[0];
                    dataToSend.RemoveAt(0);
                }

                while (!view.ReadBoolean(WritePosition + READ_CONFIRM_OFFSET))
                    Thread.Sleep(500);

                // Sets length and write data.
                view.Write(writePosition + DATA_LENGTH_OFFSET, data.Length);
                view.WriteArray<byte>(writePosition + DATA_OFFSET, data, 0, data.Length);

                // Resets the flag used to signal that data has been read.
                view.Write(writePosition + READ_CONFIRM_OFFSET, false);
                // Sets the flag used to signal that there are data avaibla.
                view.Write(writePosition + DATA_AVAILABLE_OFFSET, true);
            }

            writerThreadRunning = false;
        }

        internal virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            if (e != null && DataReceived != null)
                DataReceived(this, e);
        }

        private const int DATA_AVAILABLE_OFFSET = 0;
        private const int DATA_LENGTH_OFFSET = READ_CONFIRM_OFFSET + 1;
        private const int DATA_OFFSET = DATA_LENGTH_OFFSET + 10;
        private const int READ_CONFIRM_OFFSET = DATA_AVAILABLE_OFFSET + 1;

        #endregion Constants

        #region Properties

        private SendOrPostCallback callback;
        private List<byte[]> dataToSend;
        private bool disposed;
        private AsyncOperation operation;
        private bool started;
        private MemoryMappedViewAccessor view;
        private int writePosition;

        #endregion Properties

        private Thread writerThread;
        private bool writerThreadRunning;

        private void OnDataReceivedInternal(object state)
        {
            OnDataReceived(state as DataReceivedEventArgs);
        }

        private void ReaderThread(object stateInfo)
        {
            while (started)
            {
                // Checks if there is something to read.
                var dataAvailable = view.ReadBoolean(ReadPosition + DATA_AVAILABLE_OFFSET);
                if (dataAvailable)
                {
                    // Checks how many bytes to read.
                    int availableBytes = view.ReadInt32(ReadPosition + DATA_LENGTH_OFFSET);
                    var bytes = new byte[availableBytes];
                    // Reads the byte array.
                    int read = view.ReadArray<byte>(ReadPosition + DATA_OFFSET, bytes, 0, availableBytes);

                    // Sets the flag used to signal that there aren't available data anymore.
                    view.Write(ReadPosition + DATA_AVAILABLE_OFFSET, false);
                    // Sets the flag used to signal that data has been read.
                    view.Write(ReadPosition + READ_CONFIRM_OFFSET, true);

                    DataReceivedEventArgs args = new DataReceivedEventArgs(bytes, read);
                    operation.Post(callback, args);
                }

                Thread.Sleep(500);
            }
        }
    }
}