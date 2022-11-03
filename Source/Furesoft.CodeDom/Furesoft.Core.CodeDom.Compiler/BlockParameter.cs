using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Furesoft.Core.CodeDom.Compiler;

/// <summary>
/// A parameter to a basic block.
/// </summary>
public struct BlockParameter
{
    /// <summary>
    /// Creates a block parameter from a type. An anonymous tag
    /// is automatically generated for the block parameter.
    /// </summary>
    /// <param name="type">The block parameter's type.</param>
    public BlockParameter(IType type)
        : this(type, new ValueTag(), false)
    { }

    /// <summary>
    /// Creates a block parameter from a type and a name.
    /// </summary>
    /// <param name="type">The block parameter's type.</param>
    /// <param name="name">
    /// The name to assign to an automatically-created
    /// block parameter tag.
    /// </param>
    public BlockParameter(IType type, string name, bool isConst = false)
        : this(type, new ValueTag(name), isConst)
    { }

    /// <summary>
    /// Creates a block parameter from a type and a tag.
    /// </summary>
    /// <param name="type">The block parameter's type.</param>
    /// <param name="tag">The block parameter's tag.</param>
    public BlockParameter(IType type, ValueTag tag, bool isConst)
    {
        this = default(BlockParameter);
        Type = type;
        Tag = tag;
        IsConst = isConst;
    }

    /// <summary>
    /// Gets this block parameter's tag.
    /// </summary>
    /// <returns>The block parameter's tag.</returns>
    public ValueTag Tag { get; private set; }

    public bool IsConst { get; set; }

    /// <summary>
    /// Gets this block parameter's type.
    /// </summary>
    /// <returns>The block parameter's type.</returns>
    public IType Type { get; private set; }

    /// <summary>
    /// Implicitly converts a block parameter to its tag.
    /// </summary>
    /// <param name="parameter">
    /// The block parameter to convert.
    /// </param>
    public static implicit operator ValueTag(BlockParameter parameter)
    {
        return parameter.Tag;
    }

    /// <summary>
    /// Applies a member mapping to this block parameter.
    /// </summary>
    /// <param name="mapping">A member mapping.</param>
    /// <returns>A transformed block parameter.</returns>
    public BlockParameter Map(MemberMapping mapping)
    {
        return new BlockParameter(mapping.MapType(Type), Tag, false);
    }
}