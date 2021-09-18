// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Reflection;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a group of types (<see cref="TypeDecl"/>s and/or <see cref="Type"/>s)
    /// and/or <see cref="Namespace"/>s, with the same name (the contained objects are of type 'object').
    /// </summary>
    /// <remarks>
    /// In valid code, a <see cref="NamespaceTypeGroup"/> should contain only <see cref="TypeDecl"/>s and/or <see cref="Type"/>s
    /// with the same name (the types being a combination of a non-generic type and/or generic types with the same name but different
    /// numbers of type parameters).  The two types of objects can be mixed due to external types existing in the same namespace as types
    /// declared in local code.
    /// </remarks>
    public class NamespaceTypeGroup : INamedCodeObject, ICollection
    {
        #region /* FIELDS */

        /// <summary>
        /// The list of type objects with the same name.
        /// </summary>
        protected ArrayList _list = new ArrayList();

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an empty <see cref="NamespaceTypeGroup"/>.
        /// </summary>
        public NamespaceTypeGroup()
        { }

        /// <summary>
        /// Create a <see cref="NamespaceTypeGroup"/>, adding the specified object to it.
        /// </summary>
        public NamespaceTypeGroup(object obj)
        {
            Add(obj);
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The name of the <see cref="NamespaceTypeGroup"/>.
        /// </summary>
        public string Name
        {
            get
            {
                object obj;
                lock (this)
                    obj = _list[0];
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
            get
            {
                int count;
                lock (this)
                    count = _list.Count;
                return count;
            }
        }

        /// <summary>
        /// Get the first item in the group.
        /// </summary>
        public object First
        {
            get
            {
                object item;
                lock (this)
                    item = _list[0];
                return item;
            }
        }

        /// <summary>
        /// True if access to the <see cref="ICollection"/> is synchronized.
        /// </summary>
        public bool IsSynchronized
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </summary>
        public object SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Add the specified <see cref="Namespace"/> to the group.
        /// </summary>
        public void Add(Namespace @namespace)
        {
            Add((object)@namespace);
        }

        /// <summary>
        /// Add the specified <see cref="TypeDecl"/> to the group.
        /// </summary>
        public void Add(TypeDecl typeDecl)
        {
            Add((object)typeDecl);
        }

        /// <summary>
        /// Add the specified <see cref="Type"/> to the group.
        /// </summary>
        public void Add(Type type)
        {
            Add((object)type);
        }

        /// <summary>
        /// Add an object to the group.
        /// </summary>
        /// <param name="obj">The <see cref="TypeDecl"/>, <see cref="Type"/>, or <see cref="Namespace"/> object being added.</param>
        public void Add(object obj)
        {
            if (obj is IEnumerable)
                AddRange((IEnumerable)obj);
            else if (obj != null)
            {
                lock (this)
                    _list.Add(obj);
            }
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

        /// <summary>
        /// This method is not supported for this type.
        /// </summary>
        public SymbolicRef CreateRef(bool isFirstOnLine)
        {
            throw new Exception("Can't create a reference to a NamespaceTypeGroup!");
        }

        /// <summary>
        /// This method is not supported for this type.
        /// </summary>
        public SymbolicRef CreateRef()
        {
            return CreateRef(false);
        }

        /// <summary>
        /// Remove all items from the group.
        /// </summary>
        public void Clear()
        {
            lock (this)
                _list.Clear();
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

            lock (this)
            {
                foreach (object obj in this)
                    array.SetValue(obj, index++);
            }
        }

        /// <summary>
        /// Check if the group contains the specified code object.
        /// </summary>
        /// <param name="obj">The code object being searched for.</param>
        /// <returns>True if the group contains the object, otherwise false.</returns>
        public bool Contains(object obj)
        {
            bool contains;
            lock (this)
                contains = _list.Contains(obj);
            return contains;
        }

        /// <summary>
        /// Get an enumerator for the objects in the group.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Remove the specified object from the group.
        /// </summary>
        public void Remove(object obj)
        {
            lock (this)
                _list.Remove(obj);
        }

        /// <summary>
        /// Remove the object at the specified index from the group.
        /// </summary>
        public void RemoveAt(int index)
        {
            lock (this)
                _list.RemoveAt(index);
        }

        /// <summary>
        /// Get the object in the group at the specified index.
        /// </summary>
        public object this[int index]
        {
            get
            {
                object obj;
                lock (this)
                    obj = _list[index];
                return obj;
            }
        }

        /// <summary>
        /// This method shouldn't be called on this type.
        /// </summary>
        public void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            throw new Exception("Can't add a NamespaceTypeGroup to a NamedCodeObjectDictionary!");
        }

        /// <summary>
        /// This method shouldn't be called on this type.
        /// </summary>
        public void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            throw new Exception("Can't remove a NamespaceTypeGroup from a NamedCodeObjectDictionary!");
        }

        /// <summary>
        /// This method always returns null for this type.
        /// </summary>
        public T FindParent<T>() where T : CodeObject
        {
            return null;
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

        #endregion

        #region /* STATIC METHODS */

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
                        target = new NamespaceTypeGroup(source);
                }
                else
                    target = source;
            }
            else if (source != null && !(source is ICollection && ((ICollection)source).Count == 0))
            {
                if (!(target is NamespaceTypeGroup))
                    target = new NamespaceTypeGroup(target);
                ((NamespaceTypeGroup)target).Add(source);
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
                if (source is NamespaceTypeGroup)
                    target = new NamespaceTypeGroup(source);
                else
                    target = source;
            }
            else if (source != null)
            {
                if (!(target is NamespaceTypeGroup))
                    target = new NamespaceTypeGroup(target);
                ((NamespaceTypeGroup)target).Add(source);
            }
        }

        #endregion
    }
}
