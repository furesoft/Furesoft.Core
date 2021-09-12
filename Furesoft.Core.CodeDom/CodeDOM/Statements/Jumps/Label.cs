using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.GotoTargets;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps
{
    /// <summary>
    /// Represents a named entry point that can be targeted by one or more <see cref="Goto"/> statements.
    /// </summary>
    public class Label : Statement, INamedCodeObject
    {
        #region /* FIELDS */

        protected string _name;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Label"/> with the specified name.
        /// </summary>
        public Label(string name)
        {
            _name = name;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="Label"/>.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "label"; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create a reference to the <see cref="Label"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="LabelRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new LabelRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            // Prefix Labels and SwitchItems with a ':' to segregate them
            dictionary.Add(ParseToken + Name, this);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(ParseToken + Name, this);
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName(bool descriptive)
        {
            return _name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return _name;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = ":";

        internal static void AddParsePoints()
        {
            // Use a parse-priority of 100 (NamedArgument uses 0)
            Parser.AddParsePoint(ParseToken, 100, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Pase a <see cref="Label"/>.
        /// </summary>
        public static Label Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Validate that we have an unused identifier token
            if (parser.HasUnusedIdentifier)
                return new Label(parser, parent);
            return null;
        }

        protected Label(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Get the name from the Unused list
            Token lastToken = parser.RemoveLastUnusedToken();
            MoveFormatting(lastToken);
            _name = lastToken.NonVerbatimText;
            SetLineCol(lastToken);
            parser.NextToken();  // Move past ':'
            HasTerminator = true;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// The terminator character for the <see cref="Statement"/>.
        /// </summary>
        public override string Terminator
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        #endregion

        #region /* RENDERING */

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            writer.BeginOutdentOnNewLine(this, -TabSize);
            base.AsText(writer, flags);
            writer.EndIndentation(this);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.WriteIdentifier(_name, flags);
        }

        protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            // Render the terminator even when in Description mode (for references)
            AsTextTerminator(writer, flags);
        }

        #endregion
    }
}
