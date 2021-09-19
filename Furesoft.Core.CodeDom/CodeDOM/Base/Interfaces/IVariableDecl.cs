using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces
{
    /// <summary>
    /// This interface is implemented by all code objects that represent variable declarations, which includes
    /// <see cref="VariableDecl"/> (<see cref="ParameterDecl"/>, <see cref="LocalDecl"/>, <see cref="FieldDecl"/>, <see cref="EnumMemberDecl"/>),
    /// and <see cref="PropertyDeclBase"/> (<see cref="PropertyDecl"/>, <see cref="EventDecl"/>, <see cref="IndexerDecl"/>).
    /// </summary>
    public interface IVariableDecl : INamedCodeObject
    {
        /// <summary>
        /// The type of the variable declaration.
        /// </summary>
        Expression Type { get; set; }
    }
}
