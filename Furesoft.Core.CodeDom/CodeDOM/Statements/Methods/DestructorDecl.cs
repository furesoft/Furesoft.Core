// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a named <see cref="Block"/> of code used to destruct the instance of a class.
    /// It has no modifiers or parameters, can't be inherited or overloaded, and can't be called
    /// directly.  Only classes can have them, and they can only have one.
    /// </summary>
    public class DestructorDecl : MethodDeclBase
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DestructorDecl"/>.
        /// </summary>
        public DestructorDecl()
            : base(null, null)
        { }

        #endregion

        #region /* PROPERTIES */

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

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "destructor"; }
        }

        #endregion

        #region /* METHODS */

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

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "~";

        internal static void AddParsePoints()
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

        #endregion

        #region /* RENDERING */

        internal override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(ParseToken + Name);
        }

        #endregion
    }
}
