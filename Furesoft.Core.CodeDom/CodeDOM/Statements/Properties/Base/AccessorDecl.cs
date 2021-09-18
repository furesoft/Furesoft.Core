// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="GetterDecl"/>, <see cref="SetterDecl"/>, <see cref="AdderDecl"/>,
    /// and <see cref="RemoverDecl"/>.
    /// </summary>
    /// <remarks>
    /// The return types of SetterDecls, AdderDecls, and RemoverDecls are always 'void'.  A GetterDecl
    /// takes on the type of its parent PropertyDecl or IndexerDecl as its return type (it will be
    /// null if it has no parent yet).
    /// Names aren't used for these methods in the GUI or C# source code, but the prefix that would
    /// be attached to the parent PropertyDecl, IndexerDecl, or EventDecl name to create the internal
    /// method name is stored, so that the internal name can be generated if/when needed.
    /// </remarks>
    public abstract class AccessorDecl : MethodDecl
    {
        #region /* CONSTRUCTORS */

        protected AccessorDecl(string namePrefix, Expression returnType, Modifiers modifiers, CodeObject body)
            : base(namePrefix, returnType, modifiers, body)
        { }

        protected AccessorDecl(string namePrefix, Expression returnType, Modifiers modifiers)
            : base(namePrefix, returnType, modifiers)
        { }

        protected AccessorDecl(string namePrefix, Expression returnType, CodeObject body)
            : base(namePrefix, returnType, body)
        { }

        #endregion

        #region /* PARSING */

        protected AccessorDecl(Parser parser, CodeObject parent, string namePrefix, ParseFlags flags)
            : base(parser, parent, false, flags)
        {
            _name = namePrefix;
        }

        protected void ParseAccessor(Parser parser, ParseFlags flags)
        {
            // Preserve the parsed NewLines value (MethodDeclBase forces it to 1 if it's 0), but
            // we'll allow accessors to be inlined by default.
            NewLines = parser.Token.NewLines;

            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers
            parser.NextToken();                    // Move past keyword
            ParseTerminatorOrBody(parser, flags);

            if (AutomaticFormattingCleanup && !parser.IsGenerated)
            {
                // Force property accessors (get/set) to a single line if they have a single-line body,
                // and remove any blank lines preceeding them in the property declaration (includes events).
                if (_body != null && _body.Count < 2 && _body.IsSingleLine)
                    IsSingleLine = true;
                if (NewLines > 1)
                    NewLines = 1;
            }
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
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to no blank lines before property members
            return 1;
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(Keyword);
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        { }

        #endregion
    }
}
