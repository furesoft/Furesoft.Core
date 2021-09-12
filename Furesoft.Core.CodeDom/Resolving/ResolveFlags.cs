// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

namespace Nova.Resolving
{
    /// <summary>
    /// Symbol resolution flags (passed through Resolve methods).
    /// </summary>
    [Flags]
    public enum ResolveFlags
    {
        /// <summary>No flags set.</summary>
        None             = 0x0000,
        /// <summary>Quiet operation - no warnings or errors on failure to resolve.</summary>
        Quiet            = 0x0001,
        /// <summary>Code is embedded inside documentation comments - only warnings should be logged, no errors.</summary>
        InDocComment     = 0x0002,
        /// <summary>Code is inside a 'cref' in a documentation commment - references to non-static members are OK.</summary>
        InDocCodeRef     = 0x0004,
        /// <summary>Skip resolving of method bodies (use for performance if you don't care about method bodies).</summary>
        SkipMethodBodies = 0x0008,
        /// <summary>First resolve phase - stop at type declarations after resolving any base list.</summary>
        Phase1           = 0x0010,
        /// <summary>Second resolve phase - stop at the bodies of methods/properties, or base/this initializers, or field initializers.</summary>
        Phase2           = 0x0020,
        /// <summary>Third resolve phase - resolve method and property bodies, base/this initializers, field initializers.</summary>
        Phase3           = 0x0040,
        /// <summary>True if we're resolving references in a generated CodeUnit (code cleanup settings will be ignored).</summary>
        IsGenerated      = 0x0080,
        /// <summary>Unresolve all resolved SymbolicRefs, and don't resolve any UnresolvedRefs.</summary>
        Unresolve        = 0x0100
    }
}
