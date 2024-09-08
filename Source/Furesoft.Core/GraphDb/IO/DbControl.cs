using System.Collections.Concurrent;
using System.Diagnostics;
using Furesoft.Core.GraphDb.IO.Blocks;

namespace Furesoft.Core.GraphDb.IO;

public static class DbControl
{
    internal const string NodePath = "node.storage.db";
    internal const string RelationPath = "relation.storage.db";
    internal const string LabelPath = "label.storage.db";
    internal const string NodePropertyPath = "node_property.storage.db";
    internal const string RelationPropertyPath = "relation_property.storage.db";
    internal const string PropertyNamePath = "property_name.storage.db";

    internal const string StringPath = "string.storage.db";

    private const string IdStoragePath = "id.storage";
    private static readonly TraceSource TraceSource = new("TraceGraphyDb");
    public static string DbPath;
    private static bool _initializedIoFlag;

    internal static readonly Dictionary<string, int> BlockByteSize = new()
    {
        { StringPath, 34 },
        { PropertyNamePath, 34 },
        { NodePropertyPath, 17 },
        { RelationPropertyPath, 17 },
        { NodePath, 17 },
        { RelationPath, 33 },
        { LabelPath, 34 },
        { IdStoragePath, 4 }
    };

    private static readonly Dictionary<string, int> IdStoreOrderNumber = new()
    {
        { StringPath, 1 },
        { PropertyNamePath, 2 },
        { NodePropertyPath, 3 },
        { RelationPropertyPath, 4 },
        { NodePath, 5 },
        { RelationPath, 6 },
        { LabelPath, 0 }
    };

    private static FileStream _idFileStream;

    private static readonly ConcurrentDictionary<string, int> IdStorageDictionary = new();

    // Paths to storage files
    private static readonly List<string> DbFilePaths =
    [
        NodePath,
        RelationPath,
        LabelPath,
        RelationPropertyPath,
        NodePropertyPath,
        PropertyNamePath,
        StringPath
    ];

    public static readonly ConcurrentDictionary<string, int> LabelInvertedIndex = new();

    internal static readonly ConcurrentDictionary<string, int> PropertyNameInvertedIndex = new();

    internal static Thread ConsisterThread;

    internal static readonly Dictionary<string, FileStream> FileStreamDictionary = new();

    /// <summary>
    ///     Create storage files if missing
    /// </summary>
    public static void InitializeIO()
    {
        if (_initializedIoFlag) return;
        try
        {
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

            foreach (var filePath in DbFilePaths)
                FileStreamDictionary[filePath] = new FileStream(Path.Combine(DbPath, filePath),
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 5 * 1024 * 1024);

            // Create new empty IdStorage if not present with next free id.
            // Else initialize .storage.db -> ID mapping
            if (!File.Exists(Path.Combine(DbPath, IdStoragePath)))
            {
                _idFileStream = new FileStream(Path.Combine(DbPath, IdStoragePath),
                    FileMode.Create,
                    FileAccess.ReadWrite, FileShare.Read);
                foreach (var filePath in DbFilePaths)
                {
                    _idFileStream.Write(BitConverter.GetBytes(1), 0, 4);
                    IdStorageDictionary[filePath] = 1;
                }

                _idFileStream.Flush();
            }
            else
            {
                _idFileStream = new FileStream(Path.Combine(DbPath, IdStoragePath),
                    FileMode.Open,
                    FileAccess.ReadWrite, FileShare.Read);

                foreach (var filePath in DbFilePaths)
                {
                    var blockNumber = IdStoreOrderNumber[filePath];
                    var storedIdBytes = new byte[4];
                    _idFileStream.Seek(blockNumber * 4, SeekOrigin.Begin);
                    _idFileStream.Read(storedIdBytes, 0, 4);
                    IdStorageDictionary[filePath] = BitConverter.ToInt32(storedIdBytes, 0);
                }
            }

            // Initialize Inverted Indexes
            for (var i = 1; i < FetchLastId(LabelPath); ++i)
            {
                var labelBlock = new LabelBlock(DbReader.ReadGenericStringBlock(LabelPath, i));
                LabelInvertedIndex[labelBlock.Data] = labelBlock.Id;
            }

            for (var i = 1; i < FetchLastId(PropertyNamePath); ++i)
            {
                var propertyNameBlock =
                    new PropertyNameBlock(DbReader.ReadGenericStringBlock(PropertyNamePath, i));
                PropertyNameInvertedIndex[propertyNameBlock.Data] = propertyNameBlock.Id;
            }
        }
        catch (Exception ex)
        {
            TraceSource.TraceEvent(TraceEventType.Error, 1,
                $"Database Initialization Falied: {ex}");
        }
        finally
        {
            _initializedIoFlag = true;
        }
    }

    public static void ShutdownIo()
    {
        IdStorageDictionary.Clear();
        PropertyNameInvertedIndex.Clear();
        LabelInvertedIndex.Clear();

        foreach (var filePath in DbFilePaths)
        {
            FileStreamDictionary?[filePath]?.Dispose();
            FileStreamDictionary[filePath] = null;
        }

        _idFileStream?.Dispose();
        _idFileStream = null;
        _initializedIoFlag = false;
    }

    public static void DeleteDbFiles()
    {
        ShutdownIo();
        Directory.Delete(DbPath, true);
    }

    public static int AllocateId(string filePath)
    {
        var lastId = IdStorageDictionary[filePath];
        IdStorageDictionary[filePath] += 1;
        _idFileStream.Seek(IdStoreOrderNumber[filePath] * 4, SeekOrigin.Begin);
        _idFileStream.Write(BitConverter.GetBytes(IdStorageDictionary[filePath]), 0, 4);

        return lastId;
    }

    public static int FetchLastId(string filePath)
    {
        return IdStorageDictionary[filePath];
    }

    public static int FetchLabelId(string label)
    {
        LabelInvertedIndex.TryGetValue(label, out var labelId);
        if (labelId != 0) return labelId;
        var newLabelId = AllocateId(LabelPath);
        LabelInvertedIndex[label] = newLabelId;
        DbWriter.WriteStringBlock(new LabelBlock(true, label, newLabelId));

        return newLabelId;
    }

    public static int FetchPropertyNameId(string propertyName)
    {
        PropertyNameInvertedIndex.TryGetValue(propertyName, out var propertyId);
        if (propertyId != 0) return propertyId;
        var newPropertyNameId = AllocateId(PropertyNamePath);
        DbWriter.WriteStringBlock(new PropertyNameBlock(true, propertyName, newPropertyNameId));
        PropertyNameInvertedIndex[propertyName] = newPropertyNameId;

        return newPropertyNameId;
    }
}