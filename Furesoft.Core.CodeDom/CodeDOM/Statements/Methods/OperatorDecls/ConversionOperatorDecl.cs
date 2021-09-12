using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods.OperatorDecls;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Methods.OperatorDecls
{
    /// <summary>
    /// Represents a user-defined conversion operator.
    /// </summary>
    /// <remarks>
    /// Conversion operators must have either the implicit or explicit modifier, a single parameter,
    /// and either the parameter type OR the destination type (return type) must be the containing type.
    /// They can only be defined by class or struct types, and must be public and static.
    /// </remarks>
    public class ConversionOperatorDecl : OperatorDecl
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="ConversionOperatorDecl"/>.
        /// </summary>
        public ConversionOperatorDecl(Expression destinationType, Modifiers modifiers, CodeObject body, ParameterDecl parameter)
            : base(GetInternalName(modifiers), destinationType, modifiers, body, new[] { parameter })
        { }

        /// <summary>
        /// Create a <see cref="ConversionOperatorDecl"/>.
        /// </summary>
        public ConversionOperatorDecl(Expression destinationType, Modifiers modifiers, ParameterDecl parameter)
            : base(GetInternalName(modifiers), destinationType, modifiers, new[] { parameter })
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// True if the conversion is explicit.
        /// </summary>
        public bool IsExplicit
        {
            get { return _modifiers.HasFlag(Modifiers.Explicit); }
        }

        /// <summary>
        /// True if the conversion is implicit.
        /// </summary>
        public bool IsImplicit
        {
            get { return _modifiers.HasFlag(Modifiers.Implicit); }
        }

        #endregion

        #region /* METHODS */

        private static string GetInternalName(Modifiers modifiers)
        {
            string name = Operator.NamePrefix;
            if (modifiers.HasFlag(Modifiers.Implicit))
                name += Modifiers.Implicit.ToString();
            else if (modifiers.HasFlag(Modifiers.Explicit))
                name += Modifiers.Explicit.ToString();
            return name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public override string GetFullName(bool descriptive)
        {
            string name = (IsExplicit ? "explicit" : (IsImplicit ? "implicit" : "")) + " " + ParseToken + " " + _returnType.GetDescription();
            if (descriptive)
                name += GetParametersAsString();
            if (_parent is TypeDecl)
                name = ((TypeDecl)_parent).GetFullName(descriptive) + "." + name;
            return name;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse a <see cref="ConversionOperatorDecl"/>.
        /// </summary>
        public ConversionOperatorDecl(Parser parser, CodeObject parent, ParseFlags flags)
            : base(parser, parent, false, flags)
        {
            parser.NextToken();                                 // Move past 'operator'
            _modifiers = ModifiersHelpers.Parse(parser, this);  // Parse any modifiers in reverse from the Unused list
            _name = GetInternalName(_modifiers);                // Get the name
            ParseUnusedAnnotations(parser, this, false);        // Parse attributes and/or doc comments from the Unused list
            SetField(ref _returnType, Expression.Parse(parser, this, true, Expression.ParseTokenStartGroup), false);
            ParseParameters(parser);
            ParseTerminatorOrBody(parser, flags);
        }

        #endregion

        #region /* RENDERING */

        internal override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            _returnType.AsText(writer, passFlags);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(ParseToken + " ");
            if (flags.HasFlag(RenderFlags.Description) && _parent is TypeDecl)
            {
                ((TypeDecl)_parent).AsTextName(writer, flags);
                writer.Write(Dot.ParseToken);
            }
            AsTextName(writer, flags);
        }

        #endregion
    }
}
