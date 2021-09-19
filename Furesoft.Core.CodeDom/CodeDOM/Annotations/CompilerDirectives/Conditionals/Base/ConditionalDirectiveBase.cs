using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
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