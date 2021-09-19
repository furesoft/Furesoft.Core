// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base
{
    /// <summary>
    /// The common base class of all user-defined types with optionally declared base types (<see cref="ClassDecl"/>,
    /// <see cref="StructDecl"/>, <see cref="InterfaceDecl"/>, and <see cref="EnumDecl"/>).
    /// </summary>
    public abstract class BaseListTypeDecl : TypeDecl
    {
        #region /* FIELDS */

        /// <summary>
        /// List of base types, each of which is an <see cref="Expression"/> that must evaluate to a <see cref="TypeRef"/> in valid code.
        /// </summary>
        protected ChildList<Expression> _baseTypes;

        #endregion

        #region /* CONSTRUCTORS */

        protected BaseListTypeDecl(string name, Modifiers modifiers)
            : base(name, modifiers)
        { }

        protected BaseListTypeDecl(string name, Modifiers modifiers, params TypeParameter[] typeParameters)
            : base(name, modifiers, typeParameters)
        { }

        protected BaseListTypeDecl(string name, Modifiers modifiers, params Expression[] baseTypes)
            : base(name, modifiers)
        {
            CreateBaseTypes().AddRange(baseTypes);
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The list of base types.
        /// </summary>
        public ChildList<Expression> BaseTypes
        {
            get { return _baseTypes; }
        }

        /// <summary>
        /// True if there are any base types.
        /// </summary>
        public bool HasBaseTypes
        {
            get { return (_baseTypes != null && _baseTypes.Count > 0); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create the list of base type <see cref="Expression"/>s, or return the existing one.
        /// </summary>
        public ChildList<Expression> CreateBaseTypes()
        {
            if (_baseTypes == null)
                _baseTypes = new ChildList<Expression>(this);
            return _baseTypes;
        }

        /// <summary>
        /// Add one or more base type <see cref="Expression"/>s.
        /// </summary>
        public void AddBaseTypes(params Expression[] baseTypes)
        {
            CreateBaseTypes().AddRange(baseTypes);
        }

        /// <summary>
        /// Get all base type expressions, including those of other parts if this is a partial type.
        /// </summary>
        /// <returns>The list of base type expressions, which might be either null or empty if there aren't any.</returns>
        public List<Expression> GetAllBaseTypes()
        {
            // Get the list of base types, building it if this is a partial type
            if (IsPartial)
            {
                List<Expression> baseTypes = new List<Expression>();
                if (_baseTypes != null)
                    baseTypes.AddRange(_baseTypes);
                foreach (TypeDecl otherPart in GetOtherParts())
                {
                    if (otherPart is BaseListTypeDecl)
                    {
                        BaseListTypeDecl baseListTypeDecl = (BaseListTypeDecl)otherPart;
                        if (baseListTypeDecl.HasBaseTypes)
                            baseTypes.AddRange(baseListTypeDecl.BaseTypes);
                    }
                }
                return baseTypes;
            }
            return _baseTypes;
        }

        /// <summary>
        /// Get the method with the specified name and parameter types.
        /// </summary>
        public override MethodRef GetMethod(string name, params TypeRefBase[] parameterTypes)
        {
            // Look for the method in the type declaration
            MethodDeclBase found = GetMethod<MethodDeclBase>(name, parameterTypes);
            if (found != null)
                return (MethodRef)found.CreateRef();

            // Look for the method in any base types
            List<Expression> baseTypes = GetAllBaseTypes();
            if (baseTypes != null)
            {
                foreach (Expression baseTypeExpression in baseTypes)
                {
                    TypeRef typeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                    if (typeRef != null)
                    {
                        MethodRef methodRef = typeRef.GetMethod(name, parameterTypes);
                        if (methodRef != null)
                            return methodRef;
                    }
                }
            }

            // Finally, look for the method in the 'object' base type
            return TypeRef.ObjectRef.GetMethod(name, parameterTypes);
        }

        /// <summary>
        /// Get all methods with the specified name.
        /// </summary>
        public override void GetMethods(string name, bool searchBaseClasses, NamedCodeObjectGroup results)
        {
            FindInAllParts<MethodDeclBase>(name, results);
            if (searchBaseClasses)
            {
                List<Expression> baseTypes = GetAllBaseTypes();
                if (baseTypes != null)
                {
                    foreach (Expression baseTypeExpression in baseTypes)
                    {
                        TypeRef typeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                        if (typeRef != null)
                            typeRef.GetMethods(name, true, results);
                    }
                }
            }
        }

        /// <summary>
        /// Get the property with the specified name.
        /// </summary>
        public override PropertyRef GetProperty(string name)
        {
            // Look for the property in the type declaration
            NamedCodeObjectGroup found = new NamedCodeObjectGroup();
            FindInAllParts<PropertyDecl>(name, found);
            if (found.Count > 0)
                return (PropertyRef)((PropertyDecl)found[0]).CreateRef();

            // Look for the property in any base types
            List<Expression> baseTypes = GetAllBaseTypes();
            if (baseTypes != null)
            {
                foreach (Expression baseTypeExpression in baseTypes)
                {
                    TypeRef typeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                    if (typeRef != null)
                    {
                        PropertyRef propertyRef = typeRef.GetProperty(name);
                        if (propertyRef != null)
                            return propertyRef;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determine if the type is assignable from the specified type.
        /// </summary>
        public override bool IsAssignableFrom(TypeRef typeRef)
        {
            if (typeRef == null)
                return false;

            TypeRef thisTypeRef = CreateRef();
            return (typeRef.IsSameRef(thisTypeRef) || typeRef.IsSubclassOf(thisTypeRef) || (typeRef.IsImplementationOf(thisTypeRef)));
        }

        /// <summary>
        /// Determine if the type implements the specified interface type.
        /// </summary>
        public override bool IsImplementationOf(TypeRef interfaceTypeRef)
        {
            List<Expression> baseTypes = GetAllBaseTypes();
            if (baseTypes != null)
            {
                // Check all interfaces implemented directly by the type declaration
                foreach (Expression baseTypeExpression in baseTypes)
                {
                    TypeRef baseTypeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                    if (baseTypeRef != null && baseTypeRef.IsInterface && baseTypeRef.IsSameRef(interfaceTypeRef))
                        return true;
                }
                // If we didn't find a match yet, search all base types/interfaces for implemented interfaces
                foreach (Expression baseTypeExpression in baseTypes)
                {
                    TypeRef baseTypeRef = baseTypeExpression.SkipPrefixes() as TypeRef;
                    if (baseTypeRef != null)
                    {
                        if (baseTypeRef.IsImplementationOf(interfaceTypeRef))
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
            BaseListTypeDecl clone = (BaseListTypeDecl)base.Clone();
            clone._baseTypes = ChildListHelpers.Clone(_baseTypes, clone);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = ":";

        protected BaseListTypeDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected void ParseBaseTypeList(Parser parser)
        {
            // Check for compiler directives, storing them as infix annotations on the parent
            Block.ParseCompilerDirectives(parser, this, AnnotationFlags.IsInfix1);

            // Parse the base-type list (if any)
            if (parser.TokenText == ParseToken)
            {
                Token lastHeaderToken = parser.LastToken;
                parser.NextToken();  // Move past the ':'
                bool isFirstOnLine = parser.LastToken.IsFirstOnLine;

                _baseTypes = Expression.ParseList(parser, this, null);
                if (_baseTypes != null && _baseTypes.Count > 0)
                {
                    _baseTypes[0].IsFirstOnLine = isFirstOnLine;
                    // Move any regular comments from before the ':' to the first object in the list
                    _baseTypes[0].MoveComments(lastHeaderToken);
                }
            }
        }

        /// <summary>
        /// Move any trailing post annotations on the last base type to the first constraint (if any) as prefix annotations.
        /// </summary>
        protected void AdjustBaseTypePostComments()
        {
            if (_baseTypes != null)
            {
                int baseTypes = _baseTypes.Count;
                if (baseTypes > 0 && _baseTypes[baseTypes - 1].HasPostAnnotations && HasConstraintClauses)
                {
                    ChildList<Annotation> annotations = _baseTypes[baseTypes - 1].Annotations;
                    for (int i = 0; i < annotations.Count;)
                    {
                        Annotation annotation = annotations[i];
                        if (annotation.IsPostfix)
                        {
                            annotation.IsPostfix = false;
                            annotations.RemoveAt(i);
                            if (annotation.IsListed)
                                NotifyListedAnnotationRemoved(annotation);
                            ConstraintClauses[0].AttachAnnotation(annotation);
                            continue;
                        }
                        ++i;
                    }
                    if (annotations.Count == 0)
                        _baseTypes[baseTypes - 1].Annotations = null;
                }
            }
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_baseTypes == null || _baseTypes.Count == 0 || (!_baseTypes[0].IsFirstOnLine && _baseTypes.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _baseTypes != null && _baseTypes.Count > 0)
                {
                    _baseTypes[0].IsFirstOnLine = false;
                    _baseTypes.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            if (!HasBaseTypes)
                base.AsTextSuffix(writer, flags);
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            AsTextAnnotations(writer, AnnotationFlags.IsInfix1, flags);

            if (HasBaseTypes)
            {
                // Increase the indent level for any newlines that occur within the base list unless specifically told not to
                bool increaseIndent = !flags.HasFlag(RenderFlags.NoIncreaseIndent);
                if (increaseIndent)
                    writer.BeginIndentOnNewLine(this);

                RenderFlags passFlags = (flags & RenderFlags.PassMask);
                if (_baseTypes[0].IsFirstOnLine)
                {
                    // If the first base type is on a new line, create a new line now for the ':'
                    writer.WriteLine();
                    passFlags |= RenderFlags.SuppressNewLine;
                }
                else
                    writer.Write(" ");

                // Render any prefix annotations for the first item in the list before the ':'
                _baseTypes[0].AsTextAnnotations(writer, flags);
                writer.Write(ParseToken + " ");
                writer.WriteList(_baseTypes, passFlags | RenderFlags.NoPreAnnotations | (HasConstraintClauses ? 0 : RenderFlags.HasTerminator), this);

                // Reset the indent level
                if (increaseIndent)
                    writer.EndIndentation(this);
            }

            base.AsTextAfter(writer, flags);
        }

        #endregion
    }
}
