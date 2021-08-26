// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents one or more contraints on a type parameter.
    /// </summary>
    /// <remarks>
    /// The constraints may consist of:
    ///    - a <see cref="ClassConstraint"/>
    ///    - a <see cref="StructConstraint"/>
    ///    - a <see cref="TypeConstraint"/> for a base class
    ///    - one or more <see cref="TypeConstraint"/>s for interfaces (which can also be generic)
    ///    - a <see cref="TypeConstraint"/> for a base class followed by one or more <see cref="TypeConstraint"/>s for interfaces
    ///    - a <see cref="TypeConstraint"/> for another <see cref="TypeParameter"/> (a "naked type constraint")
    ///    - a <see cref="NewConstraint"/> by itself or in addition to any of the above (must be at the end)
    /// </remarks>
    public class ConstraintClause : CodeObject
    {
        protected ChildList<TypeParameterConstraint> _constraints;
        protected SymbolicRef _typeParameter;

        /// <summary>
        /// Create a <see cref="ConstraintClause"/>.
        /// </summary>
        public ConstraintClause(SymbolicRef symbolicRef, params TypeParameterConstraint[] constraints)
        {
            TypeParameter = symbolicRef;
            CreateConstraints().AddRange(constraints);
        }

        /// <summary>
        /// Create a <see cref="ConstraintClause"/>.
        /// </summary>
        public ConstraintClause(TypeParameterRef typeParameterRef, params TypeParameterConstraint[] constraints)
            : this((SymbolicRef)typeParameterRef, constraints)
        { }

        /// <summary>
        /// Create a <see cref="ConstraintClause"/>.
        /// </summary>
        public ConstraintClause(TypeParameter typeParameter, params TypeParameterConstraint[] constraints)
            : this(typeParameter.CreateRef(), constraints)
        { }

        /// <summary>
        /// The list of <see cref="TypeParameterConstraint"/>s.
        /// </summary>
        public ChildList<TypeParameterConstraint> Constraints
        {
            get { return _constraints; }
        }

        /// <summary>
        /// The <see cref="TypeParameter"/> being constrained.
        /// </summary>
        public SymbolicRef TypeParameter
        {
            get { return _typeParameter; }
            set { SetField(ref _typeParameter, value, true); }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ConstraintClause clone = (ConstraintClause)base.Clone();
            clone.CloneField(ref clone._typeParameter, _typeParameter);
            clone._constraints = ChildListHelpers.Clone(_constraints, clone);
            return clone;
        }

        /// <summary>
        /// Create the list of <see cref="TypeParameterConstraint"/>s, or return the existing one.
        /// </summary>
        public ChildList<TypeParameterConstraint> CreateConstraints()
        {
            if (_constraints == null)
                _constraints = new ChildList<TypeParameterConstraint>(this);
            return _constraints;
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "where";

        /// <summary>
        /// The token used to parse between the constraint type parameter and constraints.
        /// </summary>
        public const string ParseTokenSeparator = ":";

        /// <summary>
        /// Parse a <see cref="ConstraintClause"/>.
        /// </summary>
        public ConstraintClause(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Get any regular comments from before the 'where'
            MoveComments(parser.LastToken);

            parser.NextToken();  // Move past 'where'
            SetField(ref _typeParameter, UnresolvedRef.Create(parser.GetIdentifier()), false);
            ParseExpectedToken(parser, ParseTokenSeparator);  // Move past ':'

            // Parse list of TypeParameterConstraints
            while (true)
            {
                CreateConstraints().Add(TypeParameterConstraint.Parse(parser, this));
                if (parser.TokenText == TypeParameterConstraint.ParseTokenSeparator)
                    parser.NextToken();  // Move past ','
                else
                    break;
            }

            // Get the EOL comment from the Type expression of the last constraint if it's a TypeConstraint
            // (it will end up there since they use an Expression), otherwise from the LastToken.
            if (_constraints != null && _constraints.Last is TypeConstraint)
                MoveEOLComment(((TypeConstraint)_constraints.Last).Type);
            else
                MoveEOLComment(parser.LastToken);
        }

        /// <summary>
        /// Parse a list of constraint clauses.
        /// </summary>
        public static ChildList<ConstraintClause> ParseList(Parser parser, CodeObject parent)
        {
            ChildList<ConstraintClause> constraints = null;
            while (parser.TokenText == ParseToken)
            {
                if (constraints == null)
                    constraints = new ChildList<ConstraintClause>(parent);
                constraints.Add(new ConstraintClause(parser, parent));
            }
            return constraints;
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
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
                return (base.IsSingleLine && (_typeParameter == null || (!_typeParameter.IsFirstOnLine && _typeParameter.IsSingleLine))
                    && (_constraints == null || _constraints.Count == 0 || (!_constraints[0].IsFirstOnLine && _constraints.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_typeParameter != null)
                    {
                        _typeParameter.IsFirstOnLine = false;
                        _typeParameter.IsSingleLine = true;
                    }
                    if (_constraints != null && _constraints.Count > 0)
                    {
                        _constraints[0].IsFirstOnLine = false;
                        _constraints.IsSingleLine = true;
                    }
                }
            }
        }

        public static void AsTextConstraints(CodeWriter writer, ChildList<ConstraintClause> constraints, RenderFlags flags)
        {
            if (constraints != null && constraints.Count > 0)
                writer.WriteList(constraints, flags | RenderFlags.NoItemSeparators | (constraints[0].IsFirstOnLine ? 0 : RenderFlags.PrefixSpace), constraints.Parent);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            int newLines = NewLines;
            if (newLines > 0)
            {
                if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                    writer.WriteLines(newLines);
            }
            else if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");
            flags &= ~RenderFlags.SuppressNewLine;

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);

            // Increase the indent level for any newlines that occur within the statement if the flag is set
            bool increaseIndent = flags.HasFlag(RenderFlags.IncreaseIndent);
            if (increaseIndent)
                writer.BeginIndentOnNewLine(this);

            writer.Write(ParseToken);
            RenderFlags noDescFlags = passFlags & ~(RenderFlags.Description | RenderFlags.ShowParentTypes);
            if (_typeParameter != null)
                _typeParameter.AsText(writer, noDescFlags | RenderFlags.IsPrefix | RenderFlags.PrefixSpace);
            writer.Write(ParseTokenSeparator + " ");
            writer.WriteList(_constraints, noDescFlags, this);
            AsTextEOLComments(writer, flags);

            if (increaseIndent)
                writer.EndIndentation(this);

            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));
        }
    }
}