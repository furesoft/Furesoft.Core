// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Nova.CodeDOM;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables.Base
{
    /// <summary>
    /// The common base class of all variable references (<see cref="FieldRef"/>, <see cref="LocalRef"/>, <see cref="ParameterRef"/>,
    /// <see cref="EnumMemberRef"/>, <see cref="PropertyRef"/>, <see cref="IndexerRef"/>, <see cref="EventRef"/>).
    /// </summary>
    public abstract class VariableRef : SymbolicRef
    {
        protected VariableRef(IVariableDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        protected VariableRef(MemberInfo memberInfo, bool isFirstOnLine)
            : base(memberInfo, isFirstOnLine)
        { }

        protected VariableRef(ParameterInfo parameterInfo, bool isFirstOnLine)
            : base(parameterInfo, isFirstOnLine)
        { }

        /// <summary>
        /// True if the referenced variable is static.
        /// </summary>
        public virtual bool IsStatic
        {
            get { return false; }
        }
    }
}
