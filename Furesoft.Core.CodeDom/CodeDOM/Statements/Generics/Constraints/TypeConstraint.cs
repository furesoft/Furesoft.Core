// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Constrains the type that a <see cref="TypeParameter"/> can represent to the specified type (or a derived type).
    /// The constraining type can be a class type, an interface type, or a type parameter type.
    /// </summary>
    public class TypeConstraint : TypeParameterConstraint
    {
        #region /* FIELDS */

        /// <summary>
        /// Should evaluate to a reference to a class type, interface type, or type parameter type.
        /// </summary>
        protected Expression _type;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="TypeConstraint"/> instance.
        /// </summary>
        /// <param name="type">An <see cref="Expression"/> that evaluates to a <see cref="TypeRef"/>.</param>
        public TypeConstraint(Expression type)
        {
            Type = type;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The type <see cref="Expression"/>.
        /// </summary>
        public Expression Type
        {
            get { return _type; }
            set { SetField(ref _type, value, true); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            TypeConstraint clone = (TypeConstraint)base.Clone();
            clone.CloneField(ref clone._type, _type);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse a <see cref="TypeConstraint"/>.
        /// </summary>
        public TypeConstraint(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            SetField(ref _type, Expression.Parse(parser, this), true);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_type == null || (!_type.IsFirstOnLine && _type.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _type != null)
                {
                    _type.IsFirstOnLine = false;
                    _type.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextConstraint(CodeWriter writer, RenderFlags flags)
        {
            _type.AsText(writer, flags);
        }

        #endregion
    }
}
