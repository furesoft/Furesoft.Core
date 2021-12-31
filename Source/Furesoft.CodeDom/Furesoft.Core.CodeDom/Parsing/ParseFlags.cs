// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

namespace Furesoft.Core.CodeDom.Parsing
{
    /// <summary>
    /// Parsing state flags (passed through parsing methods).
    /// </summary>
    [Flags]
    public enum ParseFlags
    {
        /// <summary>No flags set.</summary>
        None             = 0x00,
        /// <summary>Parsing an expression.</summary>
        Expression       = 0x01,
        /// <summary>Parsing a type reference.</summary>
        Type             = 0x02,
        /// <summary>Parsing something that's NOT a type.</summary>
        NotAType         = 0x04,
        /// <summary>Parsing a block.</summary>
        Block            = 0x08,
        /// <summary>Parsing the arguments of an ArgumentsOperator.</summary>
        Arguments        = 0x10,
        /// <summary>Suppress parsing of arrays, specifically for built-in types.</summary>
        NoArrays         = 0x20,
        /// <summary>Skip parsing of method bodies (use for performance if you don't care about method bodies).</summary>
        SkipMethodBodies = 0x40
    }
}
