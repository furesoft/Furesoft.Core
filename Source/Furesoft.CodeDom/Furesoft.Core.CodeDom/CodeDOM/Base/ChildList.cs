﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

namespace Furesoft.Core.CodeDom.CodeDOM.Base;

/// <summary>
/// Static helper methods for ChildList - used in these cases so that the
/// collection itself can be checked for being null inside the call.
/// </summary>
public static class ChildListHelpers
{
    /// <summary>
    /// Deep-clone the collection.
    /// </summary>
    public static ChildList<T> Clone<T>(ChildList<T> thisChildList, CodeObject parent) where T : CodeObject
    {
        if (thisChildList != null)
        {
            ChildList<T> clone = new(thisChildList.Count, parent);
            foreach (T child in thisChildList)
                clone.Add(child != null ? (T)child.Clone() : null);
            return clone;
        }
        return null;
    }

    /// <summary>
    /// Return the first object in the collection, or null if the collection is null or empty.
    /// </summary>
    public static T First<T>(ChildList<T> thisChildList) where T : CodeObject
    {
        return ((thisChildList != null && thisChildList.Count > 0) ? thisChildList[0] : null);
    }
}

/// <summary>
/// Represents a collection of child <see cref="CodeObject"/>s of a particular type.
/// Sets the Parent reference of each child object added to the collection.
/// </summary>
/// <typeparam name="T">The specific type of objects in the collection (must be <see cref="CodeObject"/> or a derived type).</typeparam>
public class ChildList<T> : List<T> where T : CodeObject
{
    protected CodeObject _parent;

    /// <summary>
    /// Create a <see cref="ChildList{T}"/>, optionally using a specified <see cref="Parent"/> object.
    /// </summary>
    public ChildList(CodeObject parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Create a <see cref="ChildList{T}"/>, optionally using a specified <see cref="Parent"/> object.
    /// </summary>
    public ChildList()
    { }

    /// <summary>
    /// Create a <see cref="ChildList{T}"/> using the specified default capacity, and optional <see cref="Parent"/> object.
    /// </summary>
    public ChildList(int defaultCapacity, CodeObject parent)
        : base(defaultCapacity)
    {
        _parent = parent;
    }

    /// <summary>
    /// Create a <see cref="ChildList{T}"/> using the specified default capacity, and optional <see cref="Parent"/> object.
    /// </summary>
    public ChildList(int defaultCapacity)
        : base(defaultCapacity)
    { }

    /// <summary>
    /// Create a <see cref="ChildList{T}"/> from the specified enumerable, and optional <see cref="Parent"/> object.
    /// </summary>
    public ChildList(IEnumerable<T> enumerable, CodeObject parent)
    {
        _parent = parent;
        AddRange(enumerable);
    }

    /// <summary>
    /// Create a <see cref="ChildList{T}"/> from the specified enumerable, and optional <see cref="Parent"/> object.
    /// </summary>
    public ChildList(IEnumerable<T> enumerable)
    {
        AddRange(enumerable);
    }

    /// <summary>
    /// Determines if the code object list only requires a single line for display.
    /// </summary>
    public bool IsSingleLine
    {
        get
        {
            for (int i = 0; i < Count; ++i)
            {
                T child = this[i];
                if (child != null)
                {
                    if (i != 0 && child.IsFirstOnLine)
                        return false;
                    if (!child.IsSingleLine)
                        return false;
                }
            }
            return true;
        }
        set
        {
            for (int i = 0; i < Count; ++i)
            {
                T child = this[i];
                if (child != null)
                {
                    if (i != 0)
                        child.IsFirstOnLine = !value;
                    if (value)
                        child.IsSingleLine = true;
                }
            }
        }
    }

    /// <summary>
    /// Get the last item in the collection.
    /// </summary>
    public T Last
    {
        get { return (Count > 0 ? this[Count - 1] : null); }
    }

    /// <summary>
    /// The Parent object of the collection.
    /// </summary>
    public CodeObject Parent
    {
        get { return _parent; }
        set
        {
            _parent = value;

            // Reset all child objects to point to the new parent
            foreach (T codeObject in this)
            {
                if (codeObject != null)
                    codeObject.Parent = value;
            }
        }
    }

    /// <summary>
    /// Create a <see cref="ChildList{T}"/> with the specified number of null entries.
    /// </summary>
    public static ChildList<T> CreateListOfNulls(int nullEntryCount)
    {
        ChildList<T> list = new(nullEntryCount);
        for (int i = 0; i < nullEntryCount; ++i)
            list.Add((T)null);
        return list;
    }

    /// <summary>
    /// Add an item to the collection, setting its Parent.
    /// </summary>
    public new void Add(T item)
    {
        if (item != null)
        {
            // If any added items already have a parent, and it's not the same as the parent of this collection,
            // clone them so that we don't end up changing their Parent.  Otherwise, this could happen in many cases,
            // such as adding an evaluated TypeRef as a type argument, etc.  If code objects are moved (such as drag/drop),
            // they will be removed first, which will null the Parent reference, then added, avoiding the clone.
            if (item.Parent != null && item.Parent != _parent)
                item = (T)item.Clone();
            item.Parent = _parent;
        }
        base.Add(item);
    }

    /// <summary>
    /// Add multiple items to the collection, setting their Parents.
    /// </summary>
    public void Add(params T[] items)
    {
        foreach (T item in items)
            Add(item);
    }

    /// <summary>
    /// Add a collection of items to the collection, setting their Parents.
    /// </summary>
    public new void AddRange(IEnumerable<T> collection)
    {
        if (collection != null)
        {
            foreach (T item in collection)
                Add(item);
        }
    }

    /// <summary>
    /// Insert an item into the collection, setting its Parent.
    /// </summary>
    public new void Insert(int index, T item)
    {
        if (item != null)
        {
            // If any inserted items already have a parent, and it's not the same as the parent of this collection,
            // clone them so that we don't end up changing their Parent (see Add()).
            if (item.Parent != null && item.Parent != _parent)
                item = (T)item.Clone();
            item.Parent = _parent;
        }
        base.Insert(index, item);
    }
}
