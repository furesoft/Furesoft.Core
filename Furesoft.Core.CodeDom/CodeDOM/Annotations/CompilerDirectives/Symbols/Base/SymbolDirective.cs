using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Symbols.Base
{
    /// <summary>
    /// The common base class of <see cref="DefineSymbol"/> and <see cref="UnDefSymbol"/>.
    /// </summary>
    public abstract class SymbolDirective : CompilerDirective
    {
        #region /* FIELDS */

        protected string _symbol;

        #endregion

        #region /* CONSTRUCTORS */

        protected SymbolDirective(string symbol)
        {
            _symbol = symbol;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The associated symbol name.
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        #endregion

        #region /* PARSING */

        protected SymbolDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected void ParseSymbol(Parser parser)
        {
            Token token = parser.NextTokenSameLine(false);  // Move past directive keyword
            if (token != null)
            {
                _symbol = parser.TokenText;  // Parse symbol name
                parser.NextToken();          // Move past name
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(_symbol);
        }

        #endregion
    }
}
