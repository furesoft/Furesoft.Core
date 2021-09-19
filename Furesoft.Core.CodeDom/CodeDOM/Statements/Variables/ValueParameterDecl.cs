// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Variables
{
    /// <summary>
    /// Represents the declaration of an implicit 'value' parameter for a <see cref="GetterDecl"/>, <see cref="AdderDecl"/>, or <see cref="RemoverDecl"/>.
    /// </summary>
    /// <remarks>
    /// The name of a ValueParameterDecl is hard-wired to 'value', and it can't have a modifier (ref, our, params, this) or any attributes.
    /// The type of the value parameter is always the type of the parent <see cref="PropertyDeclBase"/>.
    /// </remarks>
    public class ValueParameterDecl : ParameterDecl
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The hard-wired name of all <see cref="ValueParameterDecl"/>s.
        /// </summary>
        public const string FixedName = "value";

        #endregion /* CONSTANTS */

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a value parameter declaration.
        /// </summary>
        public ValueParameterDecl()
            : base(FixedName, null)
        {
            IsGenerated = true;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// Always 'value'.
        /// </summary>
        public override string Name
        {
            // The name of a ValueParameterDecl is fixed (always 'value')
            get { return FixedName; }
            set { throw new Exception("You can't change the Name of a ValueParameterDecl."); }
        }

        /// <summary>
        /// The type of the parent <see cref="PropertyDeclBase"/>.
        /// </summary>
        public override Expression Type
        {
            // The type of a ValueParameterDecl is always the type of the parent PropertyDeclBase (null if no parent).
            get { return (_parent != null && _parent.Parent is PropertyDeclBase ? ((PropertyDeclBase)_parent.Parent).Type : null); }
            set { throw new Exception("You can't change the Type of a ValueParameterDecl."); }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.Description))
                AsTextType(writer, flags);
            writer.Write(FixedName);
        }

        #endregion
    }
}
