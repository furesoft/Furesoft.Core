// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Declares a property-like event, with add/remove accessors.
    /// </summary>
    /// <remarks>
    /// An EventDecl is like a PropertyDecl except it has an 'event' modifier, and add/remove
    /// accesors instead of get/set.
    /// Simple field-like events can also be created using FieldDecl with an 'event' modifier.
    /// </remarks>
    public class EventDecl : PropertyDeclBase
    {
        /// <summary>
        /// Create an <see cref="EventDecl"/> with the specified name, type, and modifiers.
        /// </summary>
        public EventDecl(string name, Expression type, Modifiers modifiers)
            : base(name, type, modifiers | Modifiers.Event, null)
        { }

        /// <summary>
        /// Create an <see cref="EventDecl"/> with the specified name and type.
        /// </summary>
        public EventDecl(string name, Expression type)
            : base(name, type, Modifiers.Event, null)
        { }

        /// <summary>
        /// Create an <see cref="EventDecl"/> with the specified name and type.
        /// </summary>
        public EventDecl(Expression name, Expression type)
            : base(name, type, Modifiers.Event)
        { }

        /// <summary>
        /// The 'adder' method for the event.
        /// </summary>
        public AdderDecl Adder
        {
            get { return _body.FindFirst<AdderDecl>(); }
            set
            {
                if (_body != null)
                {
                    AdderDecl existing = _body.FindFirst<AdderDecl>();
                    if (existing != null)
                        _body.Remove(existing);
                }
                Insert(0, value);  // Always put the 'adder' first
            }
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "event"; }
        }

        /// <summary>
        /// True if the event has an adder method.
        /// </summary>
        public bool HasAdder
        {
            get { return (_body.FindFirst<AdderDecl>() != null); }
        }

        /// <summary>
        /// True if the event has a remover method.
        /// </summary>
        public bool HasRemover
        {
            get { return (_body.FindFirst<RemoverDecl>() != null); }
        }

        /// <summary>
        /// True if the event is readable.
        /// </summary>
        public override bool IsReadable { get { return HasAdder; } }

        /// <summary>
        /// True if the event is writable.
        /// </summary>
        public override bool IsWritable { get { return HasRemover; } }

        /// <summary>
        /// The 'remover' method for the event.
        /// </summary>
        public RemoverDecl Remover
        {
            get { return _body.FindFirst<RemoverDecl>(); }
            set
            {
                if (_body != null)
                {
                    RemoverDecl existing = _body.FindFirst<RemoverDecl>();
                    if (existing != null)
                        _body.Remove(existing);
                }
                Insert((_body != null ? _body.Count : 0), value);  // Always put the 'remover' after any 'adder'
            }
        }

        /// <summary>
        /// Create a reference to the <see cref="EventDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>An <see cref="EventRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new EventRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Get the IsPrivate access right for the specified usage, and if not private then also get the IsProtected and IsInternal rights.
        /// </summary>
        /// <param name="isTargetOfAssignment">Usage - true if the target of an assignment ('lvalue'), otherwise false.</param>
        /// <param name="isPrivate">True if the access is private.</param>
        /// <param name="isProtected">True if the access is protected.</param>
        /// <param name="isInternal">True if the access is internal.</param>
        public override void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal)
        {
            isPrivate = isProtected = isInternal = false;

            // The access rights of an event actually depend on the rights of the corresponding
            // adder/remover, depending upon whether we're assigning to it or not.
            if (isTargetOfAssignment)
            {
                AdderDecl adderDecl = Adder;
                if (adderDecl != null)
                {
                    isPrivate = adderDecl.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = adderDecl.IsProtected;
                        isInternal = adderDecl.IsInternal;
                    }
                }
            }
            else
            {
                RemoverDecl removerDecl = Remover;
                if (removerDecl != null)
                {
                    isPrivate = removerDecl.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = removerDecl.IsProtected;
                        isInternal = removerDecl.IsInternal;
                    }
                }
            }
        }

        /// <summary>
        /// Parse an <see cref="EventDecl"/>.
        /// </summary>
        public EventDecl(Parser parser, CodeObject parent)
            : base(parser, parent, true)
        {
            _modifiers |= Modifiers.Event;
        }
    }
}