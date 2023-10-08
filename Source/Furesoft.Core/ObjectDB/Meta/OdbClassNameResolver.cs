using Furesoft.Core.ObjectDB.Tool;

namespace Furesoft.Core.ObjectDB.Meta;

internal static class OdbClassNameResolver
{
    private static readonly Dictionary<string, string> cacheByFullClassName =
        new();

    public static string GetClassName(string fullClassName)
    {
        return cacheByFullClassName.GetOrAdd(fullClassName, ProduceClassName);
    }

    private static string ProduceClassName(string fullClassName)
    {
        var index = fullClassName.LastIndexOf('.');

        var className = index == -1
            ? fullClassName // primitive type
            : GetClassName(fullClassName, index);
        return className;
    }

    private static string GetClassName(string fullClassName, int index)
    {
        var startIndex = index + 1;
        return fullClassName.Substring(startIndex, fullClassName.Length - startIndex);
    }

    public static string GetNamespace(string fullClassName)
    {
        var index = fullClassName.LastIndexOf('.');
        return index == -1
            ? string.Empty
            : fullClassName.Substring(0, index);
    }

    public static string GetFullName(Type type)
    {
        return type.AssemblyQualifiedName;
    }
}