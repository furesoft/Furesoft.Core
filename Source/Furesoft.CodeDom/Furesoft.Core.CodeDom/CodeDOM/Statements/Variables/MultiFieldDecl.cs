// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Variables
{
    /// <summary>
    /// Represents a compound field declaration - the declaration of multiple fields of the same type in
    /// a single statement.
    /// </summary>
    /// <remarks>
    /// The Modifiers and Type of a MultiFieldDecl will always match those of all of the <see cref="FieldDecl"/>s
    /// that it contains, while its Name and Initialization members will always be null.  Any changes
    /// to the Modifiers or Type of the MultiFieldDecl are propagated to all of its FieldDecl children,
    /// and changes to these properties directly on the children are not allowed (exceptions are thrown).
    /// </remarks>
    public class MultiFieldDecl : FieldDecl, IMultiVariableDecl
    {
        #region /* FIELDS */

        protected ChildList<FieldDecl> _fieldDecls;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a multi field declaration from an array of FieldDecls.
        /// </summary>
        /// <param name="fieldDecls">An array of FieldDecls.</param>
        public MultiFieldDecl(params FieldDecl[] fieldDecls)
            : base(null, null)
        {
            _fieldDecls = new ChildList<FieldDecl>(this);
            if (fieldDecls.Length > 0)
            {
                // Acquire the Parent, Type, and Modifiers from the first FieldDecl
                FieldDecl fieldDecl1 = fieldDecls[0];
                Parent = fieldDecl1.Parent;
                fieldDecl1.Parent = null;  // Break parent link to avoid clone on Add
                SetField(ref _type, (Expression)fieldDecl1.Type.Clone(), true);
                _modifiers = fieldDecl1.Modifiers;

                // Also move any annotations from the first FieldDecl
                if (fieldDecl1.HasAnnotations)
                {
                    Annotations = fieldDecl1.Annotations;
                    fieldDecl1.Annotations = null;
                }

                // Add with internal method to avoid changes to Type, etc.
                foreach (FieldDecl fieldDecl in fieldDecls)
                    AddInternal(fieldDecl);
            }
        }

        /// <summary>
        /// Create a multi field declaration.
        /// </summary>
        /// <param name="type">The type of the multi field declaration.</param>
        /// <param name="modifiers">The modifiers of the multi field declaration.</param>
        /// <param name="names">The list of names to be used.</param>
        public MultiFieldDecl(Expression type, Modifiers modifiers, params string[] names)
            : base(null, type, modifiers)
        {
            _fieldDecls = new ChildList<FieldDecl>(this);
            foreach (string name in names)
                Add(new FieldDecl(name, (Expression)type.Clone(), modifiers));
        }

        /// <summary>
        /// Create a multi field declaration.
        /// </summary>
        /// <param name="type">The type of the multi field declaration.</param>
        /// <param name="names">The list of names to be used.</param>
        public MultiFieldDecl(Expression type, params string[] names)
            : this(type, Modifiers.None, names)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The list of <see cref="FieldDecl"/>s.
        /// </summary>
        public ChildList<FieldDecl> FieldDecls
        {
            get { return _fieldDecls; }
        }

        /// <summary>
        /// Enumerate the child <see cref="FieldDecl"/>s.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<FieldDecl> GetEnumerator()
        {
            return ((IEnumerable<FieldDecl>)_fieldDecls).GetEnumerator();
        }

        /// <summary>
        /// Enumerate the child <see cref="FieldDecl"/>s.
        /// </summary>
        /// <returns></returns>
        IEnumerator<VariableDecl> IEnumerable<VariableDecl>.GetEnumerator()
        {
            return ((IEnumerable<FieldDecl>)_fieldDecls).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// The number of <see cref="FieldDecl"/>s.
        /// </summary>
        public int Count
        {
            get { return _fieldDecls.Count; }
        }

        /// <summary>
        /// Get the <see cref="FieldDecl"/> at the specified index.
        /// </summary>
        public FieldDecl this[int index]
        {
            get { return _fieldDecls[index]; }
        }

        /// <summary>
        /// Get the <see cref="VariableDecl"/> at the specified index.
        /// </summary>
        VariableDecl IMultiVariableDecl.this[int index]
        {
            get { return _fieldDecls[index]; }
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
                foreach (FieldDecl fieldDecl in _fieldDecls)
                    fieldDecl.Modifiers = _modifiers;
            }
        }

        /// <summary>
        /// The type of the <see cref="MultiFieldDecl"/>.
        /// </summary>
        public override Expression Type
        {
            set
            {
                SetField(ref _type, value, true);

                // Propagate the change to all children
                foreach (FieldDecl fieldDecl in _fieldDecls)
                    fieldDecl.SetTypeFromParentMulti((Expression)_type.Clone());
            }
        }

        /// <summary>
        /// The line number associated with the <see cref="MultiFieldDecl"/> (actually, the first child <see cref="FieldDecl"/>).
        /// </summary>
        public override int LineNumber
        {
            get { return _fieldDecls[0].LineNumber; }
        }

        /// <summary>
        /// The column number associated with the <see cref="MultiFieldDecl"/> (actually, the first child <see cref="FieldDecl"/>).
        /// </summary>
        public override int ColumnNumber
        {
            get { return _fieldDecls[0].ColumnNumber; }
        }

        #endregion

        #region /* METHODS */
        
        /// <summary>
        /// Add a <see cref="FieldDecl"/>.
        /// </summary>
        public void Add(FieldDecl fieldDecl)
        {
            // Set the Type and Modifiers to those of the parent multi
            fieldDecl.Type = (Type != null ? (Expression)Type.Clone() : null);
            fieldDecl.Modifiers = Modifiers;

            AddInternal(fieldDecl);
        }

        protected void AddInternal(FieldDecl fieldDecl)
        {
            // Override the default newlines to 0 if it hasn't been explicitly set
            if (!fieldDecl.IsNewLinesSet)
                fieldDecl.SetNewLines(0);

            _fieldDecls.Add(fieldDecl);
        }

        /// <summary>
        /// Add a <see cref="FieldDecl"/> with the specified name.
        /// </summary>
        public void Add(string name, Expression initialization)
        {
            AddInternal(new FieldDecl(name, Type != null ? (Expression)Type.Clone() : null, Modifiers, initialization));
        }

        /// <summary>
        /// Add a <see cref="FieldDecl"/> with the specified name.
        /// </summary>
        public void Add(string name)
        {
            AddInternal(new FieldDecl(name, Type != null ? (Expression)Type.Clone() : null, Modifiers));
        }

        /// <summary>
        /// Add one more more <see cref="FieldDecl"/>s.
        /// </summary>
        public void Add(params FieldDecl[] fieldDecls)
        {
            AddRange(fieldDecls);
        }

        /// <summary>
        /// Add a collection of <see cref="FieldDecl"/>s.
        /// </summary>
        public void AddRange(IEnumerable<FieldDecl> collection)
        {
            foreach (FieldDecl fieldDecl in collection)
                Add(fieldDecl);
        }

        /// <summary>
        /// This method isn't supported for this type.
        /// </summary>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            throw new Exception("CreateRef() isn't supported for MultiFieldDecls!");
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public override void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            // For MultiFieldDecls, we need to add each individual FieldDecl
            foreach (FieldDecl fieldDecl in _fieldDecls)
                dictionary.Add(fieldDecl.Name, fieldDecl);
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public override void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            // For MultiFieldDecls, we need to remove each individual FieldDecl
            foreach (FieldDecl fieldDecl in _fieldDecls)
                dictionary.Remove(fieldDecl.Name, fieldDecl);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            MultiFieldDecl clone = (MultiFieldDecl)base.Clone();
            clone._fieldDecls = ChildListHelpers.Clone(_fieldDecls, clone);
            return clone;
        }

        /// <summary>
        /// Get the full name of the <see cref="FieldDecl"/>, including the namespace name.
        /// </summary>
        public override string GetFullName(bool descriptive)
        {
            return null;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get { return Enumerable.All(_fieldDecls, delegate(FieldDecl fieldDecl) { return fieldDecl.IsSingleLineDefault; }); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_fieldDecls.Count == 0 || (!_fieldDecls[0].IsFirstOnLine && _fieldDecls.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_fieldDecls.Count > 0)
                {
                    if (value)
                        _fieldDecls[0].IsFirstOnLine = false;
                    _fieldDecls.IsSingleLine = value;
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
            writer.WriteList(_fieldDecls, passFlags | RenderFlags.HasTerminator, this);
        }

        protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            // The terminator will be rendered by ChildList above, so don't do it here
        }

        #endregion
    }
}
