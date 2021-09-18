// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to the current object instance.
    /// </summary>
    public class ThisRef : SelfRef
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "this";

        /// <summary>
        /// Create a <see cref="BaseRef"/>.
        /// </summary>
        public ThisRef(bool isFirstOnLine)
            : base(isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="BaseRef"/>.
        /// </summary>
        public ThisRef()
            : base(false)
        { }

        protected ThisRef(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past 'this'
        }

        /// <summary>
        /// The keyword associated with the <see cref="SelfRef"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The name of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Name
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The code object to which the <see cref="SymbolicRef"/> refers.
        /// </summary>
        public override object Reference
        {
            get
            {
                // Evaluate to the current type declaration, so that most properties and methods
                // will function according to it.
                return FindParent<TypeDecl>();
            }
        }

        /// <summary>
        /// Parse a <see cref="ThisRef"/>.
        /// </summary>
        public static SymbolicRef Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Handle the special case of 'this' being used on the right side of a Dot for an explicit interface declaration
            // of an indexer, or to specify an Indexer member of a type, such as 'IDictionary.this[object]' (which can occur
            // in a doc comment).
            if (parent is Dot && ((Dot)parent).Left != null)
            {
                parser.NextToken();  // Move past 'this'
                return new UnresolvedThisRef(parser.LastToken);
            }

            // By default, assume 'this' is a ThisRef
            return new ThisRef(parser, parent);
        }

        internal static new void AddParsePoints()
        {
            // When 'this' is used for constructor initializers, it's not at Block scope, and is
            // parsed by the ConstructorInitializer base class of ThisInitializer instead of here.
            Parser.AddParsePoint(ParseToken, Parse);
        }
    }
}