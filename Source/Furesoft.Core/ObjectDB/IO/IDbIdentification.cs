namespace Furesoft.Core.ObjectDB.IO;

/// <summary>
///     An interface to get info about database parameters
/// </summary>
public interface IDbIdentification
{
    string Id { get; }
    string Directory { get; }
    string FileName { get; }

    bool IsNew();

    void EnsureDirectories();

    IMultiBufferedFileIO GetIO(int bufferSize);

    IDbIdentification GetTransactionIdentification(long creationDateTime, string sessionId);
}