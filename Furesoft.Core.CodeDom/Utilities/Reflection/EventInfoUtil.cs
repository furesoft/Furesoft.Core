// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;

namespace Nova.Utilities
{
    /// <summary>
    /// Static helper methods for <see cref="EventInfo"/>.
    /// </summary>
    public static class EventInfoUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Return true if the event is static, otherwise false.
        /// </summary>
        public static bool IsStatic(EventInfo eventInfo)
        {
            MethodInfo addMethod = eventInfo.GetAddMethod(true);
            MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
            return ((addMethod != null && addMethod.IsStatic) || (removeMethod != null && removeMethod.IsStatic));
        }

        /// <summary>
        /// Return true if the event is public, otherwise false.
        /// </summary>
        public static bool IsPublic(EventInfo eventInfo)
        {
            MethodInfo addMethod = eventInfo.GetAddMethod(true);
            MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
            return ((addMethod != null && addMethod.IsPublic) || (removeMethod != null && removeMethod.IsPublic));
        }

        /// <summary>
        /// Return true if the event is private, otherwise false.
        /// </summary>
        public static bool IsPrivate(EventInfo eventInfo)
        {
            MethodInfo addMethod = eventInfo.GetAddMethod(true);
            MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
            return (((addMethod != null && addMethod.IsPrivate) || (removeMethod != null && removeMethod.IsPrivate))
                && !(addMethod != null && (addMethod.IsPublic || addMethod.IsFamily || addMethod.IsAssembly)
                    || (removeMethod != null && (removeMethod.IsPublic || removeMethod.IsFamily || removeMethod.IsAssembly))));
        }

        /// <summary>
        /// Return true if the event is protected, otherwise false.
        /// </summary>
        public static bool IsProtected(EventInfo eventInfo)
        {
            MethodInfo addMethod = eventInfo.GetAddMethod(true);
            MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
            return (((addMethod != null && addMethod.IsFamily) || (removeMethod != null && removeMethod.IsFamily))
                && !((addMethod != null && addMethod.IsPublic) || (removeMethod != null && removeMethod.IsPublic)));
        }

        /// <summary>
        /// Return true if the event is abstract, otherwise false.
        /// </summary>
        public static bool IsAbstract(EventInfo eventInfo)
        {
            MethodInfo addMethod = eventInfo.GetAddMethod(true);
            MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
            return ((addMethod != null && addMethod.IsAbstract) || (removeMethod != null && removeMethod.IsAbstract));
        }

        /// <summary>
        /// Return true if the event is virtual, otherwise false.
        /// </summary>
        public static bool IsVirtual(EventInfo eventInfo)
        {
            MethodInfo addMethod = eventInfo.GetAddMethod(true);
            MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
            return ((addMethod != null && addMethod.IsVirtual) || (removeMethod != null && removeMethod.IsVirtual));
        }

        #endregion
    }
}
