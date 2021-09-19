using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a named <see cref="Block"/> of code used to destruct the instance of a class.
    /// It has no modifiers or parameters, can't be inherited or overloaded, and can't be called
    /// directly.  Only classes can have them, and they can only have one.
    /// </summary>
    public class DestructorDecl : MethodDeclBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "~";

        /// <summary>
        /// Create a <see cref="DestructorDecl"/>.
        /// </summary>
        public DestructorDecl()
            : base(null, null)
        { }

        protected DestructorDecl(Parser parser, CodeObject parent, ParseFlags flags)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past '~'
            ParseMethodNameAndType(parser, parent, false, false);
            _name = null;                          // Clear name (we use the parent's name instead)
            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers
            ParseParameters(parser);
            ParseTerminatorOrBody(parser, flags);
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "destructor"; }
        }

        /// <summary>
        /// The name of the <see cref="DestructorDecl"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                // Always use the name of the current parent TypeDecl.
                // The _name field should always be null.
                if (_parent != null)
                    return ((TypeDecl)_parent).Name;
                return null;
            }
        }

        public static void AddParsePoints()
        {
            // Use a parse-priority of 0 (Complement uses 100)
            Parser.AddParsePoint(ParseToken, Parse, typeof(TypeDecl));
        }

        /// <summary>
        /// Parse a <see cref="DestructorDecl"/>.
        /// </summary>
        public static DestructorDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DestructorDecl(parser, parent, flags);
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public override string GetFullName(bool descriptive)
        {
            string name = ParseToken + Name;
            if (descriptive)
                name += ParameterDecl.ParseTokenStart + ParameterDecl.ParseTokenEnd;
            if (_parent is TypeDecl)
                name = ((TypeDecl)_parent).GetFullName(descriptive) + "." + name;
            return name;
        }

        internal override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ParseToken + Name);
        }
    }
}