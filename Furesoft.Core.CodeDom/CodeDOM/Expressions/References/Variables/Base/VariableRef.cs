// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;
using Mono.Cecil;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables.Base
{
    /// <summary>
    /// The common base class of all variable references (<see cref="FieldRef"/>, <see cref="LocalRef"/>, <see cref="ParameterRef"/>,
    /// <see cref="EnumMemberRef"/>, <see cref="PropertyRef"/>, <see cref="IndexerRef"/>, <see cref="EventRef"/>).
    /// </summary>
    public abstract class VariableRef : SymbolicRef
    {
        #region /* CONSTRUCTORS */

        protected VariableRef(IVariableDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        protected VariableRef(IMemberDefinition memberDefinition, bool isFirstOnLine)
            : base(memberDefinition, isFirstOnLine)
        { }

        protected VariableRef(MemberInfo memberInfo, bool isFirstOnLine)
            : base(memberInfo, isFirstOnLine)
        { }

        protected VariableRef(ParameterDefinition parameterDefinition, bool isFirstOnLine)
            : base(parameterDefinition, isFirstOnLine)
        { }

        protected VariableRef(ParameterInfo parameterInfo, bool isFirstOnLine)
            : base(parameterInfo, isFirstOnLine)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// True if the referenced variable is static.
        /// </summary>
        public virtual bool IsStatic
        {
            get { return false; }
        }

        #endregion

        #region /* METHODS */

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Find a type argument for the specified type parameter.
        /// </summary>
        public override TypeRefBase FindTypeArgument(TypeParameterRef typeParameterRef, CodeObject originatingChild)
        {
            TypeRefBase typeRefBase = EvaluateType();
            return (typeRefBase != null ? typeRefBase.FindTypeArgument(typeParameterRef, originatingChild) : null);
        }

        #endregion
    }
}
