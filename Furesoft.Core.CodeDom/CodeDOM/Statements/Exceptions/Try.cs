// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Contains a body of code that is scoped to one or more <see cref="Catch"/> statements and/or a <see cref="Finally"/> statement.
    /// </summary>
    public class Try : BlockStatement
    {
        #region /* FIELDS */

        protected ChildList<Catch> _catches;
        protected Finally _finally;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Try"/>.
        /// </summary>
        public Try()
        { }

        /// <summary>
        /// Create a <see cref="Try"/>.
        /// </summary>
        public Try(CodeObject body, Finally @finally)
            : base(body, false)
        {
            Finally = @finally;
        }

        /// <summary>
        /// Create a <see cref="Try"/>.
        /// </summary>
        public Try(CodeObject body, Finally @finally, params Catch[] catches)
            : this(body, @finally)
        {
            CreateCatches().AddRange(catches);
        }

        /// <summary>
        /// Create a <see cref="Try"/>.
        /// </summary>
        public Try(CodeObject body, params Catch[] catches)
            : this(body, null, catches)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// A collection of <see cref="Catch"/>es.
        /// </summary>
        public ChildList<Catch> Catches
        {
            get { return _catches; }
        }

        /// <summary>
        /// True if there are any <see cref="Catch"/>es.
        /// </summary>
        public bool HasCatches
        {
            get { return (_catches != null && _catches.Count > 0); }
        }

        /// <summary>
        /// An optional <see cref="Finally"/>.
        /// </summary>
        public Finally Finally
        {
            get { return _finally; }
            set { SetField(ref _finally, value, false); }
        }

        /// <summary>
        /// True if there is a <see cref="Finally"/>.
        /// </summary>
        public bool HasFinally
        {
            get { return (_finally != null); }
        }

        /// <summary>
        /// True for multi-part statements, such as try/catch/finally or if/else.
        /// </summary>
        public override bool IsMultiPart
        {
            get { return (HasCatches || HasFinally); }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create the list of <see cref="Catch"/>s, or return the existing one.
        /// </summary>
        public ChildList<Catch> CreateCatches()
        {
            if (_catches == null)
                _catches = new ChildList<Catch>(this);
            return _catches;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Try clone = (Try)base.Clone();
            clone._catches = ChildListHelpers.Clone(_catches, clone);
            clone.CloneField(ref clone._finally, _finally);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "try";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="Try"/>.
        /// </summary>
        public static Try Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Try(parser, parent);
        }

        protected Try(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Flush any unused objects first, so that they don't interfere with skipping
            // compiler directives below, or the parsing of any 'else' part.
            if (parser.HasUnused && _parent is BlockStatement)
                ((BlockStatement)_parent).Body.FlushUnused(parser);

            parser.NextToken();                        // Move past 'try'
            new Block(out _body, parser, this, true);  // Parse the body

            // Skip over any compiler directives that might occur before a 'catch' or 'finally', adding them to the unused list
            ParseAnnotations(parser, parent, false, true);

            while (parser.TokenText == Catch.ParseToken || parser.TokenText == Finally.ParseToken)
            {
                // Parse optional 'catch' child part
                while (parser.TokenText == Catch.ParseToken)
                {
                    CreateCatches().Add(Catch.Parse(parser, this));

                    // Skip over any compiler directives that might occur before a 'catch' or 'finally', adding them to the unused list
                    ParseAnnotations(parser, parent, false, true);
                }

                // Parse optional 'finally' child part
                if (parser.TokenText == Finally.ParseToken)
                    _finally = Finally.Parse(parser, this);
            }

            // If we skipped any compiler directives that weren't followed by a 'catch' or 'finally', we have
            // to move them now so they can be manually flushed by the parent Block *after* this statement.
            if (parser.HasUnused)
                parser.MoveUnusedToPostUnused();
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            base.Resolve(ResolveCategory.CodeObject, flags);
            if (HasCatches)
            {
                foreach (Catch @catch in _catches)
                    @catch.Resolve(ResolveCategory.CodeObject, flags);
            }
            if (HasFinally)
                _finally.Resolve(ResolveCategory.CodeObject, flags);
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (ChildListHelpers.HasUnresolvedRef(_catches))
                return true;
            if (_finally != null && _finally.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
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
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_catches == null || _catches.Count == 0 || (!_catches[0].IsFirstOnLine && _catches.IsSingleLine))
                    && (_finally == null || (!_finally.IsFirstOnLine && _finally.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_catches != null && _catches.Count > 0)
                {
                    _catches[0].IsFirstOnLine = !value;
                    _catches.IsSingleLine = value;
                }
                if (_finally != null)
                {
                    _finally.IsFirstOnLine = !value;
                    _finally.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextAfter(writer, flags);
            if (HasCatches && !flags.HasFlag(RenderFlags.Description))
            {
                foreach (Catch @catch in _catches)
                    @catch.AsText(writer, flags | RenderFlags.PrefixSpace);
            }
            if (HasFinally && !flags.HasFlag(RenderFlags.Description))
                _finally.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        #endregion
    }
}
