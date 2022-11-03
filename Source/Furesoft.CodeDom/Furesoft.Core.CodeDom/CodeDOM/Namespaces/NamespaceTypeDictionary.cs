﻿using System;
using System.Collections;
using System.Collections.Generic;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Namespaces;

/// <summary>
/// Represents a dictionary of <see cref="Namespace"/>s and types (<see cref="TypeDecl"/>s and/or <see cref="Type"/>s),
/// allowing multiple entries with the same name.
/// </summary>
/// <remarks>
/// This dictionary is used by <see cref="Namespace"/> to store child <see cref="Namespace"/>, <see cref="TypeDecl"/>,
/// and <see cref="Type"/> objects (storing items of type 'object').  It handles multiple entries with
/// the same name (for generic types with different type parameter counts) by storing a <see cref="NamespaceTypeGroup"/> object.
/// </remarks>
public class NamespaceTypeDictionary : ICollection
{
    protected Dictionary<string, object> _dictionary = new();

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
    /// Add a child <see cref="Namespace"/> to the dictionary.
    /// </summary>
    public void Add(Namespace @namespace)
    {
        Add(@namespace.Name, @namespace);
    }

    /// <summary>
    /// Add a <see cref="TypeDecl"/> to the dictionary.
    /// </summary>
    public void Add(TypeDecl typeDecl)
    {
        Add(typeDecl.Name, typeDecl);
    }

    /// <summary>
    /// Add a <see cref="Type"/> to the dictionary.
    /// </summary>
    public void Add(Type type)
    {
        Add(type.IsGenericType ? TypeUtil.NonGenericName(type) : type.Name, type);
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

        foreach (object obj in this)
            array.SetValue(obj, index++);
    }

    /// <summary>
    /// Find named code object(s) in the dictionary by name.
    /// </summary>
    /// <returns>The matching <see cref="TypeDecl"/>, <see cref="Type"/>, <see cref="Namespace"/>,
    /// <see cref="NamespaceTypeGroup"/>, or null if not found.</returns>
    public object Find(string name)
    {
        if (name == null)
            return null;
        _dictionary.TryGetValue(name, out object found);
        if (found == null)
        {
            // If we didn't find a match, check for a trailing generic parameter count
            int index = name.LastIndexOf('`');
            if (index > 0)
            {
                // Try again without the trailing count
                _dictionary.TryGetValue(name.Substring(0, index), out found);
                if (found is NamespaceTypeGroup)
                {
                    // If we found multiple matches, then return any exact match if found
                    foreach (object @object in (NamespaceTypeGroup)found)
                    {
                        if (@object is Type)
                        {
                            if (((Type)@object).Name == name)
                                return @object;
                        }
                    }
                }
            }
        }
        return found;
    }

    /// <summary>
    /// Get an enumerator for the objects in the dictionary.
    /// </summary>
    public IEnumerator GetEnumerator()
    {
        foreach (object obj in _dictionary.Values)
            yield return obj;
    }

    /// <summary>
    /// Remove the specified child <see cref="Namespace"/> from the dictionary.
    /// </summary>
    public void Remove(Namespace @namespace)
    {
        Remove(@namespace.Name, @namespace);
    }

    /// <summary>
    /// Remove the specified <see cref="TypeDecl"/> from the dictionary.
    /// </summary>
    public void Remove(TypeDecl typeDecl)
    {
        Remove(typeDecl.Name, typeDecl);
    }

    /// <summary>
    /// Remove the specified <see cref="Type"/> from the dictionary.
    /// </summary>
    public void Remove(Type type)
    {
        Remove(type.IsGenericType ? TypeUtil.NonGenericName(type) : type.Name, type);
    }

    /// <summary>
    /// Add the specified type or namespace object with the specified name to the dictionary.
    /// </summary>
    /// <param name="name">The name of the object.</param>
    /// <param name="obj">The <see cref="TypeDecl"/>, <see cref="Type"/>, or <see cref="Namespace"/> object.</param>
    protected void Add(string name, object obj)
    {
        // Protect against null names - shouldn't occur, but might in rare cases for code with parsing errors
        if (name != null)
        {
            if (_dictionary.TryGetValue(name, out object existingObj))
            {
                // If we had a name collision, add to any existing group, or create a new one
                if (existingObj is NamespaceTypeGroup)
                    ((NamespaceTypeGroup)existingObj).Add(obj);
                else
                {
                    _dictionary.Remove(name);
                    _dictionary.Add(name, new NamespaceTypeGroup { existingObj, obj });
                }
            }
            else
                _dictionary.Add(name, obj);
        }
    }

    /// <summary>
    /// Remove the type or namespace object with the specified name from the dictionary.
    /// </summary>
    /// <param name="name">The name of the object.</param>
    /// <param name="obj">The <see cref="TypeDecl"/>, <see cref="Type"/>, or <see cref="Namespace"/> object.</param>
    protected void Remove(string name, object obj)
    {
        if (_dictionary.TryGetValue(name, out object existingObj))
        {
            // If there's a group with the given name, remove the object from the group
            if (existingObj is NamespaceTypeGroup group)
            {
                group.Remove(obj);
                if (group.Count == 1)
                {
                    // If only one object is left in the group, replace the group with the object
                    _dictionary.Remove(name);
                    _dictionary.Add(name, obj);
                }
            }
            else
                _dictionary.Remove(name);
        }
    }
}
