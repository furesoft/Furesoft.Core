using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.AnonymousMethods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods
{
    /// <summary>
    /// Represents a reference to an <see cref="AnonymousMethod"/>.
    /// </summary>
    /// <remarks>
    /// Although anonymous methods don't have names, they can still be referenced, such as when the
    /// type of an expression is evaluated, resulting in an <see cref="AnonymousMethodRef"/>, which
    /// can be used during the resolution process, such as calling IsImplicitlyConvertibleTo() on it.
    /// </remarks>
    public class AnonymousMethodRef : MethodRef
    {
        /// <summary>
        /// Create an <see cref="AnonymousMethodRef"/> from an <see cref="AnonymousMethod"/>.
        /// </summary>
        public AnonymousMethodRef(AnonymousMethod anonymousMethod, bool isFirstOnLine)
            : base(anonymousMethod, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="AnonymousMethodRef"/> from an <see cref="AnonymousMethod"/>.
        /// </summary>
        public AnonymousMethodRef(AnonymousMethod anonymousMethod)
            : base(anonymousMethod, false)
        { }

        /// <summary>
        /// Always returns null for an <see cref="AnonymousMethodRef"/>.
        /// </summary>
        public override string Name
        {
            // Anonymous methods don't have a name
            get { return null; }
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Always render an AnonymousMethodRef as a description (which will render it's delegate type)
            ((AnonymousMethod)_reference).AsText(writer, flags | RenderFlags.SuppressNewLine | RenderFlags.Description);
        }

        /// <summary>
        /// Always returns <c>null</c>.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            return null;
        }

        /// <summary>
        /// Determine the return type of the anonymous method (never null - will be 'void' instead).
        /// </summary>
        public override TypeRefBase GetReturnType()
        {
            return ((AnonymousMethod)_reference).GetReturnType();
        }
    }
}
