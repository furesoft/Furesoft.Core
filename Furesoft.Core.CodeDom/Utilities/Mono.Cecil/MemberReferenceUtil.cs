// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Mono.Cecil;

namespace Furesoft.Core.CodeDom.Utilities.Mono.Cecil
{
    /// <summary>
    /// Static helper methods for <see cref="MemberReference"/>.
    /// </summary>
    public static class MemberReferenceUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Get the full name of the type or member, including the namespace name.
        /// </summary>
        public static string GetFullName(MemberReference memberReference)
        {
            if (memberReference is TypeDefinition)
                return memberReference.FullName;
            return memberReference.DeclaringType.FullName + "." + memberReference.Name;
        }

        /// <summary>
        /// Get the category name (field, method, etc).
        /// </summary>
        public static string GetCategory(MemberReference memberReference)
        {
            if (memberReference is TypeDefinition)
                return "type";
            if (memberReference is GenericParameter)
                return "type parameter";
            if (memberReference is MethodDefinition)
                return (((MethodDefinition)memberReference).IsConstructor ? "constructor" : "method");
            if (memberReference is PropertyDefinition)
                return (((PropertyDefinition)memberReference).HasParameters ? "indexer" : "property");
            if (memberReference is FieldDefinition)
                return "field";
            if (memberReference is EventDefinition)
                return "event";
            return "unknown";
        }

        #endregion
    }
}
