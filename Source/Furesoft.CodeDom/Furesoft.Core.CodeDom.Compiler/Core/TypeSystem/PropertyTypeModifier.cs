using Furesoft.Core.CodeDom.Compiler.Core.Constants;

namespace Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

/// <summary>
/// An enumeration of property type modifiers
/// </summary>
public enum PropertyTypeModifier
{
    /// <summary>
    /// Can be get
    /// </summary>
    Getter,

    /// <summary>
    /// Can be set
    /// </summary>
    Setter,

    /// <summary>
    /// Can only be set in constructor (immutability)
    /// </summary>
    Init,
}

/// <summary>
/// A collection of constants that relate to property type attributes.
/// </summary>
public static class PropertyTypeModifierAttribute
{
    /// <summary>
    /// The attribute name for access modifier attributes.
    /// </summary>
    public const string AttributeName = "PropertyTypeModifier";

    public static readonly IType AttributeType = IntrinsicAttribute.SynthesizeAttributeType(AttributeName);

    private static Dictionary<string, PropertyTypeModifier> invModifierNames;

    private static Dictionary<PropertyTypeModifier, string> modifierNames;

    static PropertyTypeModifierAttribute()
    {
        modifierNames = new Dictionary<PropertyTypeModifier, string>()
        {
            { PropertyTypeModifier.Getter, "getter" },
            { PropertyTypeModifier.Setter, "setter" },
            { PropertyTypeModifier.Init, "init" },
        };
        invModifierNames = new Dictionary<string, PropertyTypeModifier>();
        foreach (var pair in modifierNames)
        {
            invModifierNames[pair.Value] = pair.Key;
        }
    }

    /// <summary>
    /// Creates an property type modifier attribute that encodes an property type modifier.
    /// </summary>
    /// <param name="modifier">The property type modifier to encode.</param>
    /// <returns>An property type modifier attribute.</returns>
    public static IntrinsicAttribute Create(PropertyTypeModifier modifier)
    {
        return new IntrinsicAttribute(
            AttributeName,
            new[] { new StringConstant(modifierNames[modifier]) });
    }

    /// <summary>
    /// Gets a member's property type modifier. property types are getters by default.
    /// </summary>
    /// <param name="member">The member to examine.</param>
    /// <returns>The member's property type modifier if it has one; otherwise, getter.</returns>
    public static PropertyTypeModifier GetPropertyTypeModifier(this IMember member)
    {
        var attr = member.Attributes.GetOrNull(
            IntrinsicAttribute.GetIntrinsicAttributeType(AttributeName));
        if (attr == null)
        {
            return PropertyTypeModifier.Getter;
        }
        else
        {
            return Read((IntrinsicAttribute)attr);
        }
    }

    /// <summary>
    /// Reads out an access modifier attribute as an access modifier.
    /// </summary>
    /// <param name="attribute">The access modifier attribute to read.</param>
    /// <returns>The access modifier described by the attribute.</returns>
    public static PropertyTypeModifier Read(IntrinsicAttribute attribute)
    {
        ContractHelpers.Assert(attribute.Name == AttributeName);
        ContractHelpers.Assert(attribute.Arguments.Count == 1);
        return invModifierNames[((StringConstant)attribute.Arguments[0]).Value];
    }
}