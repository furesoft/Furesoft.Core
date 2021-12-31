using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types.Base
{
    /// <summary>
    /// The common base class of <see cref="ThisRef"/> and <see cref="BaseRef"/>.
    /// </summary>
    public abstract class SelfRef : SymbolicRef
    {
        protected SelfRef(bool isFirstOnLine)
            : base((INamedCodeObject)null, isFirstOnLine)
        { }

        protected SelfRef()
            : base((INamedCodeObject)null, false)
        { }

        protected SelfRef(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

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
    }
}
