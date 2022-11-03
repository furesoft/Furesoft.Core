using System.Collections.Generic;
using Furesoft.Core.CodeDom.Compiler.Core.Collections;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Core;

namespace Furesoft.Core.CodeDom.Backends.CLR;

/// <summary>
/// An IL type that modifies a type by slapping on a required
/// or optional modifier.
/// </summary>
public sealed class ClrModifierType : ContainerType
{
    private static InterningCache<ClrModifierType> instanceCache
        = new(
            new StructuralModifierTypeComparer(),
            InitializeInstance);

    internal ClrModifierType(IType elementType, IType modifierType, bool isRequired)
                : base(elementType)
    {
        this.ModifierType = modifierType;
        this.IsRequired = isRequired;
    }

    /// <summary>
    /// Tells if this type is a modreq type; if it is not, then it must
    /// be a modopt type.
    /// </summary>
    /// <value>
    /// <c>true</c> if this type is a modreq type; <c>false</c> if this type is a modopt type.
    /// </value>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Gets the modifier type that is applied to the element type.
    /// </summary>
    /// <value>The modifier type.</value>
    public IType ModifierType { get; private set; }

    /// <summary>
    /// Creates a modreq or modopt type.
    /// </summary>
    /// <param name="elementType">
    /// An element type to associate with a modifier type.
    /// </param>
    /// <param name="modifierType">
    /// A modifier type to slap onto <paramref name="elementType"/>.
    /// </param>
    /// <param name="isRequired">
    /// A Boolean flag that is <c>true</c> if a modreq type is to be
    /// created and <c>false</c> otherwise.
    /// </param>
    /// <returns>A modreq or modopt type.</returns>
    public static ClrModifierType Create(IType elementType, IType modifierType, bool isRequired)
    {
        return instanceCache.Intern(new ClrModifierType(elementType, modifierType, isRequired));
    }

    /// <summary>
    /// Creates a modopt type.
    /// </summary>
    /// <param name="elementType">
    /// An element type to associate with a modifier type.
    /// </param>
    /// <param name="modifierType">
    /// A modifier type to slap onto <paramref name="elementType"/>.
    /// </param>
    /// <returns>A modopt type.</returns>
    public static ClrModifierType CreateOptional(IType elementType, IType modifierType)
    {
        return Create(elementType, modifierType, false);
    }

    /// <summary>
    /// Creates a modreq type.
    /// </summary>
    /// <param name="elementType">
    /// An element type to associate with a modifier type.
    /// </param>
    /// <param name="modifierType">
    /// A modifier type to slap onto <paramref name="elementType"/>.
    /// </param>
    /// <returns>A modreq type.</returns>
    public static ClrModifierType CreateRequired(IType elementType, IType modifierType)
    {
        return Create(elementType, modifierType, true);
    }

    /// <inheritdoc/>
    public override ContainerType WithElementType(IType newElementType)
    {
        return Create(newElementType, ModifierType, IsRequired);
    }

    private static ClrModifierType InitializeInstance(ClrModifierType instance)
    {
        instance.Initialize(
            new SimpleName(instance.ElementType.Name.ToString() + "!" + instance.ModifierType.Name.ToString()),
            new SimpleName(instance.ElementType.FullName.ToString() + "!" + instance.ModifierType.FullName.ToString()).Qualify(),
            instance.ElementType.Attributes);
        return instance;
    }
}

internal sealed class StructuralModifierTypeComparer : IEqualityComparer<ClrModifierType>
{
    public bool Equals(ClrModifierType x, ClrModifierType y)
    {
        return x.ElementType == y.ElementType
            && x.ModifierType == y.ModifierType
            && x.IsRequired == y.IsRequired;
    }

    public int GetHashCode(ClrModifierType obj)
    {
        var hashCode = EnumerableComparer.EmptyHash;
        hashCode = EnumerableComparer.FoldIntoHashCode(hashCode, obj.ElementType);
        hashCode = EnumerableComparer.FoldIntoHashCode(hashCode, obj.ModifierType);
        hashCode = EnumerableComparer.FoldIntoHashCode(hashCode, obj.IsRequired);
        return hashCode;
    }
}