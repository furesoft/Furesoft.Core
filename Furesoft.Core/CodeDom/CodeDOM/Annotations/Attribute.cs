// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.Reflection;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents metadata associated with a <see cref="CodeObject"/>.
    /// </summary>
    /// <remarks>
    /// Format: [target: name(arg, name=arg, ...), ...]
    /// Attributes can appear on the following declarations, with targets as shown:
    /// 
    /// Declaration                      Targets
    /// -----------                      -------
    /// (global)                         assembly, module (no default!)
    /// class, struct, interface, enum   type
    /// delegate                         type, return
    /// method, operator, ctor, dtor     method, return
    /// parameter (incl type params)     param
    /// field, enum member               field
    /// property, indexer                property
    /// property - getter                method, return
    /// property - setter                method, param, return
    /// event - field                    event, field, method
    /// event - property                 event, property
    /// event - add, remove              method, param
    /// 
    /// Global attributes must appear at the top level of a file, after using directives and before
    /// namespace declarations.  The first target is the default if none is specified, except for
    /// global attributes (which have no default target).
    /// </remarks>
    public class Attribute : Annotation
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The name suffix used for attribute classes.
        /// </summary>
        public const string NameSuffix = "Attribute";

        #endregion

        #region /* FIELDS */

        protected AttributeTarget _target;

        /// <summary>
        /// One or more <see cref="Expression"/> objects, which in valid code should each be either a <see cref="Call"/> with
        /// an <see cref="Expression"/> that evaluates to a <see cref="ConstructorRef"/> of a valid attribute type (a class
        /// derived from <see cref="System.Attribute"/>, with an <see cref="System.AttributeUsageAttribute"/> attribute), or it can also be
        /// just a <see cref="ConstructorRef"/> (if no parens are used on it).  Of course, it can also be an <see cref="UnresolvedRef"/>
        /// in either case.
        /// </summary>
        protected ChildList<Expression> _attributeExpressions;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="Attribute"/>.
        /// </summary>
        public Attribute(AttributeTarget target, params Expression[] attributeExpressions)
        {
            _target = target;
            CreateAttributeExpressions().AddRange(attributeExpressions);
            foreach (Expression attributeCall in attributeExpressions)
                attributeCall.FormatAsArgument();
        }

        /// <summary>
        /// Create an <see cref="Attribute"/>.
        /// </summary>
        public Attribute(params Expression[] attributeExpressions)
            : this(AttributeTarget.None, attributeExpressions)
        { }

        /// <summary>
        /// Create an <see cref="Attribute"/>.
        /// </summary>
        public Attribute(AttributeTarget target, params TypeRefBase[] typeRefBases)
        {
            _target = target;
            foreach (TypeRefBase typeRefBase in typeRefBases)
                CreateAttributeExpressions().Add(ConstructorRef.Find(typeRefBase));
        }

        /// <summary>
        /// Create an <see cref="Attribute"/>.
        /// </summary>
        public Attribute(params TypeRefBase[] typeRefBases)
            : this(AttributeTarget.None, typeRefBases)
        { }

        /// <summary>
        /// Create an <see cref="Attribute"/>.
        /// </summary>
        public Attribute(AttributeTarget target, SymbolicRef constructorRef, params Expression[] arguments)
            : this(target, (arguments.Length > 0 ? (Expression)new Call(constructorRef, arguments) : constructorRef))
        { }

        /// <summary>
        /// Create an <see cref="Attribute"/>.
        /// </summary>
        public Attribute(SymbolicRef constructorRef, params Expression[] arguments)
            : this(AttributeTarget.None, constructorRef, arguments)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The list of attribute <see cref="Expression"/>s.
        /// </summary>
        public ChildList<Expression> AttributeExpressions
        {
            get { return _attributeExpressions; }
        }

        /// <summary>
        /// True if there are any attribute <see cref="Expression"/>s.
        /// </summary>
        public bool HasAttributeExpressions
        {
            get { return (_attributeExpressions != null && _attributeExpressions.Count > 0); }
        }

        /// <summary>
        /// The <see cref="AttributeTarget"/>.
        /// </summary>
        public AttributeTarget Target
        {
            get { return _target; }
        }

        /// <summary>
        /// True if the Attribute is global (not associated with a following code object, but rather the Assembly or Module).
        /// </summary>
        public bool IsGlobal
        {
            get { return (_target == AttributeTarget.Assembly || _target == AttributeTarget.Module); }
        }

        /// <summary>
        /// True if the annotation should be listed at the <see cref="CodeUnit"/> and <see cref="Solution"/> levels (for display in an output window).
        /// </summary>
        public override bool IsListed
        {
            // Global attributes are marked as listed in order to propagate them up to the Project,
            // but they won't actually be listed in the Messages list.
            get { return IsGlobal; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Get the list of attribute <see cref="Expression"/>s, or return the existing one.
        /// </summary>
        public ChildList<Expression> CreateAttributeExpressions()
        {
            if (_attributeExpressions == null)
                _attributeExpressions = new ChildList<Expression>(this);
            return _attributeExpressions;
        }

        /// <summary>
        /// Find the first attribute expression (Call or ConstructorRef) with the specified name.
        /// </summary>
        /// <returns>The expression if found, otherwise <c>null</c>.</returns>
        public Expression FindAttributeExpression(string attributeName)
        {
            if (_attributeExpressions != null)
            {
                foreach (Expression expression in _attributeExpressions)
                {
                    // The expression might be a ConstructorRef or an UnresolvedRef, or it might be a Call that
                    // has an invoked expression of one of those types.
                    SymbolicRef symbolicRef = (expression is Call ? ((Call)expression).Expression.SkipPrefixes() as SymbolicRef : expression as SymbolicRef);

                    // Check if the name matches, with or without an "Attribute" suffix
                    if (symbolicRef != null && (symbolicRef.Name == attributeName
                        || symbolicRef.Name + NameSuffix == attributeName || symbolicRef.Name == attributeName + NameSuffix))
                        return expression;
                }
            }
            return null;
        }

        /// <summary>
        /// Remove the first attribute expression with the specified name.
        /// </summary>
        /// <returns><c>true</c> if found and removed, otherwise <c>false</c>.</returns>
        public bool RemoveAttributeExpression(string attributeName)
        {
            if (_attributeExpressions != null)
            {
                for (int i = _attributeExpressions.Count - 1; i >= 0; --i)
                {
                    // The expression might be a ConstructorRef or an UnresolvedRef, or it might be a Call that
                    // has an invoked expression of one of those types.
                    Expression expression = _attributeExpressions[i];
                    SymbolicRef symbolicRef = (expression is Call ? ((Call)expression).Expression.SkipPrefixes() as SymbolicRef : expression as SymbolicRef);

                    // Check if the name matches, with or without an "Attribute" suffix
                    if (symbolicRef != null && (symbolicRef.Name == attributeName
                        || symbolicRef.Name + NameSuffix == attributeName || symbolicRef.Name == attributeName + NameSuffix))
                    {
                        _attributeExpressions.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Attribute clone = (Attribute)base.Clone();
            clone._attributeExpressions = ChildListHelpers.Clone(_attributeExpressions, clone);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the start of an attribute.
        /// </summary>
        public const string ParseTokenStart = "[";

        /// <summary>
        /// The token used to parse an attribute target.
        /// </summary>
        public const string ParseTokenTarget = ":";

        /// <summary>
        /// The token used to parse the end of an attribute.
        /// </summary>
        public const string ParseTokenEnd = "]";

        internal static void AddParsePoints()
        {
            // Use a parse-priority of 300 (IndexerDecl uses 0, TypeRef uses 100, Index uses 200)
            // Attributes can appear in many places (see top of this file), so we won't restrict their
            // scope for parsing (but static analysis will flag them if they aren't in a valid place.
            Parser.AddParsePoint(ParseTokenStart, 300, Parse);
        }

        /// <summary>
        /// Parse an <see cref="Attribute"/>.
        /// </summary>
        public static Attribute Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Attribute(parser, parent);
        }

        protected Attribute(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past '['

            if (parser.PeekNextTokenText() == ParseTokenTarget)
                _target = AttributeTargetHelpers.Parse(parser);

            // Parse attribute expressions (will parse to a Call if parens are used, otherwise a ConstructorRef)
            _attributeExpressions = Expression.ParseList(parser, this, ParseTokenEnd);

            ParseExpectedToken(parser, ParseTokenEnd);  // Move past ']'
        }

        /// <summary>
        /// Parse multiple attributes.
        /// </summary>
        public static void ParseAttributes(Parser parser, CodeObject parent)
        {
            while (parser.TokenText == ParseTokenStart)
            {
                Attribute attribute = new Attribute(parser, parent);
                parent.AttachAnnotation(attribute, true);
            }
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations, or if
            // it's not another attribute declaration.
            if (HasFirstOnLineAnnotations || !(previous is Attribute))
                return 2;
            return 1;
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_attributeExpressions == null || _attributeExpressions.Count == 0
                || (!_attributeExpressions[0].IsFirstOnLine && _attributeExpressions.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_attributeExpressions != null && _attributeExpressions.Count > 0)
                {
                    if (value)
                        _attributeExpressions[0].IsFirstOnLine = false;
                    _attributeExpressions.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            int newLines = NewLines;
            bool isPrefix = flags.HasFlag(RenderFlags.IsPrefix);
            if (!isPrefix && newLines > 0)
            {
                if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                    writer.WriteLines(newLines);
            }
            else if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);
            writer.Write(ParseTokenStart);

            // Increase the indent level for any newlines that occur within the declaration if the flag is set
            bool increaseIndent = flags.HasFlag(RenderFlags.IncreaseIndent);
            if (increaseIndent)
                writer.BeginIndentOnNewLine(this);

            if (_target != AttributeTarget.None)
                writer.Write(AttributeTargetHelpers.AsString(_target) + ParseTokenTarget + " ");
            writer.WriteList(_attributeExpressions, passFlags | RenderFlags.Attribute, this);

            if (increaseIndent)
                writer.EndIndentation(this);

            writer.Write(ParseTokenEnd);
            AsTextEOLComments(writer, flags);
            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));

            if (isPrefix)
            {
                // If this object is rendered as a child prefix object of another, then any whitespace is
                // rendered here *after* the object instead of before it.
                if (newLines > 0)
                    writer.WriteLines(newLines);
                else
                    writer.Write(" ");
            }
        }

        /// <summary>
        /// Helper method to convert a collection of Attributes to text.
        /// </summary>
        public static void AsTextAttributes(CodeWriter writer, ChildList<Attribute> attributes, RenderFlags flags)
        {
            if (attributes != null && attributes.Count > 0)
            {
                flags &= ~RenderFlags.Description;  // Don't pass description flag through
                foreach (Attribute attrDecl in attributes)
                    attrDecl.AsText(writer, flags);
            }
        }

        public static void AsTextAttributes(CodeWriter writer, MemberInfo memberInfo, AttributeTarget attributeTarget)
        {
            // Use the static method to get the attributes so that this works with types from reflection-only assemblies
            AsTextAttributes(writer, CustomAttributeData.GetCustomAttributes(memberInfo), attributeTarget);
        }

        public static void AsTextAttributes(CodeWriter writer, MemberInfo memberInfo)
        {
            AsTextAttributes(writer, memberInfo, AttributeTarget.None);
        }

        public static void AsTextAttributes(CodeWriter writer, ParameterInfo parameterInfo, AttributeTarget attributeTarget)
        {
            // Use the static method to get the attributes so that this works with types from reflection-only assemblies
            AsTextAttributes(writer, CustomAttributeData.GetCustomAttributes(parameterInfo), attributeTarget);
        }

        public static void AsTextAttributes(CodeWriter writer, ParameterInfo parameterInfo)
        {
            AsTextAttributes(writer, parameterInfo, AttributeTarget.None);
        }

        protected static void AsTextAttributes(CodeWriter writer, IList<CustomAttributeData> attributes, AttributeTarget attributeTarget)
        {
            int count = 0;
            foreach (CustomAttributeData attribute in attributes)
            {
                Type declaringType = attribute.Constructor.DeclaringType;
                if (declaringType != null)
                {
                    string name = declaringType.Name;
                    if (count > 0)
                        writer.Write(" ");
                    writer.Write(ParseTokenStart);
                    if (attributeTarget != AttributeTarget.None)
                        writer.Write(AttributeTargetHelpers.AsString(attributeTarget) + ": ");
                    if (name.EndsWith(NameSuffix))
                        name = name.Substring(0, name.Length - NameSuffix.Length);
                    writer.Write(name);

                    if (attribute.ConstructorArguments.Count > 0)
                    {
                        writer.Write(ParameterDecl.ParseTokenStart);
                        foreach (CustomAttributeTypedArgument argument in attribute.ConstructorArguments)
                            AsTextValue(writer, argument.Value);
                        writer.Write(ParameterDecl.ParseTokenEnd);
                    }
                    else if (attribute.NamedArguments != null && attribute.NamedArguments.Count > 0)
                    {
                        writer.Write(ParameterDecl.ParseTokenStart);
                        foreach (CustomAttributeNamedArgument argument in attribute.NamedArguments)
                        {
                            writer.Write(argument.MemberInfo.Name + " = ");
                            AsTextValue(writer, argument.TypedValue.Value);
                        }
                        writer.Write(ParameterDecl.ParseTokenEnd);
                    }

                    writer.Write(ParseTokenEnd);
                    ++count;
                }
            }
            if (count > 0)
                writer.WriteLine();
        }

        protected static void AsTextAttributes(CodeWriter writer, IList<CustomAttributeData> attributes)
        {
            AsTextAttributes(writer, attributes, AttributeTarget.None);
        }

        private static void AsTextValue(CodeWriter writer, object value)
        {
            if (value is Type)
            {
                TypeOf typeOf = new TypeOf(TypeRef.CreateNested((Type)value));
                typeOf.AsText(writer, RenderFlags.None);
            }
            else
            {
                if (value is Array)
                {
                    Initializer initializer = new Initializer();
                    foreach (object element in (Array)value)
                        initializer.CreateExpressions().Add(new Literal(element));
                    initializer.AsText(writer, RenderFlags.None);
                }
                else
                {
                    Literal literal = new Literal(value);
                    literal.AsText(writer, RenderFlags.None);
                }
            }
        }

        #endregion
    }

    #region /* ATTRIBUTE TARGET */

    /// <summary>
    /// Valid attribute target types.
    /// </summary>
    public enum AttributeTarget { None, Assembly, Module, Type, Method, Return, Param, Field, Property, Event }

    /// <summary>
    /// Static helper methods for the AttributeTarget enum.
    /// </summary>
    public static class AttributeTargetHelpers
    {
        // Setup maps of name-to-enum and enum-to-name.
        static AttributeTargetHelpers()
        {
            string[] names = Enum.GetNames(typeof(AttributeTarget));
            Array values = Enum.GetValues(typeof(AttributeTarget));
            int i = 0;
            foreach (AttributeTarget value in values)
            {
                string name = names[i].ToLower();
                _nameToTarget.Add(name, value);
                _targetToName.Add(value, name);
                ++i;
            }
        }

        private static readonly Dictionary<string, AttributeTarget> _nameToTarget = new Dictionary<string, AttributeTarget>();
        private static readonly Dictionary<AttributeTarget, string> _targetToName = new Dictionary<AttributeTarget, string>();

        /// <summary>
        /// Format Target as a string.
        /// </summary>
        public static string AsString(AttributeTarget target)
        {
            string result = "";
            string name;
            if (_targetToName.TryGetValue(target, out name))
                result = name;
            return result;
        }

        /// <summary>
        /// Parse token into Target enum.
        /// </summary>
        public static AttributeTarget Parse(Parser parser)
        {
            AttributeTarget target = 0;
            AttributeTarget temp;
            if (_nameToTarget.TryGetValue(parser.TokenText, out temp))
                target = temp;
            parser.NextToken();  // Move past target
            parser.NextToken();  // Move past ':'
            return target;
        }
    }

    #endregion
}
