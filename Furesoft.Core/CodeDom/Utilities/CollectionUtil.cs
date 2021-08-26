// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections;
using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.Utilities
{
    /// <summary>
    /// Static helper methods for Collections.
    /// </summary>
    public static class CollectionUtil
    {
        /// <summary>
        /// Compare the sizes and members of this List with another List.
        /// </summary>
        /// <param name="thisList">The collection to be compared.</param>
        /// <param name="thatList">The collection to compare it to.</param>
        /// <typeparam name="T">The generic type parameter of the Lists being compared.</typeparam>
        /// <returns>True if the collections are equal, otherwise false.</returns>
        public static bool CompareList<T>(List<T> thisList, List<T> thatList) where T : struct
        {
            if (thisList.Count != thatList.Count)
                return false;
            for (var i = 0; i < thisList.Count; ++i)
            {
                if (!thisList[i].Equals(thatList[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the specified collection is empty or null.
        /// </summary>
        /// <param name="thisCollection">The collection to check.</param>
        /// <returns>True if collection is empty or null.</returns>
        public static bool IsEmpty(ICollection thisCollection)
        {
            return (thisCollection == null || thisCollection.Count == 0);
        }

        /// <summary>
        /// Get the count of the specified collection, or 0 if it's null.
        /// </summary>
        /// <param name="thisCollection">The collection to check.</param>
        /// <returns>Collection count or 0 if null.</returns>
        public static int NNCount(ICollection thisCollection)
        {
            return (thisCollection != null ? thisCollection.Count : 0);
        }

        /// <summary>
        /// Check if the specified collection is empty or null.
        /// </summary>
        /// <param name="thisCollection">The collection to check.</param>
        /// <returns>True if collection is empty or null.</returns>
        public static bool NotEmpty(ICollection thisCollection)
        {
            return (thisCollection != null && thisCollection.Count > 0);
        }
    }
}