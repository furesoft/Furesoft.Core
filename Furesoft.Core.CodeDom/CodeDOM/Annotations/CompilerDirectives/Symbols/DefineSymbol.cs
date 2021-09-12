using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols
{
    /// <summary>
    /// Provides for the definition of a "pre-processor" symbol - may only appear at the very top of a <see cref="CodeUnit"/> (file)!
    /// </summary>
    public class DefineSymbol : SymbolDirective
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DefineSymbol"/>.
        /// </summary>
        public DefineSymbol(string symbol)
            : base(symbol)
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "define";

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DefineSymbol"/>.
        /// </summary>
        public static DefineSymbol Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DefineSymbol(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DefineSymbol"/>.
        /// </summary>
        public DefineSymbol(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseSymbol(parser);
            parser.CodeUnit.DefineCompilerDirectiveSymbol(_symbol);
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
