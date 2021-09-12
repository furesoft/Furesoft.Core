using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types.Base
{
    /// <summary>
    /// The common base class of <see cref="ThisRef"/> and <see cref="BaseRef"/>.
    /// </summary>
    public abstract class SelfRef : SymbolicRef
    {
        #region /* CONSTRUCTORS */

        protected SelfRef(bool isFirstOnLine)
            : base((INamedCodeObject)null, isFirstOnLine)
        { }

        protected SelfRef()
            : base((INamedCodeObject)null, false)
        { }

        #endregion

        #region /* METHODS */

        #endregion

        #region /* PARSING */

        protected SelfRef(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Do NOT re-resolve a SelfRef
            return this;
        }

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            // Search the evaluated type of the expression for the type parameter
            TypeRefBase typeRefBase = EvaluateType();
            return (typeRefBase != null ? typeRefBase.FindTypeArgument(typeParameterRef, originatingChild) : null);
        }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// The keyword associated with the <see cref="SelfRef"/>.
        /// </summary>
        public abstract string Keyword
        {
            get;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(Keyword);
        }

        #endregion
    }
}
