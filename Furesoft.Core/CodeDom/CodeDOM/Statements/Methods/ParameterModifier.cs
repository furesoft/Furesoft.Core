// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// These modifiers are usable on method parameters to specify special behavior.
    /// Only a single modifier is allowed for each parameter.
    /// </summary>
    public enum ParameterModifier
    {
        /// <summary>The parameter has no modifiers.</summary>
        None,

        /// <summary>The parameter is passed by reference.</summary>
        Ref,

        /// <summary>The parameter is an out parameter.</summary>
        Out,

        /// <summary>The parameter can have 0 to N arguments.</summary>
        Params,

        /// <summary>Not supported yet, but included to improve parsing.</summary>
        This
    }
}