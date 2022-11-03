using Furesoft.Core.CodeDom.Compiler.Core.Names;

namespace Furesoft.Core.CodeDom.Compiler.Core;

/// <summary>
/// The root interface for members: constructs that
/// have a name, a full name and a set of attributes.
/// </summary>
public interface IMember
{
    /// <summary>
    /// Gets the member's attributes.
    /// </summary>
    AttributeMap Attributes { get; }

    /// <summary>
    /// Gets the member's full name.
    /// </summary>
    QualifiedName FullName { get; }

    /// <summary>
    /// Gets the member's unqualified name.
    /// </summary>
    UnqualifiedName Name { get; }
}