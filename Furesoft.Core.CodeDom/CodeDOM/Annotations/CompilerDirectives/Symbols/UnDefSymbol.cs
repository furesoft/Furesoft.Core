using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Allows for removing an existing definition of a "pre-processor" symbol - may only appear at the top of a <see cref="CodeUnit"/> (file)!
    /// </summary>
    public class UnDefSymbol : SymbolDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "undef";

        /// <summary>
        /// Create an <see cref="UnDefSymbol"/>.
        /// </summary>
        public UnDefSymbol(string symbol)
            : base(symbol)
        { }

        /// <summary>
        /// Parse an <see cref="UnDefSymbol"/>.
        /// </summary>
        public UnDefSymbol(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseSymbol(parser);
            parser.CodeUnit.UndefineCompilerDirectiveSymbol(_symbol);
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse an <see cref="UnDefSymbol"/>.
        /// </summary>
        public static UnDefSymbol Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new UnDefSymbol(parser, parent);
        }
    }
}