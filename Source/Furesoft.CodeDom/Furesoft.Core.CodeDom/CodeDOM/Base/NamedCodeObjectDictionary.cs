// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps;

namespace Furesoft.Core.CodeDom.CodeDOM.Base
{
    /// <summary>
    /// Represents a dictionary of named code objects, allowing multiple entries with the same name.
    /// </summary>
    /// <remarks>
    /// This dictionary is used by <see cref="Block"/>s to maintain a dictionary of child <see cref="INamedCodeObject"/>s to allow for
    /// lookups by name.  It handles multiple entries with the same name (such as overloaded methods, different type parameter counts,
    /// or name collisions in invalid code) by storing a <see cref="NamedCodeObjectGroup"/> object.
    /// </remarks>
    public class NamedCodeObjectDictionary : ICollection
    {
        protected Dictionary<string, INamedCodeObject> _dictionary = new();

        /// <summary>
        /// The number of items in the dictionary.
        /// </summary>
        public int Count
        {
            get { return _dictionary.Values.Count; }
        }

        /// <summary>
        /// True if access to the <see cref="ICollection"/> is synchronized.
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Add a code object with the specified name to the dictionary.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="namedCodeObject">The named code object.</param>
        public void Add(string name, INamedCodeObject namedCodeObject)
        {
            // Protect against null names - shouldn't occur, but might in rare cases for code with parsing errors
            if (name != null)
            {
                if (_dictionary.TryGetValue(name, out INamedCodeObject existingObj))
                {
                    // If we had a name collision, add to any existing group, or create a new one
                    if (existingObj is NamedCodeObjectGroup)
                        ((NamedCodeObjectGroup)existingObj).Add(namedCodeObject);
                    else
                    {
                        _dictionary.Remove(name);
                        _dictionary.Add(name, new NamedCodeObjectGroup { existingObj, namedCodeObject });
                    }
                }
                else
                    _dictionary.Add(name, namedCodeObject);
            }
        }

        /// <summary>
        /// Clear all members from the dictionary.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// Copy the objects in the dictionary to the specified array, starting at the specified offset.
        /// </summary>
        /// <param name="array">The array to copy into.</param>
        /// <param name="index">The starting index in the array.</param>
        public virtual void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array", "Null array reference");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Index is out of range");
            if (array.Rank > 1)
                throw new ArgumentException("Array is multi-dimensional", "array");

            foreach (INamedCodeObject obj in this)
                array.SetValue(obj, index++);
        }

        /// <summary>
        /// Find named code object(s) in the dictionary by name.
        /// </summary>
        /// <returns>The matching <see cref="CodeObject"/>, <see cref="NamedCodeObjectGroup"/>, or null if not found.</returns>
        public INamedCodeObject Find(string name)
        {
            if (name == null)
                return null;
            _dictionary.TryGetValue(name, out INamedCodeObject found);
            return found;
        }

        /// <summary>
        /// Find a named goto target in the dictionary by name.
        /// </summary>
        /// <returns>The matching <see cref="Label"/> or <see cref="SwitchItem"/>, or null if not found
        /// (or <see cref="NamedCodeObjectGroup"/> if there are multiple matches).</returns>
        public INamedCodeObject FindGotoTarget(string name)
        {
            _dictionary.TryGetValue(Label.ParseToken + name, out INamedCodeObject found);
            return found;
        }

        /// <summary>
        /// Get an enumerator for the objects in the dictionary.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            foreach (INamedCodeObject namedCodeObject in _dictionary.Values)
                yield return namedCodeObject;
        }

        /// <summary>
        /// Remove the object with the specified name from the dictionary.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="namedCodeObject">The named code object.</param>
        public void Remove(string name, INamedCodeObject namedCodeObject)
        {
            if (_dictionary.TryGetValue(name, out INamedCodeObject existingObj))
            {
                // If there's a group with the given name, remove the object from the group
                if (existingObj is NamedCodeObjectGroup group)
                {
                    group.Remove(namedCodeObject);
                    if (group.Count == 1)
                    {
                        // If only one object is left in the group, replace the group with the object
                        _dictionary.Remove(name);
                        _dictionary.Add(name, namedCodeObject);
                    }
                }
                else
                    _dictionary.Remove(name);
            }
        }
    }
}
