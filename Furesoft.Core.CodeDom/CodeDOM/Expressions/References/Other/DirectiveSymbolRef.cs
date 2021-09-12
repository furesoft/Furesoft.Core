// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a pre-processor directive symbol.
    /// </summary>
    /// <remarks>
    /// Directive symbols may be defined with <see cref="DefineSymbol"/> or on the compiler command-line,
    /// and may be undefined with <see cref="UnDefSymbol"/>.  As such, this reference doesn't refer to any
    /// declaration, but instead is a reference by string name only, and also is never marked as "unresolved".
    /// </remarks>
    public class DirectiveSymbolRef : SymbolicRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DirectiveSymbolRef"/>.
        /// </summary>
        public DirectiveSymbolRef(string name)
            : base(name, false)  // Directive symbols can never be the first thing on a line
        { }

        /// <summary>
        /// Create a <see cref="DirectiveSymbolRef"/>.
        /// </summary>
        protected internal DirectiveSymbolRef(Token token)
            : this(token.NonVerbatimText)
        {
            SetLineCol(token);
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Name
        {
            get { return (string)_reference; }
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // Return a boolean constant that's true if the symbol is defined
            return new TypeRef(FindParent<CodeUnit>().IsCompilerDirectiveSymbolDefined(Name));
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(Name);
        }

        #endregion
    }
}
