using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using System;

namespace Furesoft.Core.CodeDom.Compiler.Core.Names;

/// <summary>
/// Defines a poiner name: a qualified name that is turned into a pointer.
/// </summary>
public class PointerName : UnqualifiedName, IEquatable<PointerName>
{
    /// <summary>
    /// Creates a pointer name from a qualified name and a pointer kind.
    /// </summary>
    /// <param name="elementName">
    /// The name of the element type in this pointer name.
    /// </param>
    /// <param name="kind">
    /// The kind of pointer named by this pointer name.
    /// </param>
    public PointerName(QualifiedName elementName, PointerKind kind)
    {
        ElementName = elementName;
        Kind = kind;
    }

    /// <summary>
    /// Gets the qualified name that is turned into a pointer.
    /// </summary>
    public QualifiedName ElementName { get; private set; }

    /// <summary>
    /// Gets this pointer name's pointer kind.
    /// </summary>
    public PointerKind Kind { get; private set; }

    /// <inheritdoc/>
    public bool Equals(PointerName other)
    {
        return ElementName.Equals(other.ElementName)
            && Kind == other.Kind;
    }

    /// <inheritdoc/>
    public override bool Equals(UnqualifiedName Other)
    {
        return Other is PointerName && Equals((PointerName)Other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (ElementName.GetHashCode() << 2) ^ Kind.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ElementName.ToString() + " " + Kind.ToString() + "*";
    }
}