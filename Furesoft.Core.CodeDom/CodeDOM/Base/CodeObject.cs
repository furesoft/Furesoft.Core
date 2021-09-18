// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all code objects.
    /// </summary>
    public abstract class CodeObject : ICloneable, IDisposable
    {
        /// <summary>
        /// The tab size used for indentation of code.
        /// </summary>
        public static int TabSize = 4;

        /// <summary>
        /// Automatically detect and preserve tabs used for code indentation.
        /// </summary>
        public static bool AutoDetectTabs = true;

        /// <summary>
        /// Use tabs instead of spaces for indentation (ignored if AutoDetectTabs is true).
        /// </summary>
        public static bool UseTabs;

        /// <summary>
        /// The maximum line length used for automatic formatting, such as code alignment and line wrapping.
        /// </summary>
        public static int MaximumLineLength = 128;

        /// <summary>
        /// Determines whether or not formatting cleanup is automatically performed during the parsing process.
        /// </summary>
        public static bool AutomaticFormattingCleanup;

        /// <summary>
        /// Determines whether or not code cleanup is automatically performed during the parsing process.
        /// </summary>
        public static bool AutomaticCodeCleanup;

        /// <summary>
        /// The parent <see cref="CodeObject"/>.
        /// </summary>
        protected CodeObject _parent;

        /// <summary>
        /// Any <see cref="Annotation"/>s (<see cref="Comment"/>s, <see cref="DocComment"/>s, <see cref="Attribute"/>s,
        /// or <see cref="Message"/>s) associated with the <see cref="CodeObject"/> (null if none).
        /// </summary>
        /// <remarks>
        /// Annotations are generally supported on all code objects, although <see cref="Attribute"/>s are only legal on certain objects,
        /// and comments on <see cref="Expression"/>s that aren't rendered as <see cref="Statement"/>s (i.e. sub-expressions) will be rendered
        /// as inline comments if not EOL (and in some cases might only be visible in the GUI as pop-ups).
        /// </remarks>
        protected ChildList<Annotation> _annotations;

        /// <summary>
        /// The starting line number associated with the <see cref="CodeObject"/>.
        /// </summary>
        protected int _lineNumber;

        /// <summary>
        /// The starting column number associated with the <see cref="CodeObject"/>.
        /// </summary>
        protected ushort _columnNumber;

        /// <summary>
        /// Formatting flags - for line feeds, braces, etc.
        /// </summary>
        protected FormatFlags _formatFlags;

        protected CodeObject()
        {
            if (IsFirstOnLineDefault)
                SetNewLines(1);
        }

        /// <summary>
        /// Create a code object from an existing one, copying members.
        /// </summary>
        protected CodeObject(CodeObject codeObject)
        {
            Parent = codeObject.Parent;
            Annotations = codeObject.Annotations;
            codeObject.Annotations = null;
            CopyFormatting(codeObject);
        }

        static CodeObject()
        {
            // Override any default static field values with any specified in the config file
            Configuration.LoadSettings();

            // Initialize static TypeRefs to defaults
            TypeRef.InitializeTypeRefs();

            // Initialize all parse-points for all CodeDOM objects
            //LoadDefaultParsePoints();
        }

        public static void LoadDefaultParsePoints()
        {
            // Force calls to all static AddParsePoints methods on all types derived from CodeObject, so that
            // all parse-points will be registered with the parser before it tries to parse something.
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(CodeObject)))
                {
                    MethodInfo method = type.GetMethod("AddParsePoints", BindingFlags.NonPublic | BindingFlags.Static);
                    if (method != null)
                        method.Invoke(null, null);
                }
            }
        }

        /// <summary>
        /// Call this method to force a reference to <see cref="CodeObject"/>, so that all static members are
        /// initialized, and settings are read from the config file.  Call this before changing any static
        /// settings manually, or your changes will be overwritten when the config file is processed.
        /// </summary>
        public static void ForceReference()
        { }

        /// <summary>
        /// The parent <see cref="CodeObject"/>.
        /// </summary>
        public virtual CodeObject Parent
        {
            get { return _parent; }
            set
            {
                // Do nothing unless the new value is different from the current one
                if (_parent != value)
                {
                    // If the parent is already set, remove any listed annotations first
                    if (_annotations != null && _parent != null)
                    {
                        foreach (Annotation annotation in _annotations)
                        {
                            if (annotation.IsListed)
                                NotifyListedAnnotationRemoved(annotation);
                        }
                    }

                    _parent = value;

                    // If the new value is non-null, propagate any listed annotations
                    if (_annotations != null && value != null)
                    {
                        foreach (Annotation annotation in _annotations)
                        {
                            if (annotation.IsListed)
                                NotifyListedAnnotationAdded(annotation);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The line number associated with the <see cref="CodeObject"/> (if any, 0 if none).
        /// </summary>
        /// <remarks>
        /// The line number will match the input file when the object is parsed, but may differ if the code tree is modified.
        /// </remarks>
        public virtual int LineNumber
        {
            get { return _lineNumber; }
        }

        /// <summary>
        /// The column number associated with the <see cref="CodeObject"/> (if any, 0 if none).
        /// </summary>
        /// <remarks>
        /// The column will match the input file when the object is parsed, but may differ if the code tree is modified.
        /// </remarks>
        public virtual int ColumnNumber
        {
            get { return _columnNumber; }
        }

        /// <summary>
        /// Any hidden reference to another <see cref="CodeObject"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="NewObject"/> or <see cref="ConstructorInitializer"/> will have a hidden <see cref="ConstructorRef"/>,
        /// indexers can have a hidden <see cref="IndexerRef"/>, overloaded operators will have a hidden <see cref="OperatorRef"/>,
        /// and a 'goto case {constant}' will have a hidden <see cref="GotoTargetRef"/>.
        /// </remarks>
        public virtual SymbolicRef HiddenRef
        {
            get { return null; }
        }

        /// <summary>
        /// Set a field of a code object, including setting the parent, and optional formatting.
        /// </summary>
        protected void SetField<T>(ref T field, T value, bool format) where T : CodeObject
        {
            if (field != null)
                field.Dispose();
            field = value;
            if (value != null)
            {
                field.Parent = this;
                if (format)
                    DefaultFormatField(field);
            }
        }

        /// <summary>
        /// Set a field of a code object, including setting the parent, and optional formatting.
        /// </summary>
        protected void SetField(ref object field, object value, bool format)
        {
            if (field is CodeObject)
                ((CodeObject)field).Dispose();
            field = value;
            if (value != null)
            {
                if (field is CodeObject)
                {
                    CodeObject codeObject = (CodeObject)field;
                    codeObject.Parent = this;
                    if (format)
                        DefaultFormatField(codeObject);
                }
            }
        }

        /// <summary>
        /// Set a ChildList collection field of a code object, including setting the parent.
        /// </summary>
        protected void SetField<T>(ref ChildList<T> collectionField, ChildList<T> newCollection) where T : CodeObject
        {
            // Set the parent of any existing collection to null
            if (collectionField != null)
                collectionField.Parent = null;

            collectionField = newCollection;

            // Set the parent of the new collection to the current object
            if (newCollection != null)
                newCollection.Parent = this;
        }

        /// <summary>
        /// Clone a field of a code object, including setting the parent.
        /// </summary>
        protected void CloneField<T>(ref T field, T value) where T : CodeObject
        {
            if (value != null)
            {
                field = (T)value.Clone();
                field.Parent = this;
            }
        }

        /// <summary>
        /// Clone a field of a code object, including setting the parent.
        /// </summary>
        protected void CloneField(ref object field, object value)
        {
            if (field is CodeObject && value != null)
            {
                field = ((CodeObject)value).Clone();
                ((CodeObject)field).Parent = this;
            }
        }

        /// <summary>
        /// Create a reference to the <see cref="CodeObject"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>The appropriate type of reference.</returns>
        public virtual SymbolicRef CreateRef(bool isFirstOnLine)
        {
            throw new Exception("References aren't supported for a code object of type: " + GetType());
        }

        /// <summary>
        /// Create a reference to the <see cref="CodeObject"/>.
        /// </summary>
        /// <returns>The appropriate type of reference.</returns>
        public SymbolicRef CreateRef()
        {
            return CreateRef(false);
        }

        /// <summary>
        /// Find the parent object of the specified type.
        /// </summary>
        public T FindParent<T>() where T : CodeObject
        {
            CodeObject obj = this;
            do
                obj = obj.Parent;
            while (obj != null && !(obj is T));
            return (obj != null ? (T)obj : null);
        }

        /// <summary>
        /// Find the parent method or anonymous method of the current code object.
        /// </summary>
        /// <returns>The parent <see cref="MethodDeclBase"/> or <see cref="AnonymousMethod"/>, or null if none found.</returns>
        public CodeObject FindParentMethod()
        {
            CodeObject obj = this;
            do
                obj = obj.Parent;
            while (obj != null && !(obj is MethodDeclBase || obj is AnonymousMethod));
            return obj;
        }

        /// <summary>
        /// Get the <see cref="Namespace"/> for this <see cref="CodeObject"/>.
        /// </summary>
        public Namespace GetNamespace()
        {
            NamespaceDecl parentNamespaceDecl = FindParent<NamespaceDecl>();
            return (parentNamespaceDecl != null ? parentNamespaceDecl.Namespace : null);
        }

        /// <summary>
        /// Get the indent level of this object.
        /// </summary>
        public virtual int GetIndentLevel()
        {
            // We can only be indented if we have a parent
            if (_parent != null)
            {
                // Start with the parent's indent level, and add one if the parent says we're indented
                int indentLevel = _parent.GetIndentLevel();
                if (_parent.IsChildIndented(this))
                    ++indentLevel;
                return indentLevel;
            }
            return 0;
        }

        /// <summary>
        /// Returns true if the specified child object is indented from the parent.
        /// </summary>
        protected virtual bool IsChildIndented(CodeObject obj)
        {
            // The child object can only be indented if it's the first thing on the line
            if (obj.IsFirstOnLine)
            {
                // By default, any child object on a new line that isn't a prefix should be indented
                return !IsChildPrefix(obj);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the specified child object is prefixed to the current object.
        /// </summary>
        protected bool IsChildPrefix(CodeObject obj)
        {
            return (_annotations != null && Enumerable.Any(_annotations, delegate (Annotation annotation) { return annotation == obj; }));
        }

        /// <summary>
        /// Get the current indent in spaces.
        /// </summary>
        public int GetIndentSpaceCount()
        {
            return GetIndentLevel() * TabSize;
        }

        /// <summary>
        /// Set the line and column numbers to those in the specified <see cref="CodeObject"/>.
        /// </summary>
        protected internal void SetLineCol(CodeObject codeObject)
        {
            _lineNumber = codeObject._lineNumber;
            _columnNumber = codeObject._columnNumber;
        }

        /// <summary>
        /// Set the line and column numbers to those in the specified token.
        /// </summary>
        protected internal void SetLineCol(Token token)
        {
            _lineNumber = token.LineNumber;
            _columnNumber = token.ColumnNumber;
        }

        /// <summary>
        /// Explicit interface implementation of Clone()
        /// </summary>
        object ICloneable.Clone()
        {
            // Delegate to virtual Clone() method below
            return Clone();
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public virtual CodeObject Clone()
        {
            // Perform a shallow copy - any references in the object MUST be nulled or cloned manually
            // by an overridden version of this method in derived classes!
            CodeObject clone = (CodeObject)MemberwiseClone();
            // Clone annotations so we get comments, attributes, and directives - but clear any messages
            clone._annotations = ChildListHelpers.Clone(_annotations, clone);
            clone.RemoveAllMessages(MessageSource.Unspecified, false);
            // Clear the parent reference - do NOT use the property, or the child collection of a Block
            // will have its Parent set to null in the *source* collection since we have shallow-copied
            // references and not cloned them yet.
            clone._parent = null;
            return clone;
        }

        /// <summary>
        /// Dispose the <see cref="CodeObject"/>.
        /// </summary>
        public virtual void Dispose()
        {
            // Clear the parent reference if disposed.  Although this is technically not necessary in order for
            // the object to be garbage-collected, it's still a very good idea in order to help discover coding
            // errors where objects are manipulated after becoming obsolete.
            _parent = null;
        }

        /// <summary>
        /// Create the list of child <see cref="Annotation"/>s, or return the existing one.
        /// </summary>
        public ChildList<Annotation> CreateAnnotations()
        {
            if (_annotations == null)
                _annotations = new ChildList<Annotation>(this);
            return _annotations;
        }

        /// <summary>
        /// Annotations (comments, attributes, directives, messages) associated with the current code object.
        /// </summary>
        public ChildList<Annotation> Annotations
        {
            get { return _annotations; }
            set { SetField(ref _annotations, value); }
        }

        /// <summary>
        /// True if the code object has any annotations.
        /// </summary>
        public bool HasAnnotations
        {
            get { return (_annotations != null && _annotations.Count > 0); }
        }

        /// <summary>
        /// True if the code object has any comments of any kind.
        /// </summary>
        public bool HasComments
        {
            get { return HasAnnotation<CommentBase>(); }
        }

        /// <summary>
        /// True if the code object has any EOL comments.
        /// </summary>
        public bool HasEOLComments
        {
            get
            {
                return (_annotations != null && Enumerable.Any(_annotations,
              delegate (Annotation annotation) { return annotation is Comment && annotation.IsEOL && !annotation.IsInfix; }));
            }
        }

        /// <summary>
        /// True if the code object has any regular (non-doc) preceeding (non-EOL, non-Infix, non-Postfix) comments.
        /// </summary>
        public bool HasNonEOLComments
        {
            get
            {
                return (_annotations != null && Enumerable.Any(_annotations,
              delegate (Annotation annotation) { return annotation is Comment && !annotation.IsEOL && !annotation.IsInfix && !annotation.IsPostfix; }));
            }
        }

        /// <summary>
        /// True if the code object has any EOL or Postfix annotations.
        /// </summary>
        public bool HasEOLOrPostAnnotations
        {
            get
            {
                return (_annotations != null && Enumerable.Any(_annotations,
              delegate (Annotation annotation) { return ((annotation is Comment && annotation.IsEOL) || annotation.IsPostfix) && !annotation.IsInfix; }));
            }
        }

        /// <summary>
        /// True if the code object has any documentation comments.
        /// </summary>
        public bool HasDocComments
        {
            get { return HasAnnotation<DocComment>(); }
        }

        /// <summary>
        /// True if the code object has any Infix comments.
        /// </summary>
        public bool HasInfixComments
        {
            get { return (_annotations != null && Enumerable.Any(_annotations, delegate (Annotation annotation) { return annotation is Comment && annotation.IsInfix; })); }
        }

        /// <summary>
        /// True if the code object has any attributes.
        /// </summary>
        public bool HasAttributes
        {
            get { return HasAnnotation<Attribute>(); }
        }

        /// <summary>
        /// True if the code object has any compiler directive annotations.
        /// </summary>
        public bool HasCompilerDirectives
        {
            get { return HasAnnotation<CompilerDirective>(); }
        }

        /// <summary>
        /// True if the code object has any postfix annotations.
        /// </summary>
        public bool HasPostAnnotations
        {
            get { return (_annotations != null && Enumerable.Any(_annotations, delegate (Annotation annotation) { return annotation.IsPostfix; })); }
        }

        /// <summary>
        /// True if the code object has any annotations on separate lines.
        /// </summary>
        public bool HasFirstOnLineAnnotations
        {
            get { return (_annotations != null && Enumerable.Any(_annotations, delegate (Annotation annotation) { return !(annotation is Message) && annotation.IsFirstOnLine; })); }
        }

        /// <summary>
        /// True if the code object has any generated messages.
        /// </summary>
        public bool HasMessages
        {
            get { return HasAnnotation<Message>(); }
        }

        /// <summary>
        /// True if the code object has any error messages.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return (_annotations != null && Enumerable.Any(_annotations,
              delegate (Annotation annotation) { return annotation is Message && ((Message)annotation).Severity == MessageSeverity.Error; }));
            }
        }

        protected bool HasAnnotation<T>() where T : Annotation
        {
            return (_annotations != null && Enumerable.Any(Enumerable.OfType<T>(_annotations)));
        }

        /// <summary>
        /// The comment for the code object (if any).
        /// </summary>
        /// <remarks>
        /// This property allows for the very convenient setting of comments in object initializers.
        /// Although there is support for multiple comments on the same object, this property doesn't
        /// support that, returning the first one that it finds, and replacing all existing ones when set.
        /// </remarks>
        public string Comment
        {
            get
            {
                // Just return the first (non-EOL, non-Postfix) comment if there is more than one
                if (_annotations != null)
                {
                    Comment comment = (Comment)Enumerable.FirstOrDefault(_annotations,
                        delegate (Annotation annotation) { return annotation is Comment && !annotation.IsEOL && !annotation.IsPostfix; });
                    if (comment != null)
                        return comment.Text;
                }
                return null;
            }
            set
            {
                // Remove all existing (non-EOL) comments before adding the new one
                if (_annotations != null)
                    _annotations.RemoveAll(delegate (Annotation annotation) { return annotation is Comment && !annotation.IsEOL && !annotation.IsPostfix; });
                if (value != null)
                    AttachComment(value);
            }
        }

        /// <summary>
        /// The documentation comment for the code object (if any).
        /// </summary>
        /// <remarks>
        /// This property allows for the very convenient setting of documentation comments in object initializers.
        /// Although there is support for multiple documentation comments on the same object, this property doesn't
        /// support that, returning the first one that it finds, and replacing all existing ones when set.
        /// </remarks>
        public DocComment DocComment
        {
            get
            {
                // Just return the first documentation comment if there is more than one
                if (_annotations != null)
                {
                    DocComment comment = (DocComment)Enumerable.FirstOrDefault(_annotations, delegate (Annotation annotation) { return annotation is DocComment; });
                    if (comment != null)
                        return comment;
                }
                return null;
            }
            set
            {
                // Remove all existing documentation comments before adding the new one
                if (_annotations != null)
                    _annotations.RemoveAll(delegate (Annotation annotation) { return annotation is DocComment; });
                if (value != null)
                    AttachAnnotation(value);
            }
        }

        /// <summary>
        /// The End-Of-Line comment for the code object (if any).
        /// </summary>
        /// <remarks>
        /// This property allows for the very convenient setting of EOL comments in object initializers.
        /// Although there is support for multiple EOL comments on the same object, this property doesn't
        /// support that, returning the first one that it finds, and replacing all existing ones when set.
        /// </remarks>
        public string EOLComment
        {
            get
            {
                // Just return the first (non-Infix) EOL comment if there is more than one
                if (_annotations != null)
                {
                    Comment comment = (Comment)Enumerable.FirstOrDefault(_annotations, delegate (Annotation annotation) { return annotation is Comment && annotation.IsEOL && !annotation.IsInfix; });
                    if (comment != null)
                        return comment.Text;
                }
                return null;
            }
            set
            {
                // Remove all existing EOL comments before adding the new one
                if (_annotations != null)
                    _annotations.RemoveAll(delegate (Annotation annotation) { return annotation is Comment && annotation.IsEOL && !annotation.IsInfix; });
                if (value != null)
                    AttachEOLComment(value);
            }
        }

        /// <summary>
        /// The infix comment for the code object (if any).
        /// </summary>
        /// <remarks>
        /// This property allows for the very convenient setting of infix comments in object initializers.
        /// Although there is support for multiple infix comments on the same object, this property doesn't
        /// support that, returning the first one that it finds, and replacing all existing ones when set.
        /// </remarks>
        public string InfixComment
        {
            get
            {
                // Just return the first infix comment if there is more than one
                if (_annotations != null)
                {
                    Comment comment = (Comment)Enumerable.FirstOrDefault(_annotations, delegate (Annotation annotation) { return annotation is Comment && annotation.IsInfix; });
                    if (comment != null)
                        return comment.Text;
                }
                return null;
            }
            set
            {
                // Remove all existing infix comments before adding the new one
                if (_annotations != null)
                    _annotations.RemoveAll(delegate (Annotation annotation) { return annotation is Comment && annotation.IsInfix; });
                if (value != null)
                    AttachComment(value, AnnotationFlags.IsInfix1);
            }
        }

        /// <summary>
        /// The postfix comment for the code object (if any).
        /// </summary>
        /// <remarks>
        /// This property allows for the very convenient setting of postfix comments in object initializers.
        /// Although there is support for multiple postfix comments on the same object, this property doesn't
        /// support that, returning the first one that it finds, and replacing all existing ones when set.
        /// </remarks>
        public string PostfixComment
        {
            get
            {
                // Just return the first postfix comment if there is more than one
                if (_annotations != null)
                {
                    Comment comment = (Comment)Enumerable.FirstOrDefault(_annotations, delegate (Annotation annotation) { return annotation is Comment && annotation.IsPostfix; });
                    if (comment != null)
                        return comment.Text;
                }
                return null;
            }
            set
            {
                // Remove all existing postfix comments before adding the new one
                if (_annotations != null)
                    _annotations.RemoveAll(delegate (Annotation annotation) { return annotation is Comment && annotation.IsPostfix; });
                if (value != null)
                    AttachComment(value, AnnotationFlags.IsPostfix);
            }
        }

        /// <summary>
        /// Create a comment object and attach it to the code object.
        /// </summary>
        public void AttachComment(string comment, AnnotationFlags annotationFlags, CommentFlags commentFlags)
        {
            Comment commentObj = new Comment(comment, commentFlags);
            // Default any infix comments to NOT first-on-line
            if ((annotationFlags & AnnotationFlags.InfixMask) != 0)
                commentObj.IsFirstOnLine = false;
            AttachAnnotation(commentObj, annotationFlags);
        }

        /// <summary>
        /// Create a comment object and attach it to the code object.
        /// </summary>
        public void AttachComment(string comment, AnnotationFlags annotationFlags)
        {
            AttachComment(comment, annotationFlags, CommentFlags.None);
        }

        /// <summary>
        /// Create a comment object and attach it to the code object.
        /// </summary>
        public void AttachComment(string comment)
        {
            AttachComment(comment, AnnotationFlags.None, CommentFlags.None);
        }

        /// <summary>
        /// Create an EOL comment object and attach it to the code object.
        /// </summary>
        public void AttachEOLComment(string comment)
        {
            AttachAnnotation(new Comment(comment, CommentFlags.EOL));
        }

        /// <summary>
        /// Create a message and attach it to the code object.
        /// </summary>
        public virtual void AttachMessage(string text, MessageSeverity messageType, MessageSource messageSource)
        {
            if (messageType != MessageSeverity.Unspecified)
                AttachAnnotation(new Message(text, messageType, messageSource));
        }

        /// <summary>
        /// Attach an <see cref="Annotation"/> to the <see cref="CodeObject"/> at the specified position.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="position">The position at which to place it.</param>
        /// <param name="atFront">Inserts at the front of any existing annotations if true, otherwise adds at the end.</param>
        public void AttachAnnotation(Annotation annotation, AnnotationFlags position, bool atFront)
        {
            annotation.AnnotationFlags |= position;
            AttachAnnotation(annotation, atFront);
        }

        /// <summary>
        /// Attach an <see cref="Annotation"/> to the <see cref="CodeObject"/> at the specified position.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="position">The position at which to place it.</param>
        public void AttachAnnotation(Annotation annotation, AnnotationFlags position)
        {
            annotation.AnnotationFlags |= position;
            AttachAnnotation(annotation);
        }

        /// <summary>
        /// Attach an <see cref="Annotation"/> (<see cref="Comment"/>, <see cref="DocComment"/>, <see cref="Attribute"/>, <see cref="CompilerDirective"/>, or <see cref="Message"/>) to the <see cref="CodeObject"/>.
        /// </summary>
        /// <param name="annotation">The <see cref="Annotation"/>.</param>
        /// <param name="atFront">Inserts at the front if true, otherwise adds at the end.</param>
        public virtual void AttachAnnotation(Annotation annotation, bool atFront)
        {
            // Clear any existing parent object, so if we're moving the annotation it won't get unnecessarily cloned
            annotation.Parent = null;
            if (atFront)
                CreateAnnotations().Insert(0, annotation);
            else
                CreateAnnotations().Add(annotation);

            // Adjust the newlines to correct the formatting if not EOL, Postfix, a Message, or the left side of
            // a binary operator (because then it's a prefix operator, with special newline formatting).
            if (!annotation.IsEOL && !annotation.IsInfix && !annotation.IsPostfix && !(annotation is Message)
                && !(_parent is BinaryOperator && ((BinaryOperator)_parent).Left == this))
            {
                // First, upgrade the object to IsFirstOnLine if it's not an Expression, and its newlines haven't
                // been manually set yet, and the default newline state was changed by the added annotation.
                if (!(this is Expression) && !IsNewLinesSet && !IsFirstOnLine && IsFirstOnLineDefault)
                {
                    // Use IsFirstOnLine instead of SetNewLines(), and force IsNewLinesSet to false, so that this
                    // works properly for Blocks.
                    IsFirstOnLine = true;
                    IsNewLinesSet = false;
                }

                // If the annotation was prefixed to any existing annotations (or if there weren't any
                // existing relevant annotations), then swap newlines with the parent object.
                CodeObject swapWith = this;

                // If the annotation was added after any existing annotations, swap newline counts with the
                // last existing annotation that isn't a Message or EOL comment (since they don't affect newlines).
                if (!atFront && _annotations.Count > 1)
                {
                    for (int i = _annotations.Count - 2; i >= 0; --i)
                    {
                        Annotation existing = _annotations[i];
                        if (!(existing is Message) && !existing.IsEOL)
                        {
                            swapWith = existing;
                            break;
                        }
                    }
                }

                // Swap the newline counts, but only if the annotation has more than the current object, or
                // if we added to the front, then also swap if they're different.
                if (annotation.NewLines > swapWith.NewLines || (atFront && annotation.NewLines != swapWith.NewLines))
                {
                    int newLines = swapWith.NewLines;
                    swapWith.SetNewLines(annotation.NewLines);
                    annotation.SetNewLines(newLines);
                }
            }

            // Send notification if the annotation is 'listed'
            if (annotation.IsListed)
                NotifyListedAnnotationAdded(annotation);
        }

        /// <summary>
        /// Attach an <see cref="Annotation"/> (<see cref="Comment"/>, <see cref="DocComment"/>, <see cref="Attribute"/>, <see cref="CompilerDirective"/>, or <see cref="Message"/>) to the <see cref="CodeObject"/>.
        /// </summary>
        /// <param name="annotation">The <see cref="Annotation"/>.</param>
        public void AttachAnnotation(Annotation annotation)
        {
            AttachAnnotation(annotation, false);
        }

        /// <summary>
        /// Move any annotations from the specified location to the specified destination location.
        /// </summary>
        public void MoveAnnotations(AnnotationFlags fromFlag, AnnotationFlags toFlag)
        {
            if (_annotations != null)
            {
                foreach (Annotation annotation in _annotations)
                {
                    if (annotation.AnnotationFlags.HasFlag(fromFlag))
                        annotation.AnnotationFlags = (annotation.AnnotationFlags & ~fromFlag) | toFlag;
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="DocSummary"/> documentation comment, or null if none exists.
        /// </summary>
        public virtual DocSummary GetDocSummary()
        {
            return (_annotations != null ? Enumerable.FirstOrDefault(Enumerable.Select<DocComment, DocSummary>(Enumerable.OfType<DocComment>(_annotations),
                delegate (DocComment annotation) { return annotation.GetDocSummary(); })) : null);
        }

        /// <summary>
        /// Get the comment that satisfies the specified predicate.
        /// </summary>
        public Comment GetComment(Predicate<Comment> predicate)
        {
            if (_annotations != null)
                return (Comment)Enumerable.FirstOrDefault(_annotations, delegate (Annotation annotation) { return annotation is Comment && predicate((Comment)annotation); });
            return null;
        }

        /// <summary>
        /// Remove all messages from this object, or optionally only from the specified source.
        /// </summary>
        /// <param name="messageSource">The message source.</param>
        /// <param name="notify">Pass <c>false</c> to NOT send notifications.</param>
        public void RemoveAllMessages(MessageSource messageSource, bool notify)
        {
            RemoveAllAnnotationsWhere<Message>(delegate (Message annotation) { return annotation.Source == messageSource || messageSource == MessageSource.Unspecified; }, notify);
        }

        /// <summary>
        /// Remove all messages from this object, or optionally only from the specified source.
        /// </summary>
        /// <param name="messageSource">The message source.</param>
        public void RemoveAllMessages(MessageSource messageSource)
        {
            RemoveAllMessages(messageSource, true);
        }

        /// <summary>
        /// Remove all messages from this object, or optionally only from the specified source.
        /// </summary>
        public void RemoveAllMessages()
        {
            RemoveAllMessages(MessageSource.Unspecified, true);
        }

        /// <summary>
        /// Remove all annotations from this object where the specified predicate is true.
        /// </summary>
        public void RemoveAllAnnotationsWhere<T>(Predicate<T> predicate, bool notify) where T : Annotation
        {
            if (_annotations != null)
            {
                for (int i = _annotations.Count - 1; i >= 0; --i)
                {
                    T annotation = _annotations[i] as T;
                    if (annotation != null && predicate(annotation))
                    {
                        _annotations.RemoveAt(i);
                        if (annotation.IsListed && notify)
                            NotifyListedAnnotationRemoved(annotation);
                    }
                }
                if (_annotations.Count == 0)
                    _annotations = null;
            }
        }

        /// <summary>
        /// Remove all annotations from this object where the specified predicate is true.
        /// </summary>
        public void RemoveAllAnnotationsWhere<T>(Predicate<T> predicate) where T : Annotation
        {
            RemoveAllAnnotationsWhere(predicate, true);
        }

        /// <summary>
        /// Propagate listed annotations to the higher levels.
        /// </summary>
        protected virtual void NotifyListedAnnotationAdded(Annotation annotation)
        {
            if (_parent != null)
                _parent.NotifyListedAnnotationAdded(annotation);
        }

        /// <summary>
        /// Remove listed annotations from the higher levels.
        /// </summary>
        protected virtual void NotifyListedAnnotationRemoved(Annotation annotation)
        {
            if (_parent != null)
                _parent.NotifyListedAnnotationRemoved(annotation);
        }

        /// <summary>
        /// Returns <c>true</c> if the attribute with the specified name exists on the object, otherwise <c>false</c>.
        /// </summary>
        public bool HasAttribute(string attributeName)
        {
            return (_annotations != null && Enumerable.Any(_annotations,
                delegate (Annotation annotation) { return annotation is Attribute && ((Attribute)annotation).FindAttributeExpression(attributeName) != null; }));
        }

        /// <summary>
        /// Returns the first attribute expression (<see cref="Call"/> or <see cref="ConstructorRef"/>) with the specified name on the <see cref="CodeObject"/>.
        /// </summary>
        public Expression GetAttribute(string attributeName)
        {
            if (_annotations != null)
            {
                foreach (Annotation annotation in _annotations)
                {
                    if (annotation is Attribute)
                    {
                        Expression expression = ((Attribute)annotation).FindAttributeExpression(attributeName);
                        if (expression != null)
                            return expression;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Remove the attribute expression with the specified name.
        /// </summary>
        /// <returns><c>true</c> if found and removed, otherwise <c>false</c>.</returns>
        public bool RemoveAttribute(string attributeName)
        {
            if (_annotations != null)
            {
                for (int i = _annotations.Count - 1; i >= 0; --i)
                {
                    if (_annotations[i] is Attribute)
                    {
                        Attribute attribute = (Attribute)_annotations[i];
                        if (attribute.RemoveAttributeExpression(attributeName))
                        {
                            // If the attribute has no more expressions, remove it
                            if (!attribute.HasAttributeExpressions)
                                _annotations.RemoveAt(i);
                            return true;
                        }
                    }
                }
                if (_annotations.Count == 0)
                    _annotations = null;
            }
            return false;
        }

        /// <summary>
        /// Get the type of the worst attached message.
        /// </summary>
        public MessageSeverity GetWorstMessageType()
        {
            MessageSeverity type = MessageSeverity.Unspecified;
            if (_annotations != null)
            {
                foreach (Annotation annotation in _annotations)
                {
                    if (annotation is Message)
                    {
                        Message message = (Message)annotation;
                        if (type == MessageSeverity.Unspecified && message.Severity != MessageSeverity.Unspecified)
                            type = message.Severity;
                        else if (message.Severity != MessageSeverity.Unspecified && message.Severity < type)
                            type = message.Severity;
                    }
                }
            }
            return type;
        }

        /// <summary>
        /// Parse a code object.
        /// </summary>
        protected CodeObject(Parser parser, CodeObject parent)
        {
            _parent = parent;
            Token token = parser.Token;
            if (token != null)
            {
                NewLines = token.NewLines;
                SetLineCol(token);

                // Remove more than 3 consecutive blank lines if auto-cleanup is on
                if (AutomaticFormattingCleanup && !parser.IsGenerated && NewLines > 4)
                    NewLines = 4;
            }
        }

        /// <summary>
        /// Parse the specified expected token, attaching a parse error to the current object if it doesn't exist.
        /// </summary>
        /// <returns>True if the token was successfully parse, otherwise false.</returns>
        protected internal bool ParseExpectedToken(Parser parser, string token)
        {
            if (parser.TokenText == token)
            {
                parser.NextToken();  // Move past expected token
                return true;
            }
            parser.AttachMessage(this, "'" + token + "' expected", parser.Token);
            return false;
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public virtual bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return false;
        }

        private void MoveComment(CommentBase comment, bool atFront, bool forceNewLine, AnnotationFlags annotationFlags)
        {
            if ((forceNewLine || comment.IsEOL) && !comment.IsFirstOnLine)
                comment.IsFirstOnLine = true;
            comment.IsEOL = false;
            comment.AnnotationFlags = comment.AnnotationFlags | annotationFlags;
            AttachAnnotation(comment, atFront);
            AdjustCommentIndentation(comment);
        }

        /// <summary>
        /// Move all (regular or EOL) comments from the specified token to the current code object, converting any
        /// EOL comments to regular comments (which will be rendered inline if necessary).
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="atFront">Inserts at the front of any existing annotations if true, otherwise adds at the end.</param>
        /// <param name="forceNewLine">Force all comments to start on a new line if true.</param>
        /// <param name="annotationFlags">Annotation flags to set on the comments.</param>
        public void MoveAllComments(Token token, bool atFront, bool forceNewLine, AnnotationFlags annotationFlags)
        {
            if (token != null && token.TrailingComments != null)
            {
                // If we're inserting, we also need to traverse the list in reverse
                if (atFront)
                {
                    for (int i = token.TrailingComments.Count - 1; i >= 0; --i)
                        MoveComment(token.TrailingComments[i], true, forceNewLine, annotationFlags);
                }
                else
                {
                    foreach (CommentBase comment in token.TrailingComments)
                        MoveComment(comment, false, forceNewLine, annotationFlags);
                }
                token.TrailingComments = null;
            }
        }

        /// <summary>
        /// Move all (regular or EOL) comments from the specified token to the current code object, converting any
        /// EOL comments to regular comments (which will be rendered inline if necessary).
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="atFront">Inserts at the front of any existing annotations if true, otherwise adds at the end.</param>
        /// <param name="forceNewLine">Force all comments to start on a new line if true.</param>
        public void MoveAllComments(Token token, bool atFront, bool forceNewLine)
        {
            MoveAllComments(token, atFront, forceNewLine, AnnotationFlags.None);
        }

        /// <summary>
        /// Move all (regular or EOL) comments from the specified token to the current code object, converting any
        /// EOL comments to regular comments (which will be rendered inline if necessary).
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="atFront">Inserts at the front of any existing annotations if true, otherwise adds at the end.</param>
        public void MoveAllComments(Token token, bool atFront)
        {
            MoveAllComments(token, atFront, false, AnnotationFlags.None);
        }

        /// <summary>
        /// Move all (regular or EOL) comments from the specified token to the current code object, converting any
        /// EOL comments to regular comments (which will be rendered inline if necessary).
        /// </summary>
        /// <param name="token">The token.</param>
        public void MoveAllComments(Token token)
        {
            MoveAllComments(token, false, false, AnnotationFlags.None);
        }

        private void MoveComment(CommentBase comment, bool atFront, List<CommentBase> comments, int i)
        {
            AttachAnnotation(comment, atFront);
            AdjustCommentIndentation(comment);
            comments.RemoveAt(i);
        }

        /// <summary>
        /// Move any non-EOL comments from the specified token to the current code object.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="atFront">Inserts at the front of any existing annotations if true, otherwise adds at the end.</param>
        public void MoveComments(Token token, bool atFront)
        {
            if (token != null)
            {
                List<CommentBase> comments = token.TrailingComments;
                if (comments != null)
                {
                    // If we're inserting, we also need to traverse the list in reverse
                    if (atFront)
                    {
                        for (int i = comments.Count - 1; i >= 0; --i)
                        {
                            CommentBase comment = comments[i];
                            if (!comment.IsEOL)
                                MoveComment(comment, true, comments, i);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < comments.Count;)
                        {
                            CommentBase comment = comments[i];
                            if (!comment.IsEOL)
                            {
                                MoveComment(comment, false, comments, i);
                                continue;
                            }
                            // Only increment if we didn't change the list!
                            ++i;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Move any non-EOL comments from the specified token to the current code object.
        /// </summary>
        /// <param name="token">The token.</param>
        public void MoveComments(Token token)
        {
            MoveComments(token, false);
        }

        /// <summary>
        /// Move any non-EOL comments from the specified token to the current code object as Post comments.
        /// </summary>
        /// <param name="token">The token.</param>
        public void MoveCommentsAsPost(Token token)
        {
            if (token != null)
            {
                List<CommentBase> comments = token.TrailingComments;
                if (comments != null && comments.Count > 0)
                {
                    for (int i = 0; i < comments.Count;)
                    {
                        CommentBase comment = comments[i];
                        if (!comment.IsEOL)
                        {
                            comment.IsPostfix = true;
                            MoveComment(comment, false, comments, i);
                            continue;
                        }
                        // Only increment if we didn't change the list!
                        ++i;
                    }
                }
            }
        }

        /// <summary>
        /// Move any EOL comment from the specified token to the current code object.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="includeInlines">Include inline comments as EOL comments if true.</param>
        /// <param name="atFront">Inserts at the front of any existing annotations if true, otherwise adds at the end.</param>
        public CommentBase MoveEOLComment(Token token, bool includeInlines, bool atFront)
        {
            if (token != null)
            {
                List<CommentBase> comments = token.TrailingComments;
                if (comments != null && comments.Count > 0)
                {
                    // Move any EOL comment (a Token can only have one, and it should be the first one).
                    CommentBase comment = comments[0];
                    if (comment.IsEOL)
                    {
                        AttachAnnotation(comment, atFront);
                        //AdjustCommentIndentation(comment);  // Shouldn't need to do this for EOL comments
                        comments.RemoveAt(0);
                        return comment;
                    }
                    // Also treat any inline comment as an EOL comment if so directed - this allows inline comments embedded
                    // within expressions to be treated as EOL comments on the preceeding sub-expression.  In some cases, we
                    // can't do this, such as with EOL comments following a comma that are being moved to the preceeding expression
                    // (in such a case, they only belong to the preceeding expression if they're true EOL comments, otherwise they
                    // should be associated with the following expression).
                    if (includeInlines && !comment.IsFirstOnLine && !(comment is DocComment))
                    {
                        comment.IsEOL = true;
                        AttachAnnotation(comment, atFront);
                        comments.RemoveAt(0);
                        return comment;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Move any EOL comment from the specified token to the current code object.
        /// </summary>
        /// <param name="token">The token.</param>
        public CommentBase MoveEOLComment(Token token)
        {
            return MoveEOLComment(token, true, false);
        }

        /// <summary>
        /// Move any EOL comment from the specified token to the current code object as an Infix EOL comment.
        /// </summary>
        /// <param name="token">The token.</param>
        public void MoveEOLCommentAsInfix(Token token)
        {
            CommentBase comment = MoveEOLComment(token, true, false);
            if (comment != null)
                comment.IsInfix = true;
        }

        private void MoveAnnotation(ChildList<Annotation> annotations, int i, Annotation annotation)
        {
            annotations.RemoveAt(i);
            if (annotation.IsListed)
                NotifyListedAnnotationRemoved(annotation);
            AttachAnnotation(annotation);
        }

        /// <summary>
        /// Move any EOL comment from the specified code object to the current code object.
        /// </summary>
        public void MoveEOLComment(CodeObject obj)
        {
            ChildList<Annotation> annotations = obj.Annotations;
            if (annotations != null)
            {
                for (int i = 0; i < annotations.Count; ++i)
                {
                    if (annotations[i] is Comment)
                    {
                        // Move the first EOL comment
                        Comment comment = (Comment)annotations[i];
                        if (comment.IsEOL)
                        {
                            comment.IsInfix = false;
                            MoveAnnotation(annotations, i, comment);
                            break;
                        }
                    }
                }
                if (annotations.Count == 0)
                    obj._annotations = null;
            }
        }

        /// <summary>
        /// Move any EOL or Postfix annotations from the specified code object to the current code object.
        /// </summary>
        public void MoveEOLAndPostAnnotations(CodeObject obj)
        {
            ChildList<Annotation> annotations = obj.Annotations;
            if (annotations != null)
            {
                for (int i = 0; i < annotations.Count;)
                {
                    Annotation annotation = annotations[i];
                    if (((annotation is Comment && annotation.IsEOL) || annotation.IsPostfix) && !annotation.IsInfix)
                    {
                        MoveAnnotation(annotations, i, annotation);
                        continue;
                    }
                    // Only increment if we didn't change the list!
                    ++i;
                }
                if (annotations.Count == 0)
                    obj._annotations = null;
            }
        }

        /// <summary>
        /// Move any prefix annotations on the specified code object to the current code object as post annotations.
        /// </summary>
        public void MovePrefixAnnotationsAsPost(CodeObject obj)
        {
            ChildList<Annotation> annotations = obj.Annotations;
            if (annotations != null)
            {
                for (int i = 0; i < annotations.Count;)
                {
                    Annotation annotation = annotations[i];
                    if (!annotation.IsEOL && !annotation.IsPostfix && !annotation.IsInfix)
                    {
                        annotation.IsPostfix = true;
                        MoveAnnotation(annotations, i, annotation);
                        continue;
                    }
                    // Only increment if we didn't change the list!
                    ++i;
                }
                if (annotations.Count == 0)
                    obj._annotations = null;
            }
        }

        /// <summary>
        /// Adjust the content of the specified comment to compensate if it was outdented.
        /// </summary>
        public static void AdjustCommentIndentation(CommentBase commentBase)
        {
            // If the comment was "outdented", remove spaces from the left of the comment text to
            // compensate for the indentation level.
            if (commentBase is Comment)
            {
                Comment comment = (Comment)commentBase;
                int removeSpaceCount = comment.GetIndentSpaceCount() - comment.PrefixSpaceCount - (comment.IsBlock ? 0 : CodeDOM.Comment.ParseToken.Length);
                if (removeSpaceCount > 0)
                {
                    // If we fail to remove the desired count of spaces, and 1 space is implied, then
                    // try removing one less space, and if that works set the NoSpaceAfterDelimiter flag.
                    if (!comment.RemoveSpaces(removeSpaceCount) && !comment.NoSpaceAfterDelimiter)
                    {
                        if (comment.RemoveSpaces(removeSpaceCount - 1))
                            comment.NoSpaceAfterDelimiter = true;
                    }
                }
            }
        }

        /// <summary>
        /// Parse any comments, attributes, compiler directives.
        /// </summary>
        protected void ParseAnnotations(Parser parser, CodeObject parent, bool forcePostfix, bool forceNotPostfix)
        {
            bool isPostfix = !forceNotPostfix;

            // Look only for comments, doc comments, attributes, compiler directives, or trailing regular comments on the last token
            while (parser.TokenType == TokenType.Comment || parser.TokenType == TokenType.DocCommentStart
                || parser.TokenText == Attribute.ParseTokenStart || parser.TokenText == CompilerDirective.ParseToken || parser.LastToken.HasTrailingComments)
            {
                // Consume any regular comments on the last token first, otherwise get the next token
                CodeObject obj;
                if (parser.LastToken.HasTrailingComments)
                {
                    obj = parser.LastToken.TrailingComments[0];
                    parser.LastToken.TrailingComments.RemoveAt(0);
                }
                else
                    obj = parser.ProcessToken(parent);

                // If we're not forcing post-mode, exit post-mode if we hit anything that isn't a non-starting conditional directive
                if (isPostfix && !forcePostfix && !(obj is ConditionalDirectiveBase && !(obj is IfDirective)))
                    isPostfix = false;

                if (obj != null)
                {
                    // Process the object - attach if post, otherwise add to unused list
                    if (isPostfix)
                    {
                        if (obj is Comment || obj is CompilerDirective)
                            AttachAnnotation((Annotation)obj, AnnotationFlags.IsPostfix);
                        else
                            parser.AddUnused(obj);
                    }
                    else
                        parser.AddUnused(obj);
                }
            }
        }

        /// <summary>
        /// Parse annotations from the Unused list.
        /// </summary>
        protected internal void ParseUnusedAnnotations(Parser parser, CodeObject parent, bool includeAll, int ifCount)
        {
            // Parse any preceeding documentation comments, attributes, and (only if 'includeAll' is true) compiler
            // directives from the unused list.  Also, always parse the special-case of an attribute sandwiched by
            // compiler directives.
            bool isFirst = true;
            bool stopOnDirective = false;
            Annotation lastUnusedAnnotation = parser.LastUnusedCodeObject as Annotation;
            while (lastUnusedAnnotation != null)
            {
                if (stopOnDirective && lastUnusedAnnotation is CompilerDirective)
                    break;

                // If 'includeAll' isn't true, stop under certain conditions
                if (!includeAll)
                {
                    // Stop if we hit a compiler directive, except for special cases
                    if (lastUnusedAnnotation is CompilerDirective)
                    {
                        // If a conditional directive other than '#if' (and not the first annotation and separated by a blank line
                        // from the current object) is preceeded only by attributes and/or other conditional directives, then allow it.
                        if (lastUnusedAnnotation is ConditionalDirectiveBase && !(lastUnusedAnnotation is IfDirective)
                            && !(isFirst && NewLines > 1))
                        {
                            // Verify that we have only attributes, doc comments, and/or conditional directives back to a starting '#if'
                            bool verified = false;
                            for (int unusedIndex = parser.Unused.Count - 2; unusedIndex >= 0; --unusedIndex)
                            {
                                CodeObject previousObject = parser.GetUnusedCodeObject(unusedIndex);
                                if (previousObject is IfDirective)
                                {
                                    verified = true;
                                    break;
                                }
                                if (!(previousObject is Attribute || previousObject is DocComment || previousObject is ConditionalDirective))
                                    break;
                            }
                            // Abort if we didn't find a starting '#if'
                            if (!verified)
                                break;

                            // Turn on inclusion of compiler attributes so we'll read them all up to the starting '#if'
                            includeAll = true;
                            ++ifCount;
                        }
                        else
                            break;
                    }

                    // Stop if we hit a regular comment as the first annotation (since we have no way of being sure that it
                    // belongs to the current object as opposed to a block of code), EXCEPT when it's preceeded by a doc comment.
                    if (lastUnusedAnnotation is Comment && isFirst)
                    {
                        CodeObject nextToLastUnused = parser.GetUnusedCodeObject(parser.Unused.Count - 2);
                        if (!(nextToLastUnused is DocComment))
                            break;
                    }

                    // Stop if we hit a global attribute (assembly or module)
                    if (lastUnusedAnnotation is Attribute && ((Attribute)lastUnusedAnnotation).IsGlobal)
                        break;
                }

                UnusedCodeObject unused = (UnusedCodeObject)parser.RemoveLastUnused();
                MoveComments(unused.LastToken, true);
                Annotation annotation = (Annotation)unused.CodeObject;
                bool preceedingBlankLine = (annotation.NewLines > 1);
                AttachAnnotation(annotation, true);
                isFirst = false;

                // If 'includeAll' is true (which is used for expressions, and for block statements when it's been determined
                // that they're "sandwiched" by conditional directives), stop if any specified 'ifCount' is reached.
                if (includeAll)
                {
                    if (annotation is IfDirective && ifCount > 0 && --ifCount == 0)
                    {
                        // We're done with preceeding conditional directives, but also still check for preceeding attributes
                        // and/or doc comments.
                        stopOnDirective = true;
                    }
                }
                // Otherwise, stop if we find blank lines between any of the annotations
                else if (preceedingBlankLine)
                    break;

                lastUnusedAnnotation = parser.LastUnusedCodeObject as Annotation;
            }
        }

        /// <summary>
        /// Parse annotations from the Unused list.
        /// </summary>
        protected internal void ParseUnusedAnnotations(Parser parser, CodeObject parent, bool includeAll)
        {
            ParseUnusedAnnotations(parser, parent, includeAll, 0);
        }

        /// <summary>
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public virtual bool IsSingleLineDefault
        {
            get { return !HasFirstOnLineAnnotations; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public virtual bool IsSingleLine
        {
            get
            {
                if (_annotations != null && _annotations.Count > 0)
                {
                    foreach (Annotation annotation in _annotations)
                    {
                        if (!(annotation is Message))
                        {
                            if (annotation.IsFirstOnLine || !annotation.IsSingleLine)
                                return false;
                        }
                    }
                }
                return true;
            }
            set
            {
                if (_annotations != null && _annotations.Count > 0)
                {
                    foreach (Annotation annotation in _annotations)
                    {
                        if (!((annotation is Comment && annotation.IsEOL) || annotation is Message || annotation is CompilerDirective))
                        {
                            annotation.IsFirstOnLine = !value;
                            if (value)
                                annotation.IsSingleLine = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public virtual bool IsFirstOnLineDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public virtual int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations
            return (HasFirstOnLineAnnotations ? 2 : 1);
        }

        /// <summary>
        /// Move formatting from the specified code object to the current object.
        /// </summary>
        public void MoveFormatting(CodeObject codeObject)
        {
            if (codeObject != null)
            {
                // Move any newlines to the object if there are more than it already has
                if (codeObject.NewLines > NewLines)
                    NewLines = codeObject.NewLines;

                // Clear the newlines on the object
                codeObject.NewLines = 0;
            }
        }

        /// <summary>
        /// Move formatting from the specified token to the current object.
        /// </summary>
        public void MoveFormatting(Token token)
        {
            if (token != null && token.NewLines > NewLines)
                NewLines = token.NewLines;
        }

        /// <summary>
        /// Copy formatting from another code object.
        /// </summary>
        public void CopyFormatting(CodeObject obj)
        {
            _formatFlags = (_formatFlags & FormatFlags.NonFormatting) | (obj._formatFlags & ~FormatFlags.NonFormatting);
        }

        /// <summary>
        /// Formatting flags.
        /// </summary>
        /// <remarks>
        /// Code objects contain some minimal formatting information used to control their "display" (whether as text
        /// or in a GUI), such as line breaks, parentheses, braces, and termination characters.
        /// When existing code is parsed, existing formatting is replicated as much as possible.  When code objects are
        /// manually created or modified, formatting is defaulted, but if certain settings are specifically set (such
        /// as newlines and parens/braces), then those settings must override the default formatting.  Some of these
        /// flags actually have nothing to do with formatting (such as Generated or Const), and are always preserved
        /// by the formatting logic.
        /// </remarks>
        [Flags]
        public enum FormatFlags : ushort
        {
            /// <summary>No formatting flags.</summary>
            None = 0x0000,

            /// <summary>Newline count bit field mask (0 to 255 newlines preceed the object).</summary>
            NewLineMask = 0x007f,

            /// <summary>Indicates that the newline count has been specifically set vs defaulted.</summary>
            NewLinesSet = 0x0100,

            /// <summary>Used by Blocks/Initializers to represent a newline before the '{', as the NewLineMask area is used for newlines before the '}'.
            /// Used by objects with parens to represent a newline before the closing ')'.</summary>
            InfixNewLine = 0x0200,

            /// <summary>Render the object with no indentation (for left-justified comments, initializers, anonymous methods, conditionals).</summary>
            NoIndentation = 0x0400,

            /// <summary>Render a terminator after the statement or expression.</summary>
            Terminator = 0x0800,

            /// <summary>Render parens/braces (for Expressions/Blocks) around the code object.</summary>
            Grouping = 0x1000,

            /// <summary>Indicates that the grouping flag has been specifically set vs defaulted.</summary>
            GroupingSet = 0x2000,

            /// <summary>Used by TypeRef to indicate a reference to a constant value.</summary>
            Const = 0x4000,

            /// <summary>Code object is "compiler" generated (used for default constructors, etc).</summary>
            Generated = 0x8000,

            /// <summary>Flags that aren't formatting related.</summary>
            NonFormatting = (Const | Generated)
        }

        protected internal void SetFormatFlag(FormatFlags flag, bool value)
        {
            if (value)
                _formatFlags |= flag;
            else
                _formatFlags &= ~flag;
        }

        /// <summary>
        /// Default format the specified child field code object.
        /// </summary>
        protected virtual void DefaultFormatField(CodeObject field)
        {
            // Just default format the field by default: this method is overridden by derived classes that
            // want to override the default formatting of child fields, such as BinaryOperator forcing off
            // parens when consecutive operators are the same, or Statement forcing off parens for child
            // expressions.
            field.DefaultFormat();

            // Force off newlines for child fields by default (such as for LocalDecls in for/foreach, etc)
            if (!field._formatFlags.HasFlag(FormatFlags.NewLinesSet))
                field._formatFlags &= ~FormatFlags.NewLineMask;
        }

        /// <summary>
        /// Default format the code object.
        /// </summary>
        protected internal virtual void DefaultFormat()
        {
            // Clear the Terminator flag by default
            _formatFlags &= ~FormatFlags.Terminator;

            // Default the newlines if they haven't been explicitly set
            if (!_formatFlags.HasFlag(FormatFlags.NewLinesSet))
                _formatFlags = ((_formatFlags & ~(FormatFlags.NewLineMask | FormatFlags.InfixNewLine)) | (IsFirstOnLineDefault ? (FormatFlags)1 : 0));
        }

        /// <summary>
        /// Determines if the code object appears as the first item on a line.
        /// </summary>
        public virtual bool IsFirstOnLine
        {
            get { return ((_formatFlags & FormatFlags.NewLineMask) != 0); }
            set
            {
                if (value)
                {
                    // Do nothing if it's already been explicitly set to greater than 1, otherwise
                    // explicitly set it to 1.
                    if (NewLines <= 1)
                        NewLines = 1;
                }
                else
                    NewLines = 0;
            }
        }

        /// <summary>
        /// The number of newlines preceeding the object (0 to N).
        /// </summary>
        public virtual int NewLines
        {
            get { return (int)(_formatFlags & FormatFlags.NewLineMask); }
            set
            {
                SetNewLines(value);
                _formatFlags |= FormatFlags.NewLinesSet;
            }
        }

        /// <summary>
        /// Special method to set the newline count without setting the NewLinesSet flag.
        /// </summary>
        public void SetNewLines(int value)
        {
            // If we're changing to 0, also change all prefix comments to 0
            if (_annotations != null && value == 0 && ((_formatFlags & FormatFlags.NewLineMask) != 0))
                SetFirstOnLineForNonEOLComments(_annotations, false);

            // Change the newline value, truncating at the maximum allowed value
            if (value > (int)FormatFlags.NewLineMask)
                value = (int)FormatFlags.NewLineMask;
            _formatFlags = ((_formatFlags & ~FormatFlags.NewLineMask) | (FormatFlags)value);
        }

        /// <summary>
        /// Set the newline flag for all non-EOL comments in the collection.
        /// </summary>
        public static void SetFirstOnLineForNonEOLComments(ChildList<Annotation> annotations, bool value)
        {
            foreach (Annotation annotation in annotations)
            {
                if (annotation is CommentBase && !annotation.IsPostfix && !annotation.IsInfix)
                {
                    CommentBase comment = (CommentBase)annotation;
                    if (!comment.IsEOL)
                        comment.IsFirstOnLine = value;
                }
            }
        }

        /// <summary>
        /// Determines if the newline count has been set on the code object.
        /// </summary>
        public bool IsNewLinesSet
        {
            get { return _formatFlags.HasFlag(FormatFlags.NewLinesSet); }
            set { SetFormatFlag(FormatFlags.NewLinesSet, value); }
        }

        /// <summary>
        /// Determines if the code object has no indentation.
        /// </summary>
        public bool HasNoIndentation
        {
            get { return _formatFlags.HasFlag(FormatFlags.NoIndentation); }
            set { SetFormatFlag(FormatFlags.NoIndentation, value); }
        }

        /// <summary>
        /// Determines if the 'grouping' (has parens or braces) status has been set.
        /// </summary>
        public bool IsGroupingSet
        {
            get { return _formatFlags.HasFlag(FormatFlags.GroupingSet); }
        }

        /// <summary>
        /// Determines if the code object has a terminator character.
        /// </summary>
        public virtual bool HasTerminator
        {
            get { return _formatFlags.HasFlag(FormatFlags.Terminator); }
            set { SetFormatFlag(FormatFlags.Terminator, value); }
        }

        /// <summary>
        /// Determines if the code object is generated.
        /// </summary>
        public bool IsGenerated
        {
            get { return _formatFlags.HasFlag(FormatFlags.Generated); }
            set { SetFormatFlag(FormatFlags.Generated, value); }
        }

        /// <summary>
        /// Rendering behavior flags (passed through rendering methods).
        /// </summary>
        [Flags]
        public enum RenderFlags : uint
        {
            /// <summary>No flags set.</summary>
            None = 0x00000000,

            /// <summary>Suppress indentation of the current block.</summary>
            NoBlockIndent = 0x00000001,

            /// <summary>Suppress parens if empty (used by NewOperators).</summary>
            NoParensIfEmpty = 0x00000002,

            /// <summary>Suppress rendering of EOL comments (used by WriteList).</summary>
            NoEOLComments = 0x00000004,

            /// <summary>Suppress the next newline because it's already been pre-rendered.</summary>
            SuppressNewLine = 0x00000008,

            /// <summary>Suppress type arguments when rendering a Type (used by TypeRef rendering).</summary>
            SuppressTypeArgs = 0x00000010,

            /// <summary>Object needs a space prefix if it's not the first thing on the line.</summary>
            PrefixSpace = 0x00000040,

            /// <summary>Object is a child prefix of another - the IsFirstOnLine flag actually means IsLastOnLine.</summary>
            IsPrefix = 0x00000080,

            /// <summary>Suppress the space suffix on a prefix object.</summary>
            NoSpaceSuffix = 0x00000100,

            /// <summary>Object is on the right side of a Dot operator.</summary>
            HasDotPrefix = 0x00000200,

            /// <summary>Render as a declaration (might differ from references).</summary>
            Declaration = 0x00001000,

            /// <summary>Rendering an attribute (hide "Attribute" suffix, hide parens if empty).</summary>
            Attribute = 0x00002000,

            /// <summary>Increase the indentation level for any future newlines.</summary>
            IncreaseIndent = 0x00004000,

            /// <summary>Render a terminator (after a statement or a ChildList).</summary>
            HasTerminator = 0x00008000,

            /// <summary>Suppress rendering of separators (commas) between items in a ChildList.</summary>
            NoItemSeparators = 0x00010000,

            /// <summary>Suppress rendering of post annoations (used by ChildList).</summary>
            NoPostAnnotations = 0x00020000,

            /// <summary>Suppress translations (used by DocText text rendering only).</summary>
            NoTranslations = 0x00040000,

            /// <summary>Do NOT increase the indentation level for any future newlines.</summary>
            NoIncreaseIndent = 0x00100000,

            // The following flags are passed through all child rendering calls without being automatically cleared:

            /// <summary>Update LineNumber and Column properties while rendering.</summary>
            UpdateLineCol = 0x00800000,

            /// <summary>Suppress brackets when rendering a TypeRef for an array type (used for NewArray with jagged arrays).</summary>
            SuppressBrackets = 0x01000000,

            /// <summary>Render comments in-line (using block style instead of EOL style).</summary>
            CommentsInline = 0x02000000,

            /// <summary>Render as description (no body, show full signature on references, etc.).</summary>
            Description = 0x04000000,

            /// <summary>Show any parent types of the type being rendered.</summary>
            ShowParentTypes = 0x08000000,

            /// <summary>Object being rendered is inside a documentation comment.</summary>
            InDocComment = 0x10000000,

            /// <summary>Suppress rendering of any prefix annotations on the current object (used to suppress them for the first item in a ChildList).</summary>
            NoPreAnnotations = 0x20000000,

            /// <summary>Suppress rendering of first/last newlines in doc comment content (used by DocComment classes).</summary>
            NoTagNewLines = 0x40000000,

            /// <summary>Format numerics in hex.</summary>
            FormatAsHex = 0x80000000,

            /// <summary>Mask of flags that propagate through all rendering calls.</summary>
            PassMask = UpdateLineCol | SuppressBrackets | CommentsInline | Description | ShowParentTypes | InDocComment | NoPreAnnotations | NoTagNewLines | FormatAsHex,

            /// <summary>Flags used during length determination for alignment purposes.</summary>
            LengthFlags = NoPreAnnotations | NoEOLComments | NoPostAnnotations
        }

        /// <summary>
        /// Render the type of the code object and its description as a string.
        /// </summary>
        /// <remarks>
        /// Only a description is rendered, without any Body - otherwise debugging sessions would be too slow
        /// as entire code object trees are rendered.
        /// </remarks>
        public override string ToString()
        {
            return GetType().Name + ": " + GetDescription();
        }

        /// <summary>
        /// Render the entire code object as a string, using LFs for newlines.
        /// </summary>
        /// <remarks>
        /// We don't do this in ToString(), because we don't want the debugger evaluating entire code object
        /// trees when displaying the values of variables.
        /// </remarks>
        public string AsString()
        {
            return AsText(RenderFlags.IncreaseIndent);
        }

#if DEBUG

        /// <summary>
        /// This property is just to make debugging easier.
        /// </summary>
        public string _AsString
        {
            get { return AsString(); }
        }

#endif

        /// <summary>
        /// True if the <see cref="CodeObject"/> is renderable.
        /// </summary>
        public virtual bool IsRenderable
        {
            get { return true; }
        }

        /// <summary>
        /// Get a short text description of the <see cref="CodeObject"/>.
        /// This is generally the shortest text representation that uniquely identifies objects, even if
        /// they have the same name, for example: type or return type, name, type parameters, parameters.
        /// </summary>
        public string GetDescription()
        {
            // Don't set the Description flag for DocComments, because we want to render tags if we're getting
            // the direct description of a DocComment object (but NOT for children DocComments of the target object).
            RenderFlags renderFlags = RenderFlags.ShowParentTypes | RenderFlags.IncreaseIndent;
            if (!(this is DocComment))
                renderFlags |= RenderFlags.Description;
            return AsText(renderFlags);
        }

        /// <summary>
        /// Update the line and column numbers according to the current positions in the <see cref="CodeWriter"/>,
        /// if the <see cref="RenderFlags.UpdateLineCol"/> flag is set.
        /// </summary>
        protected internal virtual void UpdateLineCol(CodeWriter writer, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.UpdateLineCol))
            {
                // Use this for testing that updated Line/Cols match the parsed values (on FullTest.cs):
                //if (writer.LineNumber != _lineNumber || writer.ColumnNumber != _columnNumber)
                //    Log.WriteLine("Line-Col: " + writer.LineNumber + "-" + writer.ColumnNumber + " != " + _lineNumber + "-" + _columnNumber);
                _lineNumber = writer.LineNumber;
                _columnNumber = (ushort)writer.ColumnNumber;
            }
        }

        /// <summary>
        /// Convert the code object to text using the specified flags and format (file or string).
        /// </summary>
        public string AsText(RenderFlags flags, bool isFileFormat, Stack<CodeWriter.AlignmentState> alignmentStateStack)
        {
            using (CodeWriter writer = new CodeWriter(false, IsGenerated))
            {
                if (alignmentStateStack != null)
                    writer.AlignmentStateStack = new Stack<CodeWriter.AlignmentState>(alignmentStateStack);
                try
                {
                    // Render the text into a string, suppressing any leading newline.
                    // Use CR/LFs for file format with a trailing newline, or only LFs for string format.
                    if (!isFileFormat)
                        writer.NewLine = "\n";
                    AsText(writer, flags | RenderFlags.SuppressNewLine);
                    if (isFileFormat)
                        writer.WriteLine();
                }
                catch (Exception ex)
                {
                    string message = Log.Exception(ex, "rendering");
                    writer.Write(message);
                }
                return writer.ToString();
            }
        }

        /// <summary>
        /// Convert the code object to text using the specified flags and format (file or string).
        /// </summary>
        public string AsText(RenderFlags flags, bool isFileFormat)
        {
            return AsText(flags, isFileFormat, null);
        }

        /// <summary>
        /// Convert the code object to text using the specified flags and format (file or string).
        /// </summary>
        public string AsText(RenderFlags flags)
        {
            return AsText(flags, false, null);
        }

        /// <summary>
        /// Convert the code object to text with a trailing newline, and using CR/LF pairs for newlines (file format).
        /// </summary>
        public string AsText()
        {
            return AsText(RenderFlags.None, true);
        }

        /// <summary>
        /// Determine the length of the code object if converted to a string using the specified flags.
        /// </summary>
        public int AsTextLength(RenderFlags flags, Stack<CodeWriter.AlignmentState> alignmentStateStack)
        {
            using (CodeWriter writer = new CodeWriter(true))
            {
                if (alignmentStateStack != null)
                    writer.AlignmentStateStack = new Stack<CodeWriter.AlignmentState>(alignmentStateStack);
                try
                {
                    // Render the text into a string, suppressing any leading newline.
                    // Use only LFs for string format.
                    writer.NewLine = "\n";
                    AsText(writer, flags | RenderFlags.SuppressNewLine);
                }
                catch
                { }
                return writer.ColumnNumber;
            }
        }

        /// <summary>
        /// Determine the length of the code object if converted to a string using the specified flags.
        /// </summary>
        public int AsTextLength(RenderFlags flags)
        {
            return AsTextLength(flags, null);
        }

        /// <summary>
        /// Determine the length of the code object if converted to a string using the specified flags.
        /// </summary>
        public int AsTextLength()
        {
            return AsTextLength(RenderFlags.LengthFlags, null);
        }

        public virtual void AsText(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(base.ToString());
        }

        protected virtual void AsTextBefore(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                AsTextAnnotations(writer, flags);
        }

        protected virtual void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.NoPostAnnotations))
                AsTextAnnotations(writer, AnnotationFlags.IsPostfix, flags);
        }

        protected void AsTextEvaluatedType(CodeWriter writer, SymbolicRef typeRef)
        {
            writer.Write(" { ");
            typeRef.AsText(writer, RenderFlags.Description | RenderFlags.ShowParentTypes);  // Description flag will show any constant value
            writer.Write(" }");
        }

        /// <summary>
        /// Check for alignment of any EOL comments.
        /// </summary>
        protected void CheckForAlignment(CodeWriter writer)
        {
            if (HasEOLComments && IsFirstOnLine && IsSingleLine)
            {
                BlockStatement parentBlockStatement = FindParent<BlockStatement>();
                if (parentBlockStatement != null)
                {
                    int[] columnWidths = writer.GetColumnWidths(parentBlockStatement.Body);
                    if (columnWidths != null)
                    {
                        int columnWidth = Enumerable.Last(columnWidths);
                        if (columnWidth > 0)
                        {
                            int padding = columnWidth - AsTextLength();
                            if (padding > 0)
                                writer.Write(new string(' ', padding));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Render all regular (non-EOL, non-Infix, non-Postfix, non-Message) annotations (comments, attributes, compiler directives).
        /// </summary>
        public void AsTextAnnotations(CodeWriter writer, RenderFlags flags)
        {
            if (_annotations != null && !flags.HasFlag(RenderFlags.Description))
            {
                flags |= RenderFlags.IsPrefix;
                foreach (Annotation annotation in _annotations)
                {
                    if (!annotation.IsEOL && !annotation.IsInfix && !annotation.IsPostfix && !(annotation is Message))
                        annotation.AsText(writer, flags | (annotation.IsFirstOnLine ? 0 : RenderFlags.CommentsInline));
                }
            }
        }

        /// <summary>
        /// Render all EOL comments.
        /// </summary>
        public void AsTextEOLComments(CodeWriter writer, RenderFlags flags)
        {
            if (_annotations != null && !flags.HasFlag(RenderFlags.NoEOLComments) && !flags.HasFlag(RenderFlags.Description))
            {
                foreach (Annotation annotation in _annotations)
                {
                    if (annotation is Comment && annotation.IsEOL && !annotation.IsInfix)
                        writer.WritePendingEOLComment((Comment)annotation, flags);
                }
            }
        }

        /// <summary>
        /// Render all Infix EOL comments.
        /// </summary>
        public void AsTextInfixEOLComments(CodeWriter writer, RenderFlags flags)
        {
            if (_annotations != null && !flags.HasFlag(RenderFlags.NoEOLComments) && !flags.HasFlag(RenderFlags.Description))
            {
                foreach (Annotation annotation in _annotations)
                {
                    if (annotation is Comment && annotation.IsEOL && annotation.IsInfix)
                        writer.WritePendingEOLComment((Comment)annotation, flags);
                }
            }
        }

        /// <summary>
        /// Render all Infix comments with the specified mask.
        /// </summary>
        public void AsTextInfixComments(CodeWriter writer, AnnotationFlags infixMask, RenderFlags flags)
        {
            if (_annotations != null && !flags.HasFlag(RenderFlags.NoEOLComments) && !flags.HasFlag(RenderFlags.Description))
            {
                foreach (Annotation annotation in _annotations)
                {
                    if (annotation is Comment)
                    {
                        if (infixMask != 0 ? (annotation.AnnotationFlags & AnnotationFlags.InfixMask) == infixMask : annotation.IsInfix)
                        {
                            if (flags.HasFlag(RenderFlags.PrefixSpace) && !annotation.IsEOL)
                                writer.Write(" ");
                            writer.WritePendingEOLComment((Comment)annotation, flags);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Render all specified Infix or Postfix annotations (comments, compiler directives).
        /// </summary>
        public void AsTextAnnotations(CodeWriter writer, AnnotationFlags positionFlag, RenderFlags flags)
        {
            if (_annotations != null && !flags.HasFlag(RenderFlags.Description))
            {
                foreach (Annotation annotation in _annotations)
                {
                    if (annotation.AnnotationFlags.HasFlag(positionFlag))
                    {
                        if (annotation is Comment)
                            writer.WritePendingEOLComment((Comment)annotation, flags);
                        else
                            annotation.AsText(writer, RenderFlags.None);
                    }
                }
            }
        }
    }
}