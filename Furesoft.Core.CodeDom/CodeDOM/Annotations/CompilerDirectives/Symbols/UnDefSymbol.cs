using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols
{
    /// <summary>
    /// Allows for removing an existing definition of a "pre-processor" symbol - may only appear at the top of a <see cref="CodeUnit"/> (file)!
    /// </summary>
    public class UnDefSymbol : SymbolDirective
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="UnDefSymbol"/>.
        /// </summary>
        public UnDefSymbol(string symbol)
            : base(symbol)
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "undef";

        internal static void AddParsePoints()
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

        /// <summary>
        /// Parse an <see cref="UnDefSymbol"/>.
        /// </summary>
        public UnDefSymbol(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseSymbol(parser);
            parser.CodeUnit.UndefineCompilerDirectiveSymbol(_symbol);
        }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        #endregion
    }
}
