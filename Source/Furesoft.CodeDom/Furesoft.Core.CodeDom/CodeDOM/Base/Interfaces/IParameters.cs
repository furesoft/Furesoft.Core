using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;
// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;

/// <summary>
/// This interface is implemented by all code objects that have parameters
/// (<see cref="MethodDeclBase"/> [<see cref="MethodDecl"/>, <see cref="GenericMethodDecl"/>, <see cref="ConstructorDecl"/>, <see cref="DestructorDecl"/>],
/// <see cref="IndexerDecl"/>, <see cref="DelegateDecl"/>, <see cref="AnonymousMethod"/>).
/// </summary>
public interface IParameters
{
    /// <summary>
    /// True if the there are any parameters.
    /// </summary>
    bool HasParameters { get; }

    /// <summary>
    /// The number of parameters.
    /// </summary>
    int ParameterCount { get; }

    /// <summary>
    /// A collection of <see cref="ParameterDecl"/>s.
    /// </summary>
    ChildList<ParameterDecl> Parameters { get; }
}
