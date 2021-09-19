// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Reflection;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Nova.CodeDOM;

namespace Furesoft.Core.CodeDom.CodeDOM.Base
{
    /// <summary>
    /// Represents a group of named code objects (<see cref="INamedCodeObject"/>s and/or <see cref="MemberInfo"/>s)
    /// with the same name, such as overloaded methods or other name collisions.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="NamedCodeObjectDictionary"/> to hold groups of child <see cref="INamedCodeObject"/>s with the same name,
    /// and also by various Get() and Find() type methods to return collections of <see cref="INamedCodeObject"/>s and/or
    /// <see cref="MemberInfo"/>s with the same name.
    /// </remarks>
    public class NamedCodeObjectGroup : INamedCodeObject, ICollection
    {
        /// <summary>
        /// The list of code objects with the same name.
        /// </summary>
        protected ArrayList _list = new ArrayList();

        /// <summary>
        /// Create an empty <see cref="NamedCodeObjectGroup"/>.
        /// </summary>
        public NamedCodeObjectGroup()
        { }

        /// <summary>
        /// Create a <see cref="NamedCodeObjectGroup"/>, adding the specified object to it.
        /// </summary>
        public NamedCodeObjectGroup(object obj)
        {
            Add(obj);
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "ambiguity"; }
        }

        /// <summary>
        /// The number of items in the group.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Get the first item in the group.
        /// </summary>
        public object First
        {
            get { return _list[0]; }
        }

        /// <summary>
        /// True if access to the <see cref="ICollection"/> is synchronized.
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// The name of the <see cref="NamedCodeObjectGroup"/>.
        /// </summary>
        public string Name
        {
            get
            {
                object obj = _list[0];
                string name;
                if (obj is INamedCodeObject)
                    name = ((INamedCodeObject)obj).Name;
                else if (obj is MemberInfo)
                    name = ((MemberInfo)obj).Name;
                else
                    name = null;
                return name;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Get the object in the group at the specified index.
        /// </summary>
        public object this[int index]
        {
            get { return _list[index]; }
        }

        /// <summary>
        /// Add a source object or group to a target object or group, converting the target into a group if necessary.
        /// </summary>
        /// <param name="target">The target object or group.</param>
        /// <param name="source">The source object or group.</param>
        public static void Add(ref object target, object source)
        {
            if (target == null)
            {
                if (source is ICollection)
                {
                    if (((ICollection)source).Count > 0)
                        target = new NamedCodeObjectGroup(source);
                }
                else
                    target = source;
            }
            else if (source != null && !(source is ICollection && ((ICollection)source).Count == 0))
            {
                if (!(target is NamedCodeObjectGroup))
                    target = new NamedCodeObjectGroup(target);
                ((NamedCodeObjectGroup)target).Add(source);
            }
        }

        /// <summary>
        /// Add a source object or group to a target object or group, converting the target into a group if necessary.
        /// </summary>
        /// <param name="target">The target object or group.</param>
        /// <param name="source">The source object or group.</param>
        public static void Add(ref INamedCodeObject target, INamedCodeObject source)
        {
            if (target == null)
            {
                if (source is NamedCodeObjectGroup)
                    target = new NamedCodeObjectGroup(source);
                else
                    target = source;
            }
            else if (source != null)
            {
                if (!(target is NamedCodeObjectGroup))
                    target = new NamedCodeObjectGroup(target);
                ((NamedCodeObjectGroup)target).Add(source);
            }
        }

        /// <summary>
        /// Add the specified <see cref="INamedCodeObject"/> to the group.
        /// </summary>
        public void Add(INamedCodeObject namedCodeObject)
        {
            _list.Add(namedCodeObject);
        }

        /// <summary>
        /// Add the specified <see cref="MemberInfo"/> to the group.
        /// </summary>
        public void Add(MemberInfo memberInfo)
        {
            _list.Add(memberInfo);
        }

        /// <summary>
        /// Add an object to the group.
        /// </summary>
        /// <param name="obj">The object being added.</param>
        public void Add(object obj)
        {
            if (obj is IEnumerable)
                AddRange((IEnumerable)obj);
            else if (obj != null)
                _list.Add(obj);
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        /// <summary>
        /// Remove all items from the group.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Check if the group contains the specified code object.
        /// </summary>
        /// <param name="codeObject">The code object being searched for.</param>
        /// <returns>True if the group contains the object, otherwise false.</returns>
        public bool Contains(CodeObject codeObject)
        {
            // NOTE: A bug in .NET causes an exception if Equals is called (which Contains uses) to
            // compare a MethodInfo for a generic method declaration to any non-MethodInfo object.
            // We currently only call this method for CodeObjects, so we look for them specifically
            // to avoid the bug.
            //return _list.Contains(obj);
            if (_list.Count > 0)
            {
                foreach (object obj in _list)
                {
                    if (obj is CodeObject && obj.Equals(codeObject))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Copy the objects in the group to the specified array, starting at the specified offset.
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

            foreach (object obj in this)
                array.SetValue(obj, index++);
        }

        /// <summary>
        /// This method is not supported for this type.
        /// </summary>
        public SymbolicRef CreateRef(bool isFirstOnLine)
        {
            throw new Exception("Can't create a reference to a NamedCodeObjectGroup!");
        }

        /// <summary>
        /// This method is not supported for this type.
        /// </summary>
        public SymbolicRef CreateRef()
        {
            return CreateRef(false);
        }

        /// <summary>
        /// This method always returns null for this type.
        /// </summary>
        public T FindParent<T>() where T : CodeObject
        {
            return null;
        }

        /// <summary>
        /// Get an enumerator for the objects in the group.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName(bool descriptive)
        {
            return Name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return Name;
        }

        /// <summary>
        /// Remove the specified object from the group.
        /// </summary>
        public void Remove(object obj)
        {
            _list.Remove(obj);
        }

        /// <summary>
        /// Remove the object at the specified index from the group.
        /// </summary>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }

        protected void AddRange(IEnumerable collection)
        {
            if (collection != null)
            {
                // Call the Add method for each member, allowing for nested
                // arrays and/or collections.
                foreach (object obj in collection)
                    Add(obj);
            }
        }
    }
}
