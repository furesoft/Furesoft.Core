namespace Furesoft.Core.ObjectDB.IO;

/// <summary>
///     Database Parameters for local database access
/// </summary>
internal sealed class FileIdentification : IDbIdentification
{
    internal FileIdentification(string name)
    {
        FileName = name;
    }

    public override string ToString()
    {
        return FileName;
    }

    private string GetCleanFileName()
    {
        return Path.GetFileName(FileName);
    }

    #region IFileIdentification Members

    public string Directory
    {
        get
        {
            var fullPath = Path.GetFullPath(FileName);
            return Path.GetDirectoryName(fullPath);
        }
    }

    public string Id => GetCleanFileName();

    public bool IsNew()
    {
        return !File.Exists(FileName);
    }

    public void EnsureDirectories()
    {
        OdbDirectory.Mkdirs(FileName);
    }

    public IMultiBufferedFileIO GetIO(int bufferSize)
    {
        return new MultiBufferedFileIO(FileName, bufferSize);
    }

    public IDbIdentification GetTransactionIdentification(long creationDateTime, string sessionId)
    {
        var filename =
            string.Format("{0}-{1}-{2}.transaction", Id, creationDateTime, sessionId);

        return new InMemoryIdentification(filename);
    }

    public string FileName { get; }

    #endregion IFileIdentification Members
}