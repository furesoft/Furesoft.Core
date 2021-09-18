// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a compound local variable declaration - the declaration of multiple local variables of
    /// the same type in a single statement.
    /// </summary>
    /// <remarks>
    /// The Modifiers and Type of a MultiLocalDecl will always match those of all of the <see cref="LocalDecl"/>s
    /// that it contains, while its Name and Initialization members will always be null.  Any changes
    /// to the Modifiers or Type of the MultiLocalDecl are propagated to all of its LocalDecl children,
    /// and changes to these properties directly on the children are not allowed (exceptions are thrown).
    /// </remarks>
    public class MultiLocalDecl : LocalDecl, IMultiVariableDecl
    {
        #region /* FIELDS */

        protected ChildList<LocalDecl> _localDecls;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a multi local variable declaration from an array of LocalDecls.
        /// </summary>
        /// <param name="localDecls">An array of LocalDecls.</param>
        public MultiLocalDecl(params LocalDecl[] localDecls)
            : base(null, null)
        {
            _localDecls = new ChildList<LocalDecl>(this);
            if (localDecls.Length > 0)
            {
                // Acquire the Parent, Type, and Modifiers from the first LocalDecl
                LocalDecl localDecl1 = localDecls[0];
                Parent = localDecl1.Parent;
                localDecl1.Parent = null;  // Break parent link to avoid clone on Add
                SetField(ref _type, (Expression)localDecl1.Type.Clone(), true);
                _modifiers = localDecl1.Modifiers;

                // Add with internal method to avoid changes to Type, etc.
                foreach (LocalDecl localDecl in localDecls)
                    AddInternal(localDecl);
            }
        }

        /// <summary>
        /// Create a multi field declaration.
        /// </summary>
        /// <param name="type">The type of the multi field declaration.</param>
        /// <param name="names">The list of names to be used.</param>
        public MultiLocalDecl(Expression type, params string[] names)
            : base(null, type)
        {
            _localDecls = new ChildList<LocalDecl>(this);
            foreach (string name in names)
                Add(new LocalDecl(name, (Expression)type.Clone()));
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The list of <see cref="LocalDecl"/>s.
        /// </summary>
        public ChildList<LocalDecl> LocalDecls
        {
            get { return _localDecls; }
        }

        /// <summary>
        /// Enumerate the child <see cref="FieldDecl"/>s.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<LocalDecl> GetEnumerator()
        {
            return ((IEnumerable<LocalDecl>)_localDecls).GetEnumerator();
        }

        /// <summary>
        /// Enumerate the child <see cref="FieldDecl"/>s.
        /// </summary>
        /// <returns></returns>
        IEnumerator<VariableDecl> IEnumerable<VariableDecl>.GetEnumerator()
        {
            return ((IEnumerable<LocalDecl>)_localDecls).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// The number of <see cref="LocalDecl"/>s.
        /// </summary>
        public int Count
        {
            get { return _localDecls.Count; }
        }

        /// <summary>
        /// Get the <see cref="LocalDecl"/> at the specified index.
        /// </summary>
        public LocalDecl this[int index]
        {
            get { return _localDecls[index]; }
        }

        /// <summary>
        /// Get the <see cref="VariableDecl"/> at the specified index.
        /// </summary>
        VariableDecl IMultiVariableDecl.this[int index]
        {
            get { return _localDecls[index]; }
        }

        /// <summary>
        /// Optional <see cref="Modifiers"/>.
        /// </summary>
        public override Modifiers Modifiers
        {
            set
            {
                _modifiers = value;

                // Propagate the change to all children
                foreach (LocalDecl localDecl in _localDecls)
                    localDecl.Modifiers = _modifiers;
            }
        }

        /// <summary>
        /// The type of the <see cref="MultiLocalDecl"/>.
        /// </summary>
        public override Expression Type
        {
            set
            {
                SetField(ref _type, value, true);

                // Propagate the change to all children
                foreach (LocalDecl localDecl in _localDecls)
                    localDecl.SetTypeFromParentMulti((Expression)_type.Clone());
            }
        }

        /// <summary>
        /// The line number associated with the <see cref="MultiLocalDecl"/> (actually, the first child <see cref="LocalDecl"/>).
        /// </summary>
        public override int LineNumber
        {
            get { return _localDecls[0].LineNumber; }
        }

        /// <summary>
        /// The column number associated with the <see cref="MultiLocalDecl"/> (actually, the first child <see cref="LocalDecl"/>).
        /// </summary>
        public override int ColumnNumber
        {
            get { return _localDecls[0].ColumnNumber; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Add a <see cref="LocalDecl"/>.
        /// </summary>
        public void Add(LocalDecl localDecl)
        {
            // Set the Type and Modifiers to those of the parent multi
            localDecl.Type = (Type != null ? (Expression)Type.Clone() : null);
            localDecl.Modifiers = _modifiers;
            AddInternal(localDecl);
        }

        protected void AddInternal(LocalDecl localDecl)
        {
            // Override the default newlines to 0 if it hasn't been explicitly set
            if (!localDecl.IsNewLinesSet)
                localDecl.SetNewLines(0);

            _localDecls.Add(localDecl);
        }

        /// <summary>
        /// Add a new <see cref="LocalDecl"/> with the specified name.
        /// </summary>
        public void Add(string name, Expression initialization)
        {
            AddInternal(new LocalDecl(name, Type != null ? (Expression)Type.Clone() : null, Modifiers, initialization));
        }

        /// <summary>
        /// Add a new <see cref="LocalDecl"/> with the specified name.
        /// </summary>
        public void Add(string name)
        {
            AddInternal(new LocalDecl(name, Type != null ? (Expression)Type.Clone() : null, Modifiers, null));
        }

        /// <summary>
        /// Add multiple <see cref="LocalDecl"/>s.
        /// </summary>
        public void Add(params LocalDecl[] localDecls)
        {
            AddRange(localDecls);
        }

        /// <summary>
        /// Add a collection of <see cref="LocalDecl"/>s.
        /// </summary>
        public void AddRange(IEnumerable<LocalDecl> collection)
        {
            foreach (LocalDecl localDecl in collection)
                Add(localDecl);
        }

        /// <summary>
        /// This method isn't supported for this type.
        /// </summary>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            throw new Exception("CreateRef() isn't supported for MultiLocalDecls!");
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public override void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            // For MultiLocalDecls, we need to add each individual LocalDecl
            foreach (LocalDecl localDecl in _localDecls)
                dictionary.Add(localDecl.Name, localDecl);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public override void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            // For MultiLocalDecls, we need to remove each individual LocalDecl
            foreach (LocalDecl localDecl in _localDecls)
                dictionary.Remove(localDecl.Name, localDecl);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            MultiLocalDecl clone = (MultiLocalDecl)base.Clone();
            clone._localDecls = ChildListHelpers.Clone(_localDecls, clone);
            return clone;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get { return Enumerable.All(_localDecls, delegate(LocalDecl localDecl) { return localDecl.IsSingleLineDefault; }); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_localDecls.Count == 0 || (!_localDecls[0].IsFirstOnLine && _localDecls.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_localDecls.Count > 0)
                {
                    if (value)
                        _localDecls[0].IsFirstOnLine = false;
                    _localDecls.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextPrefix(CodeWriter writer, RenderFlags flags)
        {
            ModifiersHelpers.AsText(_modifiers, writer);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextType(writer, flags);
            UpdateLineCol(writer, flags);
            writer.WriteList(_localDecls, passFlags | RenderFlags.HasTerminator, this);
        }

        protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            // The terminator will be rendered by ChildList above, so don't do it here
        }

        #endregion
    }
}
