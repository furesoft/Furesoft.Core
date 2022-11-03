namespace Furesoft.Core.CodeDom.Compiler.Core;

/// <summary>
/// Defines common functionality for member attributes.
/// </summary>
public interface IAttribute
{
    /// <summary>
    /// Gets the attribute's type.
    /// </summary>
    IType AttributeType { get; }
}
