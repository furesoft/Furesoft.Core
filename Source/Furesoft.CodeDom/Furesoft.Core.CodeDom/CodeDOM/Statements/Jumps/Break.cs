using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps
{
    /// <summary>
    /// Causes execution of the active loop or switch/case block to terminate.
    /// </summary>
    public class Break : Statement
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "break";

        /// <summary>
        /// Create a <see cref="Break"/>.
        /// </summary>
        public Break()
        { }

        protected Break(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past 'break'
            ParseTerminator(parser);
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Pase a <see cref="Break"/>.
        /// </summary>
        public static Break Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Break(parser, parent);
        }
    }
}
