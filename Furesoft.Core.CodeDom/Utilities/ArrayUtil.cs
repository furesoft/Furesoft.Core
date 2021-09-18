// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

namespace Nova.Utilities
{
    /// <summary>
    /// Extension and helper methods for <see cref="Array"/>s.
    /// </summary>
    public static class ArrayUtil
    {
        #region /* STATIC HELPER METHODS */

        // The following methods would be redundant with those in CollectionUtil:
        //public static bool IsEmpty(Array thisArray)
        //public static bool NotEmpty(Array thisArray)
        //public static int NNLength(Array thisArray)  // Same as NNCount in CollectionUtil

        /// <summary>
        /// Check if an array contains the specified item.
        /// </summary>
        /// <typeparam name="T">The type of the items in the array.</typeparam>
        /// <param name="thisArray">The array being searched.</param>
        /// <param name="item">The item being searched for.</param>
        /// <returns>True if the array contains the item, otherwise false.</returns>
        public static bool Contains<T>(T[] thisArray, T item)
        {
            foreach (T member in thisArray)
            {
                if (member.Equals(item))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
