// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Nova.CodeDOM
{
    /// <summary>
    /// This interface is implemented by all code objects that have Modifiers.
    /// </summary>
    public interface IModifiers
    {
        /// <summary>
        /// Optional <see cref="Modifiers"/> for the code object.
        /// </summary>
        Modifiers Modifiers { get; }

        /// <summary>
        /// True if the code object is static.
        /// </summary>
        bool IsStatic { get; set; }

        /// <summary>
        /// True if the code object has public access.
        /// </summary>
        bool IsPublic { get; set; }

        /// <summary>
        /// True if the code object has private access.
        /// </summary>
        bool IsPrivate { get; set; }

        /// <summary>
        /// True if the code object has protected access.
        /// </summary>
        bool IsProtected { get; set; }

        /// <summary>
        /// True if the code object has internal access.
        /// </summary>
        bool IsInternal { get; set; }

        /// <summary>
        /// Get the access rights of the code object.
        /// </summary>
        void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal);
    }
}
