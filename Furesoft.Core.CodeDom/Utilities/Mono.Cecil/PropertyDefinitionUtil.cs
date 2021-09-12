// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Mono.Cecil;

namespace Furesoft.Core.CodeDom.Utilities.Mono.Cecil
{
    /// <summary>
    /// Static helper methods for <see cref="PropertyDefinition"/>.
    /// </summary>
    public static class PropertyDefinitionUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Return true if the property is static, otherwise false.
        /// </summary>
        public static bool IsStatic(PropertyDefinition propertyDefinition)
        {
            MethodDefinition getMethod = propertyDefinition.GetMethod;
            MethodDefinition setMethod = propertyDefinition.SetMethod;
            return ((getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic));
        }

        /// <summary>
        /// Return true if the property is public, otherwise false.
        /// </summary>
        public static bool IsPublic(PropertyDefinition propertyDefinition)
        {
            MethodDefinition getMethod = propertyDefinition.GetMethod;
            MethodDefinition setMethod = propertyDefinition.SetMethod;
            return ((getMethod != null && getMethod.IsPublic) || (setMethod != null && setMethod.IsPublic));
        }

        /// <summary>
        /// Return true if the property is private, otherwise false.
        /// </summary>
        public static bool IsPrivate(PropertyDefinition propertyDefinition)
        {
            MethodDefinition getMethod = propertyDefinition.GetMethod;
            MethodDefinition setMethod = propertyDefinition.SetMethod;
            return (((getMethod != null && getMethod.IsPrivate) || (setMethod != null && setMethod.IsPrivate))
                && !(getMethod != null && (getMethod.IsPublic || getMethod.IsFamily || getMethod.IsAssembly)
                    || (setMethod != null && (setMethod.IsPublic || setMethod.IsFamily || setMethod.IsAssembly))));
        }

        /// <summary>
        /// Return true if the property is protected, otherwise false.
        /// </summary>
        public static bool IsProtected(PropertyDefinition propertyDefinition)
        {
            MethodDefinition getMethod = propertyDefinition.GetMethod;
            MethodDefinition setMethod = propertyDefinition.SetMethod;
            return (((getMethod != null && getMethod.IsFamily) || (setMethod != null && setMethod.IsFamily))
                && !((getMethod != null && getMethod.IsPublic) || (setMethod != null && setMethod.IsPublic)));
        }

        /// <summary>
        /// Return true if the property is internal, otherwise false.
        /// </summary>
        public static bool IsInternal(PropertyDefinition propertyDefinition)
        {
            MethodDefinition getMethod = propertyDefinition.GetMethod;
            MethodDefinition setMethod = propertyDefinition.SetMethod;
            return (((getMethod != null && getMethod.IsAssembly) || (setMethod != null && setMethod.IsAssembly))
                && !((getMethod != null && getMethod.IsPublic) || (setMethod != null && setMethod.IsPublic)));
        }

        /// <summary>
        /// Return true if the property is abstract, otherwise false.
        /// </summary>
        public static bool IsAbstract(PropertyDefinition propertyDefinition)
        {
            MethodDefinition getMethod = propertyDefinition.GetMethod;
            MethodDefinition setMethod = propertyDefinition.SetMethod;
            return ((getMethod != null && getMethod.IsAbstract) || (setMethod != null && setMethod.IsAbstract));
        }

        /// <summary>
        /// Return true if the property is virtual, otherwise false.
        /// </summary>
        public static bool IsVirtual(PropertyDefinition propertyDefinition)
        {
            MethodDefinition getMethod = propertyDefinition.GetMethod;
            MethodDefinition setMethod = propertyDefinition.SetMethod;
            return ((getMethod != null && getMethod.IsVirtual) || (setMethod != null && setMethod.IsVirtual));
        }

        #endregion
    }
}
