﻿using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps;

// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.GotoTargets.Base;

/// <summary>
/// The common base class of <see cref="LabelRef"/> and <see cref="SwitchItemRef"/>.
/// </summary>
public abstract class GotoTargetRef : SymbolicRef
{
    protected GotoTargetRef(Label declaration, bool isFirstOnLine)
        : base(declaration, isFirstOnLine)
    { }

    protected GotoTargetRef(SwitchItem declaration, bool isFirstOnLine)
        : base(declaration, isFirstOnLine)
    { }
}