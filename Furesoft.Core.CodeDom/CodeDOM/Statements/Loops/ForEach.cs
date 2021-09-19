// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using System;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Defines an iteration variable and a collection (or array) plus a body (a statement or block) that is
    /// repeatedly executed for each variable in the collection.
    /// </summary>
    /// <remarks>
    /// The body is required.
    /// If the collection is null, nothing happens - unlike C#, which throws an exception.
    /// The type of each object in the collection must be convertible to the type of the iteration variable.
    /// The collection expression must evaluate to a type that implements IEnumerable, or a type that
    /// declares a GetEnumerator method, which in turn must return a type that either implements IEnumerable
    /// or declares all of the methods defined in IEnumerator.
    /// </remarks>
    public class ForEach : BlockStatement
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "foreach";

        /// <summary>
        /// The token used to parse the 'in' part.
        /// </summary>
        public const string ParseTokenIn = "in";

        protected Expression _collection;
        protected LocalDecl _iteration;

        /// <summary>
        /// Create a <see cref="ForEach"/>.
        /// </summary>
        public ForEach(LocalDecl iteration, Expression collection, CodeObject body)
            : base(body, false)
        {
            Iteration = iteration;
            Collection = collection;
        }

        /// <summary>
        /// Create a <see cref="ForEach"/>.
        /// </summary>
        public ForEach(LocalDecl iteration, Expression collection)
            : base(null, false)
        {
            Iteration = iteration;
            Collection = collection;
        }

        protected ForEach(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past 'foreach'
            ParseExpectedToken(parser, Expression.ParseTokenStartGroup);  // Move past '('
            SetField(ref _iteration, LocalDecl.Parse(parser, this, false, false), false);
            ParseExpectedToken(parser, ParseTokenIn);
            SetField(ref _collection, Expression.Parse(parser, this, true, Expression.ParseTokenEndGroup), false);
            ParseExpectedToken(parser, Expression.ParseTokenEndGroup);  // Move past ')'

            new Block(out _body, parser, this, false);  // Parse the body
        }

        /// <summary>
        /// The collection being iterated over.
        /// </summary>
        public Expression Collection
        {
            get { return _collection; }
            set { SetField(ref _collection, value, true); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return true; }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
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
                return (base.IsSingleLine && (_iteration == null || (!_iteration.IsFirstOnLine && _iteration.IsSingleLine))
                    && (_collection == null || (!_collection.IsFirstOnLine && _collection.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_iteration != null)
                    {
                        _iteration.IsFirstOnLine = false;
                        _iteration.IsSingleLine = true;
                    }
                    if (_collection != null)
                    {
                        _collection.IsFirstOnLine = false;
                        _collection.IsSingleLine = true;
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="LocalDecl"/> iteration variable.
        /// </summary>
        public LocalDecl Iteration
        {
            get { return _iteration; }
            set
            {
                if (value != null && value.Parent != null)
                    throw new Exception("The LocalDecl used for the iteration variable of a ForEach must be new, not one already owned by another Parent object.");
                SetField(ref _iteration, value, true);
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="ForEach"/>.
        /// </summary>
        public static ForEach Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new ForEach(parser, parent);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ForEach clone = (ForEach)base.Clone();
            clone.CloneField(ref clone._iteration, _iteration);
            clone.CloneField(ref clone._collection, _collection);
            return clone;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_iteration != null)
                _iteration.AsText(writer, flags | RenderFlags.IsPrefix);
            writer.Write(ParseTokenIn);
            _collection.AsText(writer, flags | RenderFlags.PrefixSpace);
        }
    }
}