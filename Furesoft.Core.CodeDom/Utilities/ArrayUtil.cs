// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

namespace Furesoft.Core.CodeDom.Utilities
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

        /// <summary>
        /// Add the contents of the 2nd array to the 1st one.
        /// If the 1st array is null, the 2nd array will be assigned to it.
        /// </summary>
        /// <param name="array1">The 1st array (can be null).</param>
        /// <param name="array2">The 2nd array (can be null).</param>
        /// <typeparam name="T">The element type of the arrays.</typeparam>
        public static void Add<T>(ref T[] array1, T[] array2)
        {
            if (array1 == null || array1.Length == 0)
                array1 = array2;
            else if (array2 != null && array2.Length != 0)
            {
                int oldLength = array1.Length;
                Array.Resize(ref array1, oldLength + array2.Length);
                array2.CopyTo(array1, oldLength);
            }
        }

        #endregion
    }
}
