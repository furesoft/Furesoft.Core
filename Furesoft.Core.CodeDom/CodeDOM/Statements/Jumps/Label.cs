using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a named entry point that can be targeted by one or more <see cref="Goto"/> statements.
    /// </summary>
    public class Label : Statement, INamedCodeObject
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = ":";

        protected string _name;

        /// <summary>
        /// Create a <see cref="Label"/> with the specified name.
        /// </summary>
        public Label(string name)
        {
            _name = name;
        }

        public Label(Parser parser, CodeObject parent)
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

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "label"; }
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
        /// The name of the <see cref="Label"/>.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The terminator character for the <see cref="Statement"/>.
        /// </summary>
        public override string Terminator
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
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

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            // Prefix Labels and SwitchItems with a ':' to segregate them
            dictionary.Add(ParseToken + Name, this);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            writer.BeginOutdentOnNewLine(this, -TabSize);
            base.AsText(writer, flags);
            writer.EndIndentation(this);
        }

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

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(ParseToken + Name, this);
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
    }
}