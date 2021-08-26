// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;

namespace Furesoft.Core.CodeDom.Utilities
{
    /// <summary>
    /// Static helper methods for <see cref="PropertyInfo"/>.
    /// </summary>
    public static class PropertyInfoUtil
    {
        /// <summary>
        /// Return true if the property is abstract, otherwise false.
        /// </summary>
        public static bool IsAbstract(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);
            return ((getMethod != null && getMethod.IsAbstract) || (setMethod != null && setMethod.IsAbstract));
        }

        /// <summary>
        /// Check if the property is indexed (if it's an indexer).
        /// </summary>
        public static bool IsIndexed(PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetIndexParameters().Length > 0);
        }

        /// <summary>
        /// Return true if the property is internal, otherwise false.
        /// </summary>
        public static bool IsInternal(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);
            return (((getMethod != null && getMethod.IsAssembly) || (setMethod != null && setMethod.IsAssembly))
                && !((getMethod != null && getMethod.IsPublic) || (setMethod != null && setMethod.IsPublic)));
        }

        /// <summary>
        /// Return true if the property is private, otherwise false.
        /// </summary>
        public static bool IsPrivate(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);
            return (((getMethod != null && getMethod.IsPrivate) || (setMethod != null && setMethod.IsPrivate))
                && !(getMethod != null && (getMethod.IsPublic || getMethod.IsFamily || getMethod.IsAssembly)
                    || (setMethod != null && (setMethod.IsPublic || setMethod.IsFamily || setMethod.IsAssembly))));
        }

        /// <summary>
        /// Return true if the property is protected, otherwise false.
        /// </summary>
        public static bool IsProtected(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);
            return (((getMethod != null && getMethod.IsFamily) || (setMethod != null && setMethod.IsFamily))
                && !((getMethod != null && getMethod.IsPublic) || (setMethod != null && setMethod.IsPublic)));
        }

        /// <summary>
        /// Return true if the property is public, otherwise false.
        /// </summary>
        public static bool IsPublic(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);
            return ((getMethod != null && getMethod.IsPublic) || (setMethod != null && setMethod.IsPublic));
        }

        /// <summary>
        /// Return true if the property is static, otherwise false.
        /// </summary>
        public static bool IsStatic(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);
            return ((getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic));
        }

        /// <summary>
        /// Return true if the property is virtual, otherwise false.
        /// </summary>
        public static bool IsVirtual(PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);
            return ((getMethod != null && getMethod.IsVirtual) || (setMethod != null && setMethod.IsVirtual));
        }
    }
}