// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using System;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Annotation formatting flags.
    /// </summary>
    [Flags]
    public enum AnnotationFlags
    {
        /// <summary>No flags.</summary>
        None = 0x00,

        /// <summary>Inline status bit field mask - these bits are used to indicate different types of "infix" comments.</summary>
        InfixMask = 0x03,

        /// <summary>Indicates Infix location #1 - used for inline comments that appear in an otherwise-empty parameter or argument list, or the initialization
        /// section of a 'for' statement.  Also used for EOL comments that appear after the Block start symbol ('{') or an operator, or compiler directives that
        /// occur between a TypeDecl header and any base type list or type constraints.</summary>
        IsInfix1 = 0x01,

        /// <summary>Indicates Infix location #2 - used for inline comments that appear in an otherwise-empty conditional section of a 'for' statement, or
        /// compiler directives that occur between a ConstructorDecl header and any constructor initializer.</summary>
        IsInfix2 = 0x02,

        /// <summary>Indicates Infix location #3 - used for inline comments that appear in an otherwise-empty iteration section of a 'for' statement.</summary>
        IsInfix3 = 0x03,

        /// <summary>Indicates Postfix location - used for regular comments and compiler directives that appear after an object instead of before it.</summary>
        IsPostfix = 0x04,

        /// <summary>Used for EOL comments to indicate no space after the comment symbol.</summary>
        NoSpace = 0x08,

        /// <summary>Used by DocComments objects to represent a missing start tag.</summary>
        NoStartTag = 0x10,

        /// <summary>Used by DocComments objects to represent a missing end tag.</summary>
        NoEndTag = 0x20
    }

    /// <summary>
    /// The common base class of all code annotations, including user comments (derived from <see cref="CommentBase"/>),
    /// code attributes (<see cref="Attribute"/>), compiler directives (derived from <see cref="CompilerDirective"/>), and generated
    /// messages (derived from <see cref="Message"/>).
    /// </summary>
    public abstract class Annotation : CodeObject
    {
        protected AnnotationFlags _annotationFlags;

        protected Annotation()
        { }

        protected Annotation(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// Annotation formatting flags.
        /// </summary>
        public AnnotationFlags AnnotationFlags
        {
            get { return _annotationFlags; }
            protected internal set { _annotationFlags = value; }
        }

        /// <summary>
        /// True if the annotation appears at the end-of-line (should only be true for EOL comments).
        /// </summary>
        public virtual bool IsEOL
        {
            get { return false; }
            set { throw new Exception("Can't set IsEOL on this type!"); }
        }

        /// <summary>
        /// True if this annotation is marked as any type of Infix location.
        /// </summary>
        public bool IsInfix
        {
            get { return (_annotationFlags & AnnotationFlags.InfixMask) != 0; }
            set { SetAnnotationFlag(AnnotationFlags.IsInfix1, value); }
        }

        /// <summary>
        /// True if this annotation is marked as Infix location #1.
        /// </summary>
        public bool IsInfixLocation1
        {
            get { return _annotationFlags.HasFlag(AnnotationFlags.IsInfix1); }
            set { SetAnnotationFlag(AnnotationFlags.IsInfix1, value); }
        }

        /// <summary>
        /// True if this annotation is marked as Infix location #1.
        /// </summary>
        public bool IsInfixLocation2
        {
            get { return _annotationFlags.HasFlag(AnnotationFlags.IsInfix2); }
            set { SetAnnotationFlag(AnnotationFlags.IsInfix2, value); }
        }

        /// <summary>
        /// True if this annotation is marked as Infix location #1.
        /// </summary>
        public bool IsInfixLocation3
        {
            get { return _annotationFlags.HasFlag(AnnotationFlags.IsInfix3); }
            set { SetAnnotationFlag(AnnotationFlags.IsInfix3, value); }
        }

        /// <summary>
        /// True if the annotation should be listed at the <see cref="CodeUnit"/> level (for display in an output window).
        /// </summary>
        public virtual bool IsListed
        {
            get { return false; }
        }

        /// <summary>
        /// True if this annotation appears after the object it's attached to as opposed to before it.
        /// </summary>
        public bool IsPostfix
        {
            get { return _annotationFlags.HasFlag(AnnotationFlags.IsPostfix); }
            set { SetAnnotationFlag(AnnotationFlags.IsPostfix, value); }
        }

        /// <summary>
        /// Get the annotation in text format.
        /// </summary>
        public virtual string Text
        {
            get { return AsString(); }
            set { throw new Exception("Can't set Text on this type!"); }
        }

        protected internal void SetAnnotationFlag(AnnotationFlags flag, bool value)
        {
            if (value)
                _annotationFlags |= flag;
            else
                _annotationFlags &= ~flag;
        }
    }
}