using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base
{
    /// <summary>
    /// The common base class of <see cref="IfDirective"/>, <see cref="ElIfDirective"/>, <see cref="ElseDirective"/>, and <see cref="EndIfDirective"/>.
    /// </summary>
    public class ConditionalDirectiveBase : CompilerDirective
    {
        protected ConditionalDirectiveBase()
        { }

        protected ConditionalDirectiveBase(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }
    }
}
