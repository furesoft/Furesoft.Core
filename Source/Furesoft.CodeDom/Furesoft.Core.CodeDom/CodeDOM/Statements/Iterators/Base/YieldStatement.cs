using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Iterators.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Iterators;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Iterators.Base
{
    /// <summary>
    /// The common base class of <see cref="YieldReturn"/> and <see cref="YieldBreak"/>.
    /// </summary>
    public abstract class YieldStatement : Statement
    {
        /// <summary>
        /// The token used to parse yield statements.
        /// </summary>
        public const string ParseToken1 = "yield";

        protected YieldStatement()
        { }

        protected YieldStatement(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        public static void AddParsePoints()
        {
            // Set parse-point on 1st of the 2 keywords
            Parser.AddParsePoint(ParseToken1, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="YieldBreak"/> or <see cref="YieldReturn"/>.
        /// </summary>
        public static YieldStatement Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Only parse 'yield' if it's followed by 'break' or 'return'
            string nextTokenText = parser.PeekNextTokenText();
            if (nextTokenText == YieldBreak.ParseToken2)
                return new YieldBreak(parser, parent);
            if (nextTokenText == YieldReturn.ParseToken2)
                return new YieldReturn(parser, parent);
            return null;
        }
    }
}
