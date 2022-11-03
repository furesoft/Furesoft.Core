using System.Runtime.CompilerServices;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;

namespace Furesoft.Core.CodeDom.Compiler.Core.Constants;

/// <summary>
/// A default-value constant, which represents the default
/// value of some type, typically characterized by the all-zeroes
/// bit pattern.
/// </summary>
public sealed class DefaultConstant : Constant
{
    private DefaultConstant()
    {

    }

    /// <summary>
    /// An instance of the default-value constant.
    /// </summary>
    /// <returns>The default-value constant.</returns>
    public static readonly DefaultConstant Instance = new();

    /// <inheritdoc/>
    public override bool Equals(Constant other)
    {
        return other is DefaultConstant;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "default";
    }
}
