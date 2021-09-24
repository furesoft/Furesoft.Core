// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.Parsing;
using System;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Properties
{
    /// <summary>
    /// Represents a special type of method used to read the value of a property.
    /// </summary>
    /// <remarks>
    /// A GetterDecl acquires its return type and internal name from its parent <see cref="PropertyDecl"/> or <see cref="IndexerDecl"/>.
    /// The return type will be null if no Parent has been set yet.
    /// </remarks>
    public class GetterDecl : AccessorDecl
    {
        /// <summary>
        /// The internal name prefix.
        /// </summary>
        public const string NamePrefix = ParseToken + "_";

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "get";

        /// <summary>
        /// Create a <see cref="GetterDecl"/>.
        /// </summary>
        public GetterDecl(Modifiers modifiers, CodeObject body)
            : base(NamePrefix, null, modifiers, body)
        { }

        /// <summary>
        /// Create a <see cref="GetterDecl"/>.
        /// </summary>
        public GetterDecl(Modifiers modifiers)
            : base(NamePrefix, null, modifiers)
        { }

        /// <summary>
        /// Create a <see cref="GetterDecl"/>.
        /// </summary>
        public GetterDecl()
            : base(NamePrefix, null, Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="GetterDecl"/>.
        /// </summary>
        public GetterDecl(CodeObject body)
            : base(NamePrefix, null, body)
        { }

        protected GetterDecl(Parser parser, CodeObject parent, ParseFlags flags)
                    : base(parser, parent, NamePrefix, flags)
        {
            ParseAccessor(parser, flags);
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The return type of the method (never null - will be type 'void' instead).
        /// </summary>
        public override Expression ReturnType
        {
            // The return type of a GetterDecl is always the type of the parent PropertyDeclBase
            get { return ((PropertyDeclBase)_parent).Type; }
            set { throw new Exception("Can't set the ReturnType of a GetterDecl - change the type of the PropertyDecl instead."); }
        }

        public static new void AddParsePoints()
        {
            // Parse 'get' for Properties, Indexers, and Events
            // (analysis will complain in the last case that it should be 'add' instead)
            Parser.AddParsePoint(ParseToken, Parse, typeof(PropertyDeclBase));
        }

        /// <summary>
        /// Parse a <see cref="GetterDecl"/>.
        /// </summary>
        public static new GetterDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new GetterDecl(parser, parent, flags);
        }
    }
}