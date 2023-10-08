using Furesoft.Core.ObjectDB.Tool;

namespace Furesoft.Core.ObjectDB.Api;

/// <summary>
///     The main NDatabase ODB Configuration class.
/// </summary>
/// <remarks>
///     All engine configuration is done via this class.
/// </remarks>
public static class OdbConfiguration
{
    private static bool loggingEnabled;
    private static bool btreeValidationEnabled;

    /// <summary>
    ///     Default index BTree degree - 20
    /// </summary>
    public static readonly int DefaultIndexBTreeDegree = 20;

    /// <summary>
    ///     The default btree size for index btrees
    /// </summary>
    private static int indexBTreeDegree = DefaultIndexBTreeDegree;

    /// <summary>
    ///     Enables the B tree validation.
    /// </summary>
    /// <remarks>
    ///     It is more safe to run with that (finding issues), but that hits performance.
    /// </remarks>
    public static void EnableBTreeValidation()
    {
        btreeValidationEnabled = true;
    }

    /// <summary>
    ///     Determines whether [is B tree validation enabled].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [is B tree validation enabled]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsBTreeValidationEnabled()
    {
        return btreeValidationEnabled;
    }

    /// <summary>
    ///     Disables the B tree validation.
    /// </summary>
    public static void DisableBTreeValidation()
    {
        btreeValidationEnabled = false;
    }

    /// <summary>
    ///     Get index BTree degree (on start it is equals to 20)
    /// </summary>
    /// <remarks>
    ///     It is less safe to run without that (finding issues), but that improves performance.
    /// </remarks>
    /// <returns>Degree of index BTree</returns>
    public static int GetIndexBTreeDegree()
    {
        return indexBTreeDegree;
    }

    /// <summary>
    ///     Sets the index B tree degree.
    /// </summary>
    /// <remarks>
    ///     Default value is equal to 20.
    /// </remarks>
    /// <param name="indexBTreeSize">Size of the index B tree.</param>
    public static void SetIndexBTreeDegree(int indexBTreeSize)
    {
        indexBTreeDegree = indexBTreeSize;
    }

    /// <summary>
    ///     Determines whether [is logging enabled].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [is logging enabled]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLoggingEnabled()
    {
        return loggingEnabled;
    }

    /// <summary>
    ///     Enables the logging.
    /// </summary>
    public static void EnableLogging()
    {
        loggingEnabled = true;
    }

    /// <summary>
    ///     Disables the logging.
    /// </summary>
    public static void DisableLogging()
    {
        loggingEnabled = false;
    }

    /// <summary>
    ///     Registers the logger.
    /// </summary>
    /// <remarks>
    ///     Automatically enables the logging.
    /// </remarks>
    /// <param name="logger">The logger.</param>
    public static void RegisterLogger(ILogger logger)
    {
        if (!IsLoggingEnabled())
            EnableLogging();

        DLogger.Register(logger);
    }
}