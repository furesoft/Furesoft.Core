// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Variables;

/// <summary>
/// Represents the declaration of the single valid member of an <see cref="EnumDecl"/>,
/// which has one or more <see cref="EnumMemberDecl"/> children.  It has no name (its children have names),
/// and its type is the base type of the parent <see cref="EnumDecl"/>, or it defaults to int.
/// MultiEnumMemberDecls are implicitly constants (and therefore also static).
/// </summary>
/// <remarks>
/// The Type of a MultiEnumMemberDecl will always match those of all of the EnumMemberDecls that
/// it contains, and this Type is also always the base type of the parent EnumDecl.  The Name,
/// Initialization, and Attributes of a MultiEnumMemberDecl are always null.  Each EnumMemberDecl
/// child object can have its own distinct Attributes (unlike with MultiFieldDecl).
/// </remarks>
public class MultiEnumMemberDecl : EnumMemberDecl, IMultiVariableDecl
{
    #region /* FIELDS */

    protected ChildList<EnumMemberDecl> _enumMemberDecls;

    #endregion

    #region /* CONSTRUCTORS */

    /// <summary>
    /// Create a multi enum member declaration.
    /// </summary>
    public MultiEnumMemberDecl()
        : base(null)
    {
        _enumMemberDecls = new ChildList<EnumMemberDecl>(this);
        NewLines = 0;
    }

    /// <summary>
    /// Create a multi enum member declaration from an array of EnumMemberDecls.
    /// </summary>
    /// <param name="enumMemberDecls">An array of EnumMemberDecls.</param>
    public MultiEnumMemberDecl(params EnumMemberDecl[] enumMemberDecls)
        : this()
    {
        if (enumMemberDecls.Length > 0)
        {
            // Acquire the Parent from the first EnumMemberDecl
            EnumMemberDecl enumMemberDecl = enumMemberDecls[0];
            Parent = enumMemberDecl.Parent;
            enumMemberDecl.Parent = null;  // Break parent link to avoid clone on Add
            AddRange(enumMemberDecls);
        }
    }

    /// <summary>
    /// Create a multi enum member declaration from an array of names.
    /// </summary>
    /// <param name="names">An array of nanes.</param>
    public MultiEnumMemberDecl(params string[] names)
        : this()
    {
        foreach (string name in names)
            Add(new EnumMemberDecl(name));
    }

    #endregion

    #region /* PROPERTIES */

    /// <summary>
    /// The list of <see cref="EnumMemberDecl"/>s.
    /// </summary>
    public ChildList<EnumMemberDecl> MemberDecls
    {
        get { return _enumMemberDecls; }
    }

    /// <summary>
    /// Enumerate the child <see cref="FieldDecl"/>s.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<EnumMemberDecl> GetEnumerator()
    {
        return ((IEnumerable<EnumMemberDecl>)_enumMemberDecls).GetEnumerator();
    }

    /// <summary>
    /// Enumerate the child <see cref="FieldDecl"/>s.
    /// </summary>
    /// <returns></returns>
    IEnumerator<VariableDecl> IEnumerable<VariableDecl>.GetEnumerator()
    {
        return ((IEnumerable<EnumMemberDecl>)_enumMemberDecls).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// The number of <see cref="EnumMemberDecl"/>s.
    /// </summary>
    public int Count
    {
        get { return _enumMemberDecls.Count; }
    }

    /// <summary>
    /// Get the <see cref="EnumMemberDecl"/> at the specified index.
    /// </summary>
    public EnumMemberDecl this[int index]
    {
        get { return _enumMemberDecls[index]; }
    }

    /// <summary>
    /// Get the <see cref="VariableDecl"/> at the specified index.
    /// </summary>
    VariableDecl IMultiVariableDecl.this[int index]
    {
        get { return _enumMemberDecls[index]; }
    }

    /// <summary>
    /// The type of the parent <see cref="EnumDecl"/>.
    /// </summary>
    public override Expression Type
    {
        get { return (_parent is EnumDecl ? new TypeRef((EnumDecl)_parent) : null); }
        set { throw new Exception("Can't change the Type of a MultiEnumMemberDecl - it's always the parent EnumDecl."); }
    }

    /// <summary>
    /// Get the underlying type of the enum.
    /// </summary>
    public Expression UnderlyingType
    {
        get { return (_parent is EnumDecl ? ((EnumDecl)_parent).UnderlyingType : null); }
    }

    /// <summary>
    /// Get the parent <see cref="EnumDecl"/>.
    /// </summary>
    public override EnumDecl ParentEnumDecl
    {
        get
        {
            // Our parent should be an EnumDecl
            return (_parent is EnumDecl ? _parent as EnumDecl : null);
        }
    }

    /// <summary>
    /// The line number associated with the <see cref="MultiEnumMemberDecl"/> (actually, the first child <see cref="EnumMemberDecl"/>).
    /// </summary>
    public override int LineNumber
    {
        get { return _enumMemberDecls[0].LineNumber; }
    }

    /// <summary>
    /// The column number associated with the <see cref="MultiEnumMemberDecl"/> (actually, the first child <see cref="EnumMemberDecl"/>).
    /// </summary>
    public override int ColumnNumber
    {
        get { return _enumMemberDecls[0].ColumnNumber; }
    }

    #endregion

    #region /* METHODS */

    /// <summary>
    /// Add an <see cref="EnumMemberDecl"/>.
    /// </summary>
    public void Add(EnumMemberDecl enumMemberDecl)
    {
        // Force enum values on separate lines by default if the parent block is on a separate line
        if (!enumMemberDecl.IsNewLinesSet && !enumMemberDecl.IsFirstOnLine && _parent is BlockStatement && ((BlockStatement)_parent).Body.IsFirstOnLine)
            enumMemberDecl.SetNewLines(1);

        // If we're adding an EnumMemberDecl on a line by itself, and the main decl isn't multi-line, then
        // we need to re-format after adding.
        bool reformatAsMultiLine = (!IsFirstOnLine && enumMemberDecl.IsFirstOnLine);
        _enumMemberDecls.Add(enumMemberDecl);
        if (reformatAsMultiLine && _parent is BlockStatement)
            ((BlockStatement)_parent).ReformatBlock();
    }

    /// <summary>
    /// Add an <see cref="EnumMemberDecl"/> with the specified name.
    /// </summary>
    public void Add(string name, Expression initialization)
    {
        Add(new EnumMemberDecl(name, initialization));
    }

    /// <summary>
    /// Add an <see cref="EnumMemberDecl"/> with the specified name.
    /// </summary>
    public void Add(string name)
    {
        Add(new EnumMemberDecl(name));
    }

    /// <summary>
    /// Add multiple <see cref="EnumMemberDecl"/>s.
    /// </summary>
    public void Add(params EnumMemberDecl[] enumMemberDecls)
    {
        AddRange(enumMemberDecls);
    }

    /// <summary>
    /// Add a collection of <see cref="EnumMemberDecl"/>s.
    /// </summary>
    public void AddRange(IEnumerable<EnumMemberDecl> collection)
    {
        foreach (EnumMemberDecl enumMemberDecl in collection)
            Add(enumMemberDecl);
    }

    /// <summary>
    /// Attach an <see cref="Annotation"/> (<see cref="Comment"/>, <see cref="DocComment"/>, <see cref="Attribute"/>, <see cref="CompilerDirective"/>, or <see cref="Message"/>) to the <see cref="CodeObject"/>.
    /// </summary>
    /// <param name="annotation">The <see cref="Annotation"/>.</param>
    /// <param name="atFront">Inserts at the front if true, otherwise adds at the end.</param>
    public override void AttachAnnotation(Annotation annotation, bool atFront)
    {
        // Any annotations added to a MultiEnumMemberDecl should be transferred to its first member instead
        // (otherwise they won't be visible, since a MultiEnumMemberDecl itself isn't displayed - just its members).
        if (_enumMemberDecls.Count > 0)
            _enumMemberDecls[0].AttachAnnotation(annotation, atFront);
        else
            throw new Exception("Can't attach annotations to an empty MultiEnumMemberDecl!");
    }

    /// <summary>
    /// Add the <see cref="CodeObject"/> to the specified dictionary.
    /// </summary>
    public override void AddToDictionary(NamedCodeObjectDictionary dictionary)
    {
        // For MultiEnumMemberDecls, we need to add each individual EnumMemberDecl
        foreach (EnumMemberDecl enumMemberDecl in _enumMemberDecls)
            dictionary.Add(enumMemberDecl.Name, enumMemberDecl);
    }

    /// <summary>
    /// Remove the <see cref="CodeObject"/> from the specified dictionary.
    /// </summary>
    public override void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
    {
        // For MultiEnumMemberDecls, we need to remove each individual EnumMemberDecl
        foreach (EnumMemberDecl enumMemberDecl in _enumMemberDecls)
            dictionary.Remove(enumMemberDecl.Name, enumMemberDecl);
    }

    /// <summary>
    /// Deep-clone the code object.
    /// </summary>
    public override CodeObject Clone()
    {
        MultiEnumMemberDecl clone = (MultiEnumMemberDecl)base.Clone();
        clone._enumMemberDecls = ChildListHelpers.Clone(_enumMemberDecls, clone);
        return clone;
    }

    /// <summary>
    /// Get the enum member with the specified name.
    /// </summary>
    public EnumMemberRef GetMember(string name)
    {
        EnumMemberDecl enumMemberDecl = _enumMemberDecls.Find(delegate(EnumMemberDecl e) { return e.Name == name; });
        return (enumMemberDecl != null ? (EnumMemberRef)enumMemberDecl.CreateRef() : null);
    }

    /// <summary>
    /// Get the full name of the <see cref="EnumMemberDecl"/>, including the namespace name.
    /// </summary>
    public override string GetFullName(bool descriptive)
    {
        return null;
    }

    #endregion

    #region /* PARSING */

    // Parsing of a MultiEnumMemberDecl is handled by EnumMemberDecl.
    // We can't just have EnumDecl do a forced parse of a single MultiEnumMemberDecl, because there
    // are things handled by the Block parser that we need, plus we need to handle stand-alone comments
    // and we'd like to keep open the possibility of additional enum members in the future.

    #endregion

    #region /* FORMATTING */

    /// <summary>
    /// True if the code object only requires a single line for display by default.
    /// </summary>
    public override bool IsSingleLineDefault
    {
        get { return Enumerable.All(_enumMemberDecls, delegate(EnumMemberDecl enumMemberDecl) { return enumMemberDecl.IsSingleLineDefault; }); }
    }

    /// <summary>
    /// Determines if the code object only requires a single line for display.
    /// </summary>
    public override bool IsSingleLine
    {
        get { return (base.IsSingleLine && (_enumMemberDecls.Count == 0 || (!_enumMemberDecls[0].IsFirstOnLine && _enumMemberDecls.IsSingleLine))); }
        set
        {
            base.IsSingleLine = value;
            if (_enumMemberDecls.Count > 0)
            {
                if (value)
                    _enumMemberDecls[0].IsFirstOnLine = false;
                _enumMemberDecls.IsSingleLine = value;
            }
        }
    }

    /// <summary>
    /// True if the code object defaults to starting on a new line.
    /// </summary>
    public override bool IsFirstOnLineDefault
    {
        get { return (_enumMemberDecls != null && !_enumMemberDecls.IsSingleLine); }
    }

    /// <summary>
    /// The number of newlines preceeding the object (0 to N).
    /// </summary>
    public override int NewLines
    {
        get
        {
            // This object is invisible, and doesn't have newlines itself, so use
            // the NewLines value from the first EnumMemberDecl.
            return (_enumMemberDecls.Count > 0 ? _enumMemberDecls[0].NewLines : 0);
        }
        set
        {
            // This object is invisible, and doesn't have newlines itself, but changes
            // to the NewLines value are propagated to all EnumMemberDecls.
            if (_enumMemberDecls.Count > 0)
                _enumMemberDecls[0].NewLines = value;
            _enumMemberDecls.IsSingleLine = (value == 0);
        }
    }

    /// <summary>
    /// True if the <see cref="Statement"/> has a terminator character by default.
    /// </summary>
    public override bool HasTerminatorDefault
    {
        get { return false; }
    }

    #endregion

    #region /* RENDERING */

    public override void AsText(CodeWriter writer, RenderFlags flags)
    {
        // Check for alignment of enum constant values and/or EOL comments (ignore if there aren't multiple
        // items or if the 2nd item isn't on a new line).
        bool isBitFlag = IsBitFlag;
        int nameColumnWidth = 0;
        int initializationWidth = 0;
        int totalWidth = 0;
        if (_enumMemberDecls.Count > 1 && _enumMemberDecls[1].IsFirstOnLine)
        {
            foreach (EnumMemberDecl enumMemberDecl in _enumMemberDecls)
            {
                // Calculate alignment of enum names and any initializations
                if (enumMemberDecl.Name.Length > nameColumnWidth)
                    nameColumnWidth = enumMemberDecl.Name.Length;
                if (enumMemberDecl.Initialization != null)
                {
                    int length = enumMemberDecl.Initialization.AsTextLength();
                    if (length > initializationWidth)
                        initializationWidth = length;
                }
            }
            totalWidth = nameColumnWidth + (initializationWidth > 0 ? initializationWidth + 3 : 0);
        }

        // If we're aligning bit-flag enum constants, create an alignment state to hold the alignment offset
        bool aligningConstants = (isBitFlag && nameColumnWidth > 0);
        if (aligningConstants)
            writer.BeginAlignment(this, new[] { nameColumnWidth });

        // Pass any total width through to WriteList to align any EOL comments
        int[] columnWidths = (totalWidth > 0 ? new[] { totalWidth } : null);
        writer.WriteList(_enumMemberDecls, flags | RenderFlags.NoIncreaseIndent, this, columnWidths);

        if (aligningConstants)
            writer.EndAlignment(this);
    }

    public override void AsTextType(CodeWriter writer, RenderFlags flags)
    { }

    #endregion
}
