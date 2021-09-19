// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furesoft.Core.CodeDom.Utilities.Reflection;

namespace Furesoft.Core.CodeDom.Utilities.Reflection
{
    /// <summary>
    /// Static helper methods for <see cref="MemberInfo"/>.
    /// </summary>
    public static class MemberInfoUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Determine if the MemberInfo has a custom attribute.
        /// </summary>
        /// <remarks>
        /// This method must be used instead of the built-in GetCustomAttributes() call when working with
        /// members of reflection-only assemblies (otherwise, the custom attribute type would be instantiated,
        /// which is illegal).
        /// </remarks>
        public static bool HasCustomAttribute(MemberInfo thisMemberInfo, string name)
        {
            return Enumerable.Any(CustomAttributeData.GetCustomAttributes(thisMemberInfo), delegate(CustomAttributeData customAttribute) { return customAttribute.Constructor.DeclaringType != null && customAttribute.Constructor.DeclaringType.Name == name; });
        }

        /// <summary>
        /// Get all custom attributes with the specified name from the MemberInfo.
        /// </summary>
        /// <remarks>
        /// This method must be used instead of the built-in GetCustomAttributes() call when working with
        /// reflection-only assemblies (otherwise, the custom attribute type would be instantiated, which is illegal).
        /// </remarks>
        public static List<CustomAttributeData> GetCustomAttributes(MemberInfo thisMemberInfo, string name)
        {
            return Enumerable.ToList(Enumerable.Where(CustomAttributeData.GetCustomAttributes(thisMemberInfo),
                delegate(CustomAttributeData customAttribute) { return customAttribute.Constructor.DeclaringType != null && customAttribute.Constructor.DeclaringType.Name == name; }));
        }

        /// <summary>
        /// Get the custom attribute with the specified name from the MemberInfo.  If there are multiple attributes with the name, the first one is returned.
        /// </summary>
        /// <remarks>
        /// This method must be used instead of the built-in GetCustomAttributes() call when working with
        /// reflection-only assemblies (otherwise, the custom attribute type would be instantiated, which is illegal).
        /// </remarks>
        public static CustomAttributeData GetCustomAttribute(MemberInfo thisMemberInfo, string name)
        {
            return Enumerable.FirstOrDefault(CustomAttributeData.GetCustomAttributes(thisMemberInfo),
                delegate(CustomAttributeData customAttribute) { return customAttribute.Constructor.DeclaringType != null && customAttribute.Constructor.DeclaringType.Name == name; });
        }

        /// <summary>
        /// Get the full name of the type or member, including the namespace name (unlike the FullName property, never returns null).
        /// </summary>
        public static string GetFullName(MemberInfo thisMemberInfo)
        {
            if (thisMemberInfo is Type)
            {
                // The FullName property on Type can return null under certain circumstances, so build the name from the Namespace
                // and Name in such a case (but use FullName if possible, because it handles nested types properly).
                Type thisType = (Type)thisMemberInfo;
                return (thisType.FullName ?? thisType.Namespace + "." + thisType.Name);
            }
            return (thisMemberInfo.DeclaringType != null ? GetFullName(thisMemberInfo.DeclaringType) + "." : "") + thisMemberInfo.Name;
        }

        /// <summary>
        /// Get the category name (field, method, etc).
        /// </summary>
        public static string GetCategory(MemberInfo thisMemberInfo)
        {
            switch (thisMemberInfo.MemberType)
            {
                case MemberTypes.Field:       return "field";
                case MemberTypes.Property:    return (PropertyInfoUtil.IsIndexed((PropertyInfo)thisMemberInfo) ? "indexer" : "property");
                case MemberTypes.Method:      return "method";
                case MemberTypes.Constructor: return "constructor";
                case MemberTypes.Event:       return "event";
                default:                      return "type";
            }
        }

        #endregion
    }
}
