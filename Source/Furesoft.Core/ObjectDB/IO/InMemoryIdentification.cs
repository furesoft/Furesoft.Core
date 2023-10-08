namespace Furesoft.Core.ObjectDB.IO;

internal sealed class InMemoryIdentification : IDbIdentification
{
    private const string InMemoryName = "InMemory.";

    public InMemoryIdentification()
    {
    }

    public InMemoryIdentification(string id)
    {
        Id = InMemoryName + id;
    }

    public string Id { get; } = InMemoryName + "ID";

    public string Directory => string.Empty;

    public string FileName => string.Empty;

    public bool IsNew()
    {
        return true;
    }

    public void EnsureDirectories()
    {
        // in memory
    }

    public IMultiBufferedFileIO GetIO(int bufferSize)
    {
        return new MultiBufferedFileIO(bufferSize);
    }

    public IDbIdentification GetTransactionIdentification(long creationDateTime, string sessionId)
    {
        var filename =
            string.Format("{0}-{1}-{2}.transaction", Id, creationDateTime, sessionId);

        return new InMemoryIdentification(filename);
    }
}