using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base
{
    /// <summary>
    /// The common base class of <see cref="SetterDecl"/>, <see cref="AdderDecl"/>, and <see cref="RemoverDecl"/>.
    /// </summary>
    public abstract class AccessorDeclWithValue : AccessorDecl
    {
        #region /* CONSTRUCTORS */

        protected AccessorDeclWithValue(string namePrefix, Modifiers modifiers, CodeObject body)
            : base(namePrefix, TypeRef.VoidRef, modifiers, body)
        {
            // Add the implicit 'value' parameter - the type is always the type of the Parent (null if no Parent)
            CreateParameters().Add(new ValueParameterDecl());
        }

        protected AccessorDeclWithValue(string namePrefix, Modifiers modifiers)
            : this(namePrefix, modifiers, new Block())
        { }

        protected AccessorDeclWithValue(string namePrefix, CodeObject body)
            : this(namePrefix, Modifiers.None, body)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The 'value' parameter.
        /// </summary>
        public ParameterDecl ValueParameter
        {
            get { return _parameters.Last; }
        }

        #endregion

        #region /* PARSING */

        protected AccessorDeclWithValue(Parser parser, CodeObject parent, string namePrefix, ParseFlags flags)
            : base(parser, parent, namePrefix, flags)
        {
            // Add the implicit 'value' parameter - the type is always the type of the Parent (null if no Parent exists yet)
            CreateParameters().Add(new ValueParameterDecl());
        }

        #endregion
    }
}
