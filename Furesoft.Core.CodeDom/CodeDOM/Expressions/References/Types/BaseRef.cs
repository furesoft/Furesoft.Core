// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to the base class of the current object instance.
    /// </summary>
    public class BaseRef : SelfRef
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "base";

        /// <summary>
        /// Create a <see cref="BaseRef"/>.
        /// </summary>
        public BaseRef(bool isFirstOnLine)
            : base(isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="BaseRef"/>.
        /// </summary>
        public BaseRef()
            : base(false)
        { }

        protected BaseRef(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past 'base'
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
                // Evaluate to the base type declaration, so that most properties and methods
                // will function according to it.
                TypeDecl typeDecl = FindParent<TypeDecl>();
                return (typeDecl != null ? typeDecl.GetBaseType().Reference : null);
            }
        }

        /// <summary>
        /// Parse a <see cref="BaseRef"/>.
        /// </summary>
        public static BaseRef Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new BaseRef(parser, parent);
        }

        internal static new void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse);
        }
    }
}