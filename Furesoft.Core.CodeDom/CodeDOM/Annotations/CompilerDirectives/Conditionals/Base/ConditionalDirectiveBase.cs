using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base
{
    /// <summary>
    /// The common base class of <see cref="IfDirective"/>, <see cref="ElIfDirective"/>, <see cref="ElseDirective"/>, and <see cref="EndIfDirective"/>.
    /// </summary>
    public class ConditionalDirectiveBase : CompilerDirective
    {
        #region /* CONSTRUCTORS */

        protected ConditionalDirectiveBase()
        { }

        #endregion

        #region /* PARSING */

        protected ConditionalDirectiveBase(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
