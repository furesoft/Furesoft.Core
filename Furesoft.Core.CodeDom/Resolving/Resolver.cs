// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Collections.Generic;

using Nova.CodeDOM;
using Nova.Utilities;
using Attribute = Nova.CodeDOM.Attribute;

namespace Nova.Resolving
{
    /// <summary>
    /// Used to resolve an <see cref="UnresolvedRef"/>.
    /// </summary>
    public class Resolver
    {
        /// <summary>
        /// Determines whether or not exact matching is enforced when only one possible match is found for an <see cref="UnresolvedRef"/>.
        /// By default, an exact match is required, but this can be set to false to allow a match if there is only one possibility.
        /// </summary>
        public static bool ExactMatching = true;

        /// <summary>
        /// The total number of resolve attempts for the current pass.
        /// </summary>
        public static int ResolveAttempts;

        /// <summary>
        /// The total number of resolve failures for the current pass.
        /// </summary>
        public static int ResolveFailures;

        /// <summary>
        /// Indicates if a complete match exists.
        /// </summary>
        /// <remarks>This field is basically a copy of the same field in the associated UnresolvedRef (for performance).</remarks>
        protected bool _hasCompleteMatch;

        /// <summary>
        /// The delegate used to find valid object types for the target category.
        /// </summary>
        protected Func<object, bool> _isValidCategory;

        /// <summary>
        /// The parent expression of the unresolved reference (if any).
        /// </summary>
        protected Expression _parentExpression;

        /// <summary>
        /// The targeted object category.
        /// </summary>
        /// <remarks>This field is basically a copy of the same field in the associated UnresolvedRef (for performance).</remarks>
        protected ResolveCategory _resolveCategory;

        /// <summary>
        /// Special flags controlling the symbol resolution.
        /// </summary>
        protected ResolveFlags _resolveFlags;

        /// <summary>
        /// The UnresolvedRef being resolved.
        /// </summary>
        protected UnresolvedRef _unresolvedRef;

        /// <summary>
        /// Create a Resolver instance for the specified <see cref="UnresolvedRef"/>.
        /// </summary>
        /// <param name="unresolvedRef">The UnresolvedRef to be resolved.</param>
        /// <param name="resolveCategory">The ResolveCategory for the UnresolvedRef.</param>
        /// <param name="flags">Any ResolveFlags to be used.</param>
        public Resolver(UnresolvedRef unresolvedRef, ResolveCategory resolveCategory, ResolveFlags flags)
        {
            _unresolvedRef = unresolvedRef;
            _resolveCategory = resolveCategory;
            _resolveFlags = flags;
        }

        static Resolver()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            CodeObject.ForceReference();
        }

        /// <summary>
        /// True if a complete match has been found.
        /// </summary>
        public bool HasCompleteMatch
        {
            get { return _hasCompleteMatch; }
        }

        /// <summary>
        /// Returns true if the reference being resolved is in a DocCodeRefBase and has a Method category.
        /// </summary>
        public bool IsDocCodeRefToMethod
        {
            get { return (_resolveFlags.HasFlag(ResolveFlags.InDocCodeRef) && _resolveCategory == ResolveCategory.Method); }
        }

        /// <summary>
        /// Returns true if the reference being resolved has type arguments and the target category is for a type or constructor, otherwise false.
        /// </summary>
        public bool IsGenericTypeOrConstructor
        {
            get { return (_unresolvedRef.HasTypeArguments && ResolveCategoryHelpers.IsTypeOrConstructor[(int)_resolveCategory]); }
        }

        /// <summary>
        /// True if we are looking for a method.
        /// </summary>
        public bool IsMethodCategory
        {
            get { return ResolveCategoryHelpers.IsMethod[(int)_resolveCategory]; }
        }

        /// <summary>
        /// The <see cref="ResolveCategory"/> for the resolve attempt.
        /// </summary>
        public ResolveCategory ResolveCategory
        {
            get { return _resolveCategory; }
        }

        /// <summary>
        /// The <see cref="ResolveFlags"/> for the resolve attempt.
        /// </summary>
        public ResolveFlags ResolveFlags
        {
            get { return _resolveFlags; }
        }

        /// <summary>
        /// The number of type arguments on the <see cref="UnresolvedRef"/> being resolved.
        /// </summary>
        public int TypeArgumentCount
        {
            get { return _unresolvedRef.TypeArgumentCount; }
        }

        /// <summary>
        /// The <see cref="UnresolvedRef"/> being resolved.
        /// </summary>
        public UnresolvedRef UnresolvedRef
        {
            get { return _unresolvedRef; }
        }

        /// <summary>
        /// Returns true if there are any matches that aren't internal types imported from other assemblies/projects.
        /// </summary>
        public bool HasMatchesOtherThanImportedInternalTypes()
        {
            foreach (MatchCandidate candidate in _unresolvedRef.Matches)
            {
                object obj = candidate.Object;
                if (obj is TypeDecl)
                {
                    TypeDecl typeDecl = (TypeDecl)obj;
                    if (!typeDecl.IsInternal || typeDecl.FindParent<Project>() == _unresolvedRef.FindParent<Project>())
                        return true;
                }
                else if (obj is TypeDefinition)
                {
                    TypeDefinition typeDefinition = (TypeDefinition)obj;
                    if (!TypeDefinitionUtil.IsInternal(typeDefinition))
                        return true;
                }
                else if (obj is Type)
                {
                    Type type = (Type)obj;
                    if (!TypeUtil.IsInternal(type))
                        return true;
                }
                else
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if the target category is only valid at-or-below the MethodDecl level.
        /// </summary>
        public bool IsMethodLevelCategory()
        {
            return ResolveCategoryHelpers.IsMethodLevel[(int)_resolveCategory];
        }

        /// <summary>
        /// Determine if the target category is only valid at-or-below the Type level.
        /// </summary>
        public bool IsTypeLevelCategory()
        {
            return ResolveCategoryHelpers.IsTypeLevel[(int)_resolveCategory];
        }

        /// <summary>
        /// Attempt to resolve the associated UnresolvedRef object.
        /// </summary>
        /// <returns>The new reference object if resolved, otherwise the original UnresolvedRef.</returns>
        public SymbolicRef Resolve()
        {
            SymbolicRef resultRef;
            try
            {
                if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                    ++ResolveAttempts;

                string name = (string)_unresolvedRef.Reference;
                switch (_resolveCategory)
                {
                    case ResolveCategory.Type:
                        resultRef = ResolveSpecific(name, "Type name", "type names",
                            delegate (object obj) { return (obj is ITypeDecl && (!(obj is Alias) || ((Alias)obj).IsType)) || obj is TypeReference || obj is Type; });
                        break;

                    case ResolveCategory.Namespace:
                        resultRef = ResolveSpecific(name, "Namespace name", "namespace names", delegate (object obj) { return obj is Namespace || (obj is Alias && ((Alias)obj).IsNamespace); });
                        break;

                    case ResolveCategory.NamespaceOrType:
                        resultRef = ResolveSpecific(name, "Namespace or Type name", "namespace or type names",
                            delegate (object obj) { return obj is Namespace || obj is ITypeDecl || obj is TypeDefinition || obj is Type; });
                        break;

                    case ResolveCategory.Interface:
                        resultRef = ResolveSpecific(name, "Interface name", "interface names",
                            delegate (object obj) { return obj is InterfaceDecl || (obj is TypeDefinition && ((TypeDefinition)obj).IsInterface) || (obj is Type && ((Type)obj).IsInterface); });
                        break;

                    case ResolveCategory.Method:
                        resultRef = ResolveSpecific(name, "Method, delegate, or event name", "method, delegate, or event names",
                            delegate (object obj)
                                {
                                    return obj is MethodDecl || (obj is MethodDefinition && !((MethodDefinition)obj).IsConstructor) || obj is MethodInfo
                                            || (obj is IVariableDecl && HasPossibleDelegateType((IVariableDecl)obj))
                                            || (obj is FieldDefinition && IsDelegateType(((FieldDefinition)obj).FieldType))
                                            || (obj is PropertyDefinition && IsDelegateType(((PropertyDefinition)obj).PropertyType))
                                            || (obj is EventDefinition && IsDelegateType(((EventDefinition)obj).EventType))
                                            || (obj is FieldInfo && IsDelegateType(((FieldInfo)obj).FieldType))
                                            || (obj is PropertyInfo && IsDelegateType(((PropertyInfo)obj).PropertyType))
                                            || (obj is EventInfo && IsDelegateType(((EventInfo)obj).EventHandlerType))
                                            ||
                                            (IsDocCodeRefToMethod &&
                                            (obj is ConstructorDecl || (obj is MethodDefinition && ((MethodDefinition)obj).IsConstructor) || obj is ConstructorInfo));
                                });
                        break;

                    case ResolveCategory.Constructor:
                        resultRef = ResolveSpecific(name, "Constructor", "constructors",
                            delegate (object obj) { return obj is ConstructorDecl || (obj is MethodDefinition && ((MethodDefinition)obj).IsConstructor) || obj is ConstructorInfo; });
                        break;

                    case ResolveCategory.Attribute:
                        resultRef = ResolveAttribute("Attribute", "attributes",
                            delegate (object obj) { return obj is ConstructorDecl || (obj is MethodDefinition && ((MethodDefinition)obj).IsConstructor) || obj is ConstructorInfo; });
                        break;

                    case ResolveCategory.OperatorOverload:
                        resultRef = ResolveSpecific(name, "Overloaded operator", "overloaded operators",
                            delegate (object obj) { return obj is OperatorDecl || obj is MethodDefinition || obj is MethodInfo; });
                        break;

                    case ResolveCategory.Property:
                        resultRef = ResolveSpecific(name, "Property name", "property names",
                            delegate (object obj)
                            {
                                return obj is PropertyDecl || (obj is PropertyDefinition && !((PropertyDefinition)obj).HasParameters)
             || (obj is PropertyInfo && !PropertyInfoUtil.IsIndexed((PropertyInfo)obj));
                            });
                        break;

                    case ResolveCategory.Indexer:
                        resultRef = ResolveSpecific(name, "Indexer", "indexers",
                            delegate (object obj)
                            {
                                return obj is IndexerDecl || (obj is PropertyDefinition && ((PropertyDefinition)obj).HasParameters) ||
             (obj is PropertyInfo && PropertyInfoUtil.IsIndexed((PropertyInfo)obj));
                            });
                        break;

                    case ResolveCategory.Event:
                        resultRef = ResolveSpecific(name, "Event name", "event names",
                            delegate (object obj)
                            {
                                return obj is EventDecl || obj is EventDefinition || obj is EventInfo
             || (obj is FieldDecl && ((FieldDecl)obj).IsEvent);
                            });
                        break;

                    case ResolveCategory.TypeParameter:
                    case ResolveCategory.LocalTypeParameter:
                        resultRef = ResolveSpecific(name, "Type parameter name", "type parameter names", delegate (object obj) { return obj is TypeParameter || obj is GenericParameter; });
                        break;

                    case ResolveCategory.Parameter:
                        resultRef = ResolveSpecific(name, "Parameter name", "parameter names",
                            delegate (object obj) { return obj is ParameterDecl || obj is ParameterDefinition || obj is ParameterInfo; });
                        break;

                    case ResolveCategory.GotoTarget:
                        resultRef = ResolveSpecific(name, "Label or case name", "label or case names", delegate (object obj) { return obj is Label || obj is SwitchItem; });
                        break;

                    case ResolveCategory.NamespaceAlias:
                        resultRef = ResolveSpecific(name, "Namespace alias name", "namespace alias names",
                            delegate (object obj) { return obj is ExternAlias || (obj is Alias && ((Alias)obj).IsNamespace); });
                        break;

                    case ResolveCategory.RootNamespace:
                        resultRef = ResolveSpecific(name, "Root namespace name", "root namespace names", delegate (object obj) { return obj is Namespace; });
                        break;

                    case ResolveCategory.Expression:
                        // Handle references in expressions - the exact category isn't known, but we can still filter out illegal
                        // object types.  For example, constructors will be filtered out, so that only the parent types will be matched.
                        // Destructors can also be eliminated, since they can't be referenced directly.  Indexers aren't normally referenced
                        // directly, but this can be done in doc comments, so they're legal, but only if we have an UnresolvedThisRef.
                        // Alias objects (Namespace or Type aliases) are valid in expressions, because they can be prefixed on a reference
                        // to a static member of a type, or on an enum member.  Ditto for ExternAlias.
                        resultRef = ResolveUnspecified(delegate (object obj)
                            {
                                return !(obj is ConstructorDecl || (obj is MethodDefinition && ((MethodDefinition)obj).IsConstructor)
                                         || obj is ConstructorInfo || obj is DestructorDecl)
                                       && (!(obj is IndexerDecl || (obj is PropertyDefinition && ((PropertyDefinition)obj).HasParameters)
                                             || (obj is PropertyInfo && PropertyInfoUtil.IsIndexed((PropertyInfo)obj))) || _unresolvedRef is UnresolvedThisRef);
                            });
                        break;

                    default:  // ResolveCategory.CodeObject, ResolveCategory.Unspecified
                        // These other categories are placeholders - this routine should never be called with them, or something
                        // is wrong somewhere in the code.  Don't resolve the reference, so that the problem will show up.
                        resultRef = _unresolvedRef;
                        break;
                }

                if (resultRef == null || resultRef is UnresolvedRef)
                {
                    // Count resolve failures, but ignore if "quiet" mode
                    if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                        ++ResolveFailures;
                }
                else
                {
                    // Go ahead and count successful resolves in "quiet" mode
                    if (_resolveFlags.HasFlag(ResolveFlags.Quiet))
                        ++ResolveAttempts;
                }

                // If we're about to replace this object, dispose it now
                if (_unresolvedRef != resultRef)
                    _unresolvedRef.Dispose();
            }
            catch (Exception ex)
            {
                string message = Log.Exception(ex, "resolving" + (_unresolvedRef != null ? " '" + _unresolvedRef.Name + "'" : ""));
                if (_unresolvedRef != null)
                {
                    _unresolvedRef.AttachMessage(message, _resolveFlags.HasFlag(ResolveFlags.InDocComment)
                        ? MessageSeverity.Warning : MessageSeverity.Error, MessageSource.Resolve);
                }
                resultRef = _unresolvedRef;
            }

            return resultRef;
        }

        protected static List<Expression> GetDelegateParametersAsArguments(Expression parentExpression)
        {
            List<Expression> arguments = null;
            if (parentExpression != null)
            {
                ICollection delegateParameters = parentExpression.GetDelegateParameters();
                if (delegateParameters != null)
                {
                    int parameterCount = delegateParameters.Count;
                    arguments = new List<Expression>(parameterCount);
                    for (int i = 0; i < parameterCount; ++i)
                        arguments.Add(ParameterRef.GetParameterType(delegateParameters, i, parentExpression));
                }
            }
            return arguments;
        }

        protected static string GetWrongCategoryError(object obj)
        {
            string error;
            if (obj is INamedCodeObject)
            {
                INamedCodeObject namedObj = (INamedCodeObject)obj;
                error = namedObj.Category + " name '" + namedObj.Name + "'";
            }
            else if (obj is MemberReference)
            {
                MemberReference memberDefinition = (MemberReference)obj;
                error = MemberReferenceUtil.GetCategory(memberDefinition) + " name '" + memberDefinition.Name + "'";
            }
            else if (obj is ParameterDefinition)
            {
                ParameterDefinition parameterDefinition = (ParameterDefinition)obj;
                error = ParameterDefinitionUtil.GetCategory(parameterDefinition) + " name '" + parameterDefinition.Name + "'";
            }
            else if (obj is MemberInfo)
            {
                MemberInfo memberInfo = (MemberInfo)obj;
                error = MemberInfoUtil.GetCategory(memberInfo) + " name '" + memberInfo.Name + "'";
            }
            else if (obj is ParameterInfo)
            {
                ParameterInfo parameterInfo = (ParameterInfo)obj;
                error = ParameterInfoUtil.GetCategory(parameterInfo) + " name '" + parameterInfo.Name + "'";
            }
            else
                error = "unknown object";
            return error;
        }

        protected static bool IsStatic(object obj)
        {
            if (obj is IModifiers)
                return ((IModifiers)obj).IsStatic;
            if (obj is VariableDecl)
                return ((VariableDecl)obj).IsStatic;
            if (obj is MethodDefinition)
                return ((MethodDefinition)obj).IsStatic;
            if (obj is FieldDefinition)
                return ((FieldDefinition)obj).IsStatic;
            if (obj is PropertyDefinition)
                return PropertyDefinitionUtil.IsStatic((PropertyDefinition)obj);
            if (obj is EventDefinition)
                return EventDefintionUtil.IsStatic((EventDefinition)obj);
            if (obj is MethodBase)
                return ((MethodBase)obj).IsStatic;
            if (obj is FieldInfo)
                return ((FieldInfo)obj).IsStatic;
            if (obj is PropertyInfo)
                return PropertyInfoUtil.IsStatic((PropertyInfo)obj);
            if (obj is EventInfo)
                return EventInfoUtil.IsStatic((EventInfo)obj);
            return false;
        }

        /// <summary>
        /// Get the arguments from the parent object.
        /// </summary>
        /// <param name="hasArguments">False if we're looking for the parameters of a delegate type to
        /// which a method group is being assigned.</param>
        /// <param name="hasUnresolvedDelegateType">True if the arguments couldn't be returned because the
        /// associated delegate type is unresolved.</param>
        /// <returns>The list of arguments.  An empty list is returned if there are zero arguments, or null
        /// is returned if no arguments exist or if they can't be determined due to an unresolved delegate type.</returns>
        protected List<Expression> GetParentArguments(bool hasArguments, out bool hasUnresolvedDelegateType)
        {
            hasUnresolvedDelegateType = false;

            // Get the parent object, or grandparent if we have a Dot or a CheckedOperator (which can wrap the method
            // name, before the call).  Also skip a Conditional parent, allowing for a Return expression with one.
            bool hasDot = false;
            CodeObject lastObject = _unresolvedRef;
            CodeObject parent = _unresolvedRef.Parent;
            if (parent is Dot)
            {
                lastObject = parent;
                parent = parent.Parent;
                hasDot = true;
            }
            if (parent is CheckedOperator || parent is Conditional)
            {
                lastObject = parent;
                parent = parent.Parent;
            }

            // Find the arguments based upon the parent object type
            List<Expression> arguments = null;
            if (parent is ArgumentsOperator)
            {
                ArgumentsOperator argumentsOperator = (ArgumentsOperator)parent;
                if (hasArguments)
                {
                    // Handle the most common case - a method (or delegate) invocation.  We return the actual
                    // ChildList for performance - it's up to the caller to not modify the list in this case.
                    arguments = argumentsOperator.Arguments;

                    // If the list is null, create a dummy empty one to distinguish from the absense of arguments
                    if (arguments == null)
                        arguments = new List<Expression>();

                    // If they're Attribute arguments, extract from the list if necessary to remove any property assignments
                    if (argumentsOperator is Call && argumentsOperator.Parent is Attribute)
                    {
                        int argumentCount = Enumerable.Count(Enumerable.TakeWhile(arguments, delegate (Expression argument) { return !(argument is Assignment); }));
                        return (argumentCount < arguments.Count ? arguments.GetRange(0, argumentCount) : arguments);
                    }
                }
                else
                {
                    // Handle a method group being passed as an argument to a parameter of a delegate type.
                    // The method group can only represent methods (or generic methods) and not constructors, destructors,
                    // or indexers.  However, our parent ArgumentsOperator might be a Call, Index, or NewObject.
                    for (int i = 0; i < argumentsOperator.Arguments.Count; ++i)
                    {
                        Expression argument = argumentsOperator.Arguments[i];
                        if (argument == lastObject)
                        {
                            SymbolicRef targetRef = argumentsOperator.GetInvocationTargetRef();
                            if (targetRef == null || targetRef is UnresolvedRef)
                            {
                                // The arguments couldn't be determined because the delegate type is not resolved yet
                                hasUnresolvedDelegateType = true;
                                return null;
                            }
                            TypeRefBase delegateTypeRef = argumentsOperator.GetDelegateParameterType(targetRef.Reference, i);
                            arguments = GetDelegateParametersAsArguments(delegateTypeRef);
                            break;
                        }
                    }
                }
            }
            else if (parent is Attribute)
            {
                // Handle the case of an attribute with any parens on the call
                arguments = new List<Expression>();
            }
            else if (parent is VariableDecl)
            {
                // Handle a method group in a variable initialization - get the parameters of the delegate
                // type to which we're being assigned.
                if (!hasArguments)
                    arguments = GetDelegateParametersAsArguments(((VariableDecl)parent).Type);
            }
            else if (parent is TypeRefBase)
            {
                // If our parent is a TypeRefBase, we're doing an overload resolution of an existing method group, smuggling the
                // associated delegate type in as our Parent.  See UnresolvedRef.ResolveMethodGroup().
                arguments = GetDelegateParametersAsArguments((TypeRefBase)parent);
            }
            else if (parent is IParameters)
            {
                if (hasDot)
                {
                    // Handle the special case of an explicit interface implementation of a method or
                    // indexer - we have to convert the parameters to arguments so they can be matched.
                    // We MUST provide an empty list if there are zero parameters.
                    arguments = new List<Expression>();
                    ChildList<ParameterDecl> parameterDecls = ((IParameters)parent).Parameters;
                    if (parameterDecls != null && parameterDecls.Count > 0)
                        arguments.AddRange(Enumerable.Select<ParameterDecl, Expression>(parameterDecls, delegate (ParameterDecl parameterDecl) { return parameterDecl.Type; }));
                }
            }
            else if (parent is BinaryOperator)
            {
                BinaryOperator binaryOperator = (BinaryOperator)parent;
                if (binaryOperator.HiddenRef == _unresolvedRef)
                {
                    // Handle the case of checking for an overloaded binary operator - in this case, the
                    // arguments are the operands of the operator.
                    arguments = new List<Expression>(2) { binaryOperator.Left, binaryOperator.Right };
                }
                else
                {
                    // Handle a method group on the right side of a binary operator - such as a method group being
                    // assigned to a delegate, or added/removed from an event with "+=" or "-=", or simply compared
                    // to a delegate variable with "==" or "!=".  Note that for the last example, the method group
                    // could be on the left side - although it's poor programming style, and also can't resolve on
                    // the first pass since the right side won't be resolved yet.  Regardless, we must check for the
                    // left side scenario here and pre-resolve the right side.
                    if (binaryOperator.Left != null)
                    {
                        if (binaryOperator.Left.SkipPrefixes() == _unresolvedRef)
                        {
                            if (binaryOperator.Right != null)
                            {
                                binaryOperator.Right = (Expression)binaryOperator.Right.Resolve(ResolveCategory.Expression, _resolveFlags);
                                arguments = GetDelegateParametersAsArguments(binaryOperator.Right.EvaluateType());
                            }
                        }
                        else
                            arguments = GetDelegateParametersAsArguments(binaryOperator.Left.EvaluateType());
                    }
                }
            }
            else if (parent is UnaryOperator)
            {
                // Handle the case of checking for an overloaded unary operator - in this case, the
                // argument is the operand of the operator.
                if (parent.HiddenRef == _unresolvedRef)
                    arguments = new List<Expression>(1) { ((UnaryOperator)parent).Expression };
                else if (parent is Cast)
                {
                    // Handle casting to a delegate type
                    arguments = GetDelegateParametersAsArguments(((Cast)parent).Type.EvaluateType());
                }
            }
            else if (parent is Return)
            {
                // If the parent is a Return statement, get the delegate type from the parent method return type,
                // so that we can resolve overloaded methods based on the parent's return type.
                Expression delegateExpression = null;
                CodeObject parentMethod = parent.FindParentMethod();
                if (parentMethod is MethodDeclBase)
                    delegateExpression = ((MethodDeclBase)parentMethod).ReturnType;
                else if (parentMethod is AnonymousMethod)
                {
                    // If the anonymous method is being returned from another anonymous method, recursively get the parent
                    // delegate expression of the parent anonymous method, then get the return type of that delegate.
                    delegateExpression = ((AnonymousMethod)parentMethod).GetParentDelegateExpression();
                    if (delegateExpression != null)
                        delegateExpression = delegateExpression.GetDelegateReturnType();
                }
                if (delegateExpression != null)
                    arguments = GetDelegateParametersAsArguments(delegateExpression);
            }

            return arguments;
        }

        protected bool HasPossibleDelegateType(IVariableDecl variableDecl)
        {
            Expression type = variableDecl.Type;
            if (type != null)
            {
                TypeRefBase typeRefBase = type.EvaluateType();
                if (typeRefBase != null)
                {
                    typeRefBase = typeRefBase.EvaluateTypeArgumentTypes(_unresolvedRef.Parent, _unresolvedRef);
                    return typeRefBase.IsPossibleDelegateType;
                }
            }
            return true;
        }

        protected bool IsDelegateType(TypeReference typeReference)
        {
            return TypeRef.Create(typeReference).EvaluateTypeArgumentTypes(_unresolvedRef.Parent, _unresolvedRef).IsDelegateType;
        }

        protected bool IsDelegateType(Type type)
        {
            return TypeRef.Create(type).EvaluateTypeArgumentTypes(_unresolvedRef.Parent, _unresolvedRef).IsDelegateType;
        }

        protected bool IsErrorDueToUnresolvedOnly(SymbolicRef parentTypeRef)
        {
            return ((parentTypeRef is TypeRefBase && parentTypeRef.HasUnresolvedRef()) || _unresolvedRef.IsAnyMismatchDueToUnresolvedOnly());
        }

        /// <summary>
        /// Resolve the reference as an attribute.
        /// </summary>
        protected SymbolicRef ResolveAttribute(string expected, string noMatches, Func<object, bool> isValidCategory)
        {
            // First try adding an "Attribute" suffix, regardless of whether or not it already has one - this is
            // necessary to match names such as "XmlAttributeAttribute".
            string name = (string)_unresolvedRef.Reference;
            SymbolicRef resultRef = ResolveSpecific(name + Attribute.NameSuffix, expected, noMatches, isValidCategory);
            if (resultRef is UnresolvedRef && !((UnresolvedRef)resultRef).HasMatches)
            {
                // If that didn't find any matches, try the unmodified name
                _unresolvedRef.ResetResolutionMembers();
                resultRef = ResolveSpecific(name, expected, noMatches, isValidCategory);
            }
            return resultRef;
        }

        protected void ResolveBinaryOperators(TypeRefBase leftTypeRefBase, TypeRefBase rightTypeRefBase, string name, bool checkBuiltInTypes)
        {
            // Only lookup on the left operand if it's not a base type of the right operand (to avoid dups)
            if (leftTypeRefBase is TypeRef)
            {
                if (leftTypeRefBase.IsBuiltInType == checkBuiltInTypes && !(rightTypeRefBase is TypeRef && ((TypeRef)rightTypeRefBase).IsSubclassOf((TypeRef)leftTypeRefBase)))
                    leftTypeRefBase.ResolveRef(name, this);
            }

            // Only lookup on the right operand if it's not a base type of the left operand (to avoid dups)
            if (rightTypeRefBase is TypeRef)
            {
                if (rightTypeRefBase.IsBuiltInType == checkBuiltInTypes && !(leftTypeRefBase is TypeRef && ((TypeRef)leftTypeRefBase).IsSubclassOf((TypeRef)rightTypeRefBase)))
                    rightTypeRefBase.ResolveRef(name, this);
            }
        }

        protected void ResolveMember(ref SymbolicRef parentTypeRef, string name)
        {
            CodeObject parent = _unresolvedRef.Parent;

            // Handle symbols on the right side of a '.' - restrict the search scope to the parent object type
            _parentExpression = ((Dot)parent).Left;
            if (_parentExpression == null)
                return;

            // If the parent expression is an UnresolvedRef because it's an ambiguous type reference, try to
            // resolve as a member of each of the possible types in order to avoid cascading errors.
            if (_parentExpression is UnresolvedRef)
            {
                UnresolvedRef parentUnresolvedRef = (UnresolvedRef)_parentExpression;
                if (parentUnresolvedRef.HasMatches)
                {
                    foreach (MatchCandidate candidate in parentUnresolvedRef.Matches)
                    {
                        object candidateObject = candidate.Object;
                        if (candidateObject is ITypeDecl || candidateObject is TypeDefinition || candidateObject is Type)
                        {
                            // Try to resolve as a member of the type, and exit if it works
                            TypeRefBase currentRef = (TypeRefBase)_unresolvedRef.CreateRef(candidateObject, true);
                            _parentExpression = currentRef;  // Override the parent expression
                            currentRef.ResolveRef(name, this);
                            if (_unresolvedRef.HasCompleteMatch)
                                return;
                        }
                    }

                    // Reset and restore the parent expression if it didn't work
                    _unresolvedRef.ResetResolutionMembers();
                    _parentExpression = ((Dot)parent).Left;
                }
                return;
            }

            // In this case, the parent expression might evaluate to a NamespaceRef (in addition to a TypeRef or UnresolvedRef)
            parentTypeRef = _parentExpression.EvaluateTypeOrNamespace();
            if (parentTypeRef != null)
            {
                // Search the parent Type or Namespace for the child member
                parentTypeRef.ResolveRef(name, this);

                // If the member failed to resolve, and the parent is a simple name of a variable (or property), then check if a type
                // exists in scope with the same name as the parent ref, and if so, then try resolving as a *static* member of that type,
                // and if successful, convert the parent expression into a TypeRef of that type.  This allows what would otherwise be a
                // compile error to be successfully resolved.  Note that the spec says that the evaluated type of the variable should match
                // the name of the variable, but this is actually WRONG - although this is often true, it's possible for the type to be a
                // base type or interface of a valid type that matches the variable name. For this reason, and to avoid a string compare of
                // the type name, it seems best to only check for this situation after the initial resolve fails, rather than trying to
                // detect it in advance.
                if (!_unresolvedRef.HasCompleteMatch && _parentExpression is VariableRef)
                {
                    // Determine if the parent expression could be alternatively interpreted as a type
                    UnresolvedRef unresolvedRef = new UnresolvedRef((VariableRef)_parentExpression, ResolveCategory.Type);
                    TypeRef typeRef = unresolvedRef.Resolve(ResolveCategory.Type, ResolveFlags.Quiet) as TypeRef;
                    if (typeRef != null)
                    {
                        // If so, determine if this unresolved ref is a valid static member of that type
                        _parentExpression = typeRef;
                        typeRef.ResolveRef(name, this);
                        if (_unresolvedRef.HasCompleteMatch)
                        {
                            // If so, replace the parent VariableRef with the TypeRef
                            ((Dot)parent).Left = typeRef;
                        }
                        else
                        {
                            // Reset and restore the parent expression if it didn't work, and re-resolve
                            // to restore the exact state.
                            _unresolvedRef.ResetResolutionMembers();
                            _parentExpression = ((Dot)parent).Left;
                            parentTypeRef.ResolveRef(name, this);
                        }
                    }
                    else
                        unresolvedRef.Dispose();
                }
            }
        }

        /// <summary>
        /// Resolve the reference using a known category.
        /// </summary>
        protected SymbolicRef ResolveSpecific(string name, string expected, string noMatches, Func<object, bool> isValidCategory)
        {
            // Matching type and method arguments can be either 'loose' or exact - this is controlled by
            // the ExactMatching constant.  Exact matching can be turned on, but the general idea is to
            // match anyway if the arguments don't match, and then have the analysis phase indicate an
            // error if the arguments don't match exactly.
            // If a match can't be determined because of ambiguity, the unresolved symbol is updated
            // to indicate the appropriate error message and list of possible matches.

            SymbolicRef resultRef = _unresolvedRef;
            CodeObject parent = _unresolvedRef.Parent;
            _isValidCategory = isValidCategory;

            // The parent type is a SymbolicRef because it might be a NamespaceRef
            SymbolicRef parentTypeRef = null;

            // Special handling for operator overloads
            if (_resolveCategory == ResolveCategory.OperatorOverload)
            {
                if (parent is BinaryOperator && parent.HiddenRef == _unresolvedRef)
                {
                    // Evaluate a possible binary operator-overload reference - start by determining the types of the
                    // two operands.  Abort if they're not TypeRefs.  For now, we're ignoring TypeParameterRefs - they
                    // will fail to resolve, but that's OK since any operator overload should only be done with actual
                    // types, which means it would occur in the generated code (not even sure how this is handled).
                    // Note that 'user-defined' overloaded operators can appear even in built-in types such as 'System.Decimal'.
                    BinaryOperator binaryOperator = (BinaryOperator)parent;
                    Expression left = binaryOperator.Left;
                    Expression right = binaryOperator.Right;
                    if (left == null || right == null)
                        return null;
                    TypeRefBase leftTypeRefBase = left.EvaluateType(true);
                    TypeRefBase rightTypeRefBase = right.EvaluateType(true);

                    // If the left and right are the same type, only lookup on one of them
                    if (leftTypeRefBase != null && leftTypeRefBase.IsSameRef(rightTypeRefBase))
                        leftTypeRefBase.ResolveRef(name, this);
                    else
                    {
                        // Only do lookups on built-in types if no operators exist on user types
                        ResolveBinaryOperators(leftTypeRefBase, rightTypeRefBase, name, false);
                        if (_unresolvedRef.Matches == null || _unresolvedRef.Matches.Count == 0)
                            ResolveBinaryOperators(leftTypeRefBase, rightTypeRefBase, name, true);
                    }
                }
                else if (parent is UnaryOperator && parent.HiddenRef == _unresolvedRef)
                {
                    // Evaluate a possible unary operator-overload reference - start by determining the type of the operand.
                    // Note that 'user-defined' overloaded operators can appear even in built-in types such as 'System.Decimal'.
                    _parentExpression = ((UnaryOperator)parent).Expression;
                    TypeRef typeRef = _parentExpression.EvaluateType(true) as TypeRef;

                    // Abort if it's not a TypeRef.  For now, we're ignoring TypeParameterRefs.
                    if (typeRef == null)
                        return null;

                    // Abort if it's a cast to an 'object' or the same type as the expression
                    if (_unresolvedRef.Name == "op_Explicit" && _unresolvedRef.Parent is Cast)
                    {
                        TypeRefBase castTypeRef = ((Cast)_unresolvedRef.Parent).Type.EvaluateType();
                        if (castTypeRef.IsSameRef(TypeRef.ObjectRef) || castTypeRef.IsSameRef(typeRef))
                            return null;
                    }

                    typeRef.ResolveRef(name, this);
                }
                else
                    return null;  // Abort if not recognized as a valid operator overload
            }
            else
            {
                if (parent is Dot && ((Dot)parent).Right == _unresolvedRef)
                {
                    // Resolve the symbol as a member of the parent object
                    ResolveMember(ref parentTypeRef, name);
                }
                else if (parent is Lookup && ((Lookup)parent).Right == _unresolvedRef)
                {
                    // Handle symbols on the right side of a '::' - restrict the search scope to the namespace alias on the left
                    Expression leftExpression = ((Lookup)parent).Left;
                    Namespace rootNamespace = null;
                    if (leftExpression is ExternAliasRef)
                        rootNamespace = ((ExternAliasRef)leftExpression).RootNamespace;
                    else if (leftExpression is AliasRef)
                        rootNamespace = ((AliasRef)leftExpression).Namespace.Namespace;
                    if (rootNamespace != null)
                        rootNamespace.ResolveRef(name, this, false);
                }
                else if (parent is NewObject && parent.HiddenRef == _unresolvedRef)
                {
                    // Handle special hidden ConstructorRef references for NewObjects
                    _parentExpression = ((NewObject)parent).Expression;
                    parentTypeRef = _parentExpression.EvaluateType();
                    if (parentTypeRef != null)
                        AddMatchInternal(parentTypeRef.Reference, true);
                }
                else if (parent is Index && parent.HiddenRef == _unresolvedRef)
                {
                    // Handle special hidden IndexerRef references for Index operators
                    _parentExpression = ((Index)parent).Expression;
                    parentTypeRef = _parentExpression.EvaluateType();
                    if (parentTypeRef != null)
                        parentTypeRef.ResolveIndexerRef(this);
                }
                else if (parent is ConstructorInitializer && parent.HiddenRef == _unresolvedRef)
                {
                    // Handle constructor initializer calls (': this(...)' or ': base(...)')
                    if (parent is ThisInitializer)
                    {
                        // For ': this(...)', always evaluate as a constructor of the current type
                        AddMatchInternal(_unresolvedRef.FindParent<TypeDecl>(), true);
                    }
                    else
                    {
                        // For ': base(...)', always evaluate as a constructor of the immediate base type
                        TypeDecl typeDecl = _unresolvedRef.FindParent<TypeDecl>();
                        if (typeDecl is ClassDecl)
                        {
                            TypeRef baseRef = typeDecl.GetBaseType();
                            AddMatchInternal(baseRef.Reference, true);
                        }
                    }
                }
                else if (parent != null)
                {
                    // Search up the code tree, trying to resolve the symbol.  Multiple matches might be found, and
                    // partial matches might also be found.  The search stops at the current scope if at least one
                    // match is found, and will also stop at certain points for certain search categories.  If a
                    // match is found, any partial matches will be ignored.  If multiple matches are found, we will
                    // attempt to trim the list, getting it down to a single match if possible.  If no match is found,
                    // any list of partial matches will also be trimmed if possible.
                    if (_resolveCategory == ResolveCategory.GotoTarget)
                        parent.ResolveGotoTargetUp(name, this);
                    else
                        parent.ResolveRefUp(name, this);
                }
            }

            // If we have multiple matches, try filtering them (by better method matches, or other parts of partial types)
            if (_unresolvedRef.Matches != null && _unresolvedRef.Matches.Count > 1)
                FilterMatches(parentTypeRef as TypeRefBase);
            // If we're in a documentation comment, try ignoring accessibility rules to get a better match
            if (_unresolvedRef.Matches != null && _unresolvedRef.Matches.Count > 0 && _resolveFlags.HasFlag(ResolveFlags.InDocComment))
                FilterToInaccessibleOnly();

            MatchCandidates matches = _unresolvedRef.Matches;
            int foundCount = (matches != null ? matches.Count : 0);
            if (_resolveCategory == ResolveCategory.OperatorOverload)
            {
                // If it's an operator overload and we didn't find a single complete match, ignore it (use the default operator)
                //if (foundCount < 1 || !matches.IsCompleteMatch)  // Use this to see multiple complete matches for debugging
                if (foundCount != 1 || !matches.IsCompleteMatch)
                    return null;
                _resolveFlags &= ~ResolveFlags.Quiet;  // Turn on messages at this point (really only applies for debugging logic above)
            }

            string message = null;
            MessageSeverity messageType = _resolveFlags.HasFlag(ResolveFlags.InDocComment) ? MessageSeverity.Warning : MessageSeverity.Error;
            if (foundCount == 1)
            {
                // Generate the reference or error message, as appropriate
                MatchCandidate finalMatch = matches[0];
                if (finalMatch.IsCompleteMatch || (!ExactMatching && finalMatch.IsCategoryMatch))
                {
                    resultRef = _unresolvedRef.CreateRef(finalMatch.Object, true);

                    // If we have inferred type arguments, copy them to the result (this can occur
                    // for MethodRefs or TypeRefs in doc comments).
                    if (finalMatch.InferredTypeArguments != null)
                        finalMatch.CopyInferredTypeArguments((TypeRefBase)resultRef);
                }
                else if (finalMatch.IsCategoryMatch)
                {
                    if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                    {
                        message = expected + " found, but " + finalMatch.GetMismatchDescription();
                        if (IsErrorDueToUnresolvedOnly(parentTypeRef))
                            messageType = MessageSeverity.Warning;
                    }
                }
                else if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                    message = expected + " expected, but " + GetWrongCategoryError(finalMatch.Object) + " found";
            }
            else if (foundCount > 1)
            {
                if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                {
                    // Generate the appropriate error message
                    if (matches.IsCompleteMatch)
                        message = expected + " exists, but multiple matches were found";
                    else if (matches.IsCategoryMatch)
                    {
                        message = expected + " exists, but multiple incomplete matches were found";
                        if (IsErrorDueToUnresolvedOnly(parentTypeRef))
                            messageType = MessageSeverity.Warning;
                    }
                    else
                        message = expected + " expected, but multiple matches were found, and none of them are " + noMatches;
                }
            }
            else if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
            {
                if (_parentExpression != null)
                {
                    // Set an appropriate error message based on the parent (if any)
                    if (_parentExpression is UnresolvedRef)
                    {
                        message = "Parent is unresolved";
                        messageType = MessageSeverity.Warning;
                    }
                    else if (_parentExpression is AliasRef && _parentExpression.HasUnresolvedRef())
                    {
                        message = "Parent is an unresolved alias";
                        messageType = MessageSeverity.Warning;
                    }
                    else if ((parentTypeRef is TypeRefBase && parentTypeRef.HasUnresolvedRef()) || (parentTypeRef == null))
                    {
                        message = (_parentExpression is SymbolicRef ? "Type of parent object is undetermined" : "Type of parent expression is undetermined");
                        messageType = MessageSeverity.Warning;
                    }
                    else
                    {
                        message = expected + " expected, but " + parentTypeRef.Category + " '" + parentTypeRef.GetDescription() + "' has no member '" + name + "'";
                        if (parentTypeRef is TypeRefBase && parentTypeRef.HasUnresolvedRef())
                            messageType = MessageSeverity.Warning;
                    }
                }
                else
                    message = expected + " expected, but symbol '" + name + "' can't be resolved";
            }

            if (message != null)
                _unresolvedRef.AttachMessage(message, messageType, MessageSource.Resolve);

            return resultRef;
        }

        /// <summary>
        /// This method is called to resolve a symbol in the situation where a category couldn't be determined.
        /// Because parsing can always determine the category in various situations, this method should never be
        /// called for: Generic Types, Namespaces, Methods (with parens), Constructors, Attributes, Goto Targets,
        /// Directive Expressions, literal constants, Indexers, Events, variables of delegate type.
        /// So, it doesn't need to worry about type arguments or method arguments.
        /// This leaves the possibilities of: Non-Generic Types, Type Parameters, Properties, Fields, Locals,
        /// Parameters, and non-invoked Method names (without parens, such as when assigned to delegates or passed
        /// as a parameter of delegate type).  Namespace prefixes can also exist for many of these.
        /// </summary>
        protected SymbolicRef ResolveUnspecified(Func<object, bool> isValidCategory)
        {
            SymbolicRef resultRef = _unresolvedRef;
            string name = (string)_unresolvedRef.Reference;
            CodeObject parent = _unresolvedRef.Parent;
            _isValidCategory = isValidCategory;

            // The parent type is a SymbolicRef because it might be a NamespaceRef
            SymbolicRef parentTypeRef = null;

            if (parent is Dot && ((Dot)parent).Right == _unresolvedRef)
            {
                // Resolve the symbol as a member of the parent object
                ResolveMember(ref parentTypeRef, name);
            }
            else if (parent != null)
            {
                if (parent.GetType() == typeof(Assignment) && ((Assignment)parent).Left == _unresolvedRef)
                {
                    CodeObject grandParent = parent.Parent;

                    // The symbol is the 'lvalue' of an assignment - check some special cases
                    if (grandParent is Initializer)
                    {
                        // Handle assignment lvalues in object initializers - restrict the search scope to the great-
                        // grandparent if it's a NewObject, Initializer, or Assignment (ignore NewArray).
                        CodeObject greatGrandParent = grandParent.Parent;
                        if (greatGrandParent is Expression && !(greatGrandParent is NewArray))
                            parentTypeRef = ((Expression)greatGrandParent).EvaluateType();
                    }
                    else if (grandParent is Call && grandParent.Parent is Attribute)
                    {
                        // Handle assignment lvalues in attribute calls - restrict the search scope to the parent type
                        Call call = (Call)grandParent;
                        parentTypeRef = call.EvaluateType();

                        // Parameters are resolved first to allow the call expression to be resolved, so we
                        // probably have to resolve the call expression at this point (which should work now,
                        // as all of the 'real' parameters should be resolved now).
                        if (parentTypeRef is UnresolvedRef)
                        {
                            call.Expression = (Expression)call.Expression.Resolve(ResolveCategory.Attribute, _resolveFlags);
                            parentTypeRef = call.EvaluateType();
                        }
                    }
                }

                // If we have a parent type, search it for the member, otherwise search up the code tree
                if (parentTypeRef != null)
                    parentTypeRef.ResolveRef(name, this);
                else
                    parent.ResolveRefUp(name, this);
            }

            // If we have multiple matches, try filtering them (by better method matches, or other parts of partial types)
            if (_unresolvedRef.Matches != null && _unresolvedRef.Matches.Count > 1)
                FilterMatches(null);
            // If we're in a documentation comment, try ignoring accessibility rules to get a better match
            if (_unresolvedRef.Matches != null && _unresolvedRef.Matches.Count > 0 && _resolveFlags.HasFlag(ResolveFlags.InDocComment))
                FilterToInaccessibleOnly();

            string message = null;
            MessageSeverity messageType = _resolveFlags.HasFlag(ResolveFlags.InDocComment) ? MessageSeverity.Warning : MessageSeverity.Error;

            MatchCandidates matches = _unresolvedRef.Matches;
            int foundCount = (matches != null ? matches.Count : 0);
            if (foundCount == 1)
            {
                // Generate the reference or error message, as appropriate
                MatchCandidate finalMatch = matches[0];
                if (finalMatch.IsCompleteMatch || (!ExactMatching && finalMatch.IsCategoryMatch))
                {
                    resultRef = _unresolvedRef.CreateRef(finalMatch.Object, true);

                    // If we have inferred type arguments, copy them to the result (this can occur
                    // for MethodRefs or TypeRefs in doc comments).
                    if (finalMatch.InferredTypeArguments != null)
                        finalMatch.CopyInferredTypeArguments((TypeRefBase)resultRef);
                }
                else if (finalMatch.IsCategoryMatch)
                {
                    if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                    {
                        message = "Match found, but " + finalMatch.GetMismatchDescription();
                        if (IsErrorDueToUnresolvedOnly(parentTypeRef))
                            messageType = MessageSeverity.Warning;
                    }
                }
                else if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                    message = "Match found, but it isn't a valid object type in this context";
            }
            else if (foundCount > 1)
            {
                if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
                {
                    // Generate the appropriate error message
                    if (matches.IsCompleteMatch)
                        message = "Multiple matches were found";
                    else if (matches.IsCategoryMatch)
                    {
                        message = "Multiple incomplete matches were found";
                        if (IsErrorDueToUnresolvedOnly(parentTypeRef))
                            messageType = MessageSeverity.Warning;
                    }
                    else
                        message = "Multiple matches were found, but they are not valid object types in this context";
                }
            }
            else if (!_resolveFlags.HasFlag(ResolveFlags.Quiet))
            {
                if (_parentExpression != null)
                {
                    // Set an appropriate error message based on the parent (if any)
                    if (_parentExpression is UnresolvedRef)
                    {
                        message = "Parent is unresolved";
                        messageType = MessageSeverity.Warning;
                    }
                    else if (_parentExpression is AliasRef && _parentExpression.HasUnresolvedRef())
                    {
                        message = "Parent is an unresolved alias";
                        messageType = MessageSeverity.Warning;
                    }
                    else if ((parentTypeRef is TypeRefBase && parentTypeRef.HasUnresolvedRef()) || (parentTypeRef == null))
                    {
                        message = (_parentExpression is SymbolicRef ? "Type of parent object is undetermined" : "Type of parent expression is undetermined");
                        messageType = MessageSeverity.Warning;
                    }
                    else
                    {
                        message = "The " + parentTypeRef.Category + " '" + parentTypeRef.GetDescription() + "' has no member '" + name + "'";
                        if (parentTypeRef is TypeRefBase && parentTypeRef.HasUnresolvedRef())
                            messageType = MessageSeverity.Warning;
                    }
                }
                else
                    message = "Symbol '" + name + "' " + (name.Length == 1 && (char.IsPunctuation(name, 0) || char.IsSymbol(name, 0)) ? "unexpected" : "can't be resolved");
            }

            if (message != null)
                _unresolvedRef.AttachMessage(message, messageType, MessageSource.Resolve);

            return resultRef;
        }

        protected void FilterMatches(TypeRefBase parentTypeRef)
        {
            // Don't bother filtering non-complete matches, because we'll want to see all of the possible
            // options in the error message.  Also, in the case of matching a method group passed as a
            // parameter (see IsImplicitlyConvertibleTo in this class), we might match one of the other
            // members later, so we need to keep them all around.
            if (_unresolvedRef.HasCompleteMatch)
            {
                MatchCandidates matches = _unresolvedRef.Matches;

                // If we have multiple complete method matches, try filtering for better matches based upon the
                // parameters, or in the case of user-defined explicit conversions, the most-specific conversion.
                // If that doesn't work, we'll also try filtering out other parts of partial types.
                bool isMethodCategory = ResolveCategoryHelpers.IsMethod[(int)_resolveCategory];
                object firstMatch = matches[0].Object;
                if (isMethodCategory || firstMatch is MethodDecl || firstMatch is MethodDefinition || firstMatch is MethodInfo)
                {
                    bool hasUnresolvedDelegateTypes;
                    List<Expression> methodArguments = GetParentArguments(isMethodCategory, out hasUnresolvedDelegateTypes);
                    if (methodArguments != null)
                    {
                        // Check for multiple matching user-defined explicit conversion operators
                        if (_resolveCategory == ResolveCategory.OperatorOverload && _unresolvedRef.Name == "op_Explicit" && _unresolvedRef.Parent is Cast)
                        {
                            // If we can determine a single best conversion, use it (otherwise, no filtering)
                            MatchCandidate bestConversion = FindBestExplicitConversion(matches, isMethodCategory, methodArguments);
                            if (bestConversion != null)
                            {
                                matches = matches.New();
                                matches.Add(bestConversion);
                            }
                        }
                        else
                        {
                            // If we have multiple complete method matches, try looking for better vs lessor method matches
                            // based upon the matching of their parameters (ignore if non-method objects have been matched).
                            // Unlike the spec, we allow ref/out mismatches (although the type must match exactly) in AddMatch(),
                            // relying on the analysis phase to flag them as errors.  Since it's possible to overload methods
                            // with only ref/out differences, we could get dups, and also need to filter them here.
                            MatchCandidates betterMatches = matches.New();
                            betterMatches.Add(matches[0]);
                            for (int i = 1; i < matches.Count; ++i)
                            {
                                MatchCandidate candidate = matches[i];
                                object obj = candidate.Object;
                                object betterMethod = null;
                                if (isMethodCategory || obj is MethodDecl || obj is MethodDefinition || obj is MethodInfo)
                                    betterMethod = FindBetterMethod(betterMatches[0], candidate, methodArguments, _unresolvedRef);
                                if (betterMethod == null)
                                {
                                    // Neither method is better, so add to the list of similar matches
                                    betterMatches.Add(candidate);
                                }
                                else if (betterMethod == obj)
                                {
                                    // Replace any existing better matches with the new better match
                                    betterMatches.Clear();
                                    betterMatches.Add(candidate);
                                }
                            }
                            matches = betterMatches;
                        }
                    }
                }

                // Try filtering out extra parts of partial types (just use the first one)
                if (matches.Count > 1)
                {
                    MatchCandidates partialMatches = matches.New();
                    bool isFirstPartial = true;
                    foreach (MatchCandidate result in matches)
                    {
                        if (result.Object is TypeDecl && ((TypeDecl)result.Object).IsPartial)
                        {
                            // Only keep the first part of partial types
                            if (isFirstPartial)
                            {
                                partialMatches.Add(result);
                                isFirstPartial = false;
                            }
                        }
                        else
                            partialMatches.Add(result);
                    }

                    // Only keep the results if we found at least one match
                    if (partialMatches.Count > 0)
                        matches = partialMatches;
                }

                // Try filtering out hidden members.
                // Other logic (such as not searching base classes when a complete match is found in a derived class,
                // or checking the categories of matches) will prevent duplicates here.  However, in the case of multiple
                // inheritance of interfaces (which can also occur as generic constraints), it's possible to have matches
                // that should be hidden.
                if (matches.Count > 1)
                {
                    MatchCandidates nonHiddenMatches = matches.New();
                    // First, determine the declaring types of any methods, properties, indexers, and events
                    TypeRefBase[] declaringTypeRefs = new TypeRefBase[matches.Count];
                    for (int i = 0; i < matches.Count; ++i)
                    {
                        MatchCandidate candidate = matches[i];
                        object obj = candidate.Object;
                        if (obj is MethodDecl || obj is MethodDefinition || obj is MethodInfo)
                            declaringTypeRefs[i] = MethodRef.GetDeclaringType(obj);
                        else if (obj is EventDecl || obj is EventDefinition || obj is EventInfo)
                            declaringTypeRefs[i] = PropertyRef.GetDeclaringType(obj);
                        else if (obj is PropertyDeclBase || obj is PropertyDefinition || obj is PropertyInfo)
                            declaringTypeRefs[i] = PropertyRef.GetDeclaringType(obj);
                        else
                            declaringTypeRefs[i] = null;
                    }
                    // Now, for each candidate, hide it if there's another match in a 'derived' interface (don't bother
                    // checking method parameters, because we're only dealing with complete matches here).
                    for (int i = 0; i < matches.Count; ++i)
                    {
                        bool isHidden = false;
                        TypeRefBase declaringType = declaringTypeRefs[i];
                        if (declaringType != null && declaringType.IsInterface)
                        {
                            for (int j = 0; j < matches.Count; ++j)
                            {
                                if (j != i)
                                {
                                    TypeRefBase otherType = declaringTypeRefs[j];
                                    if (otherType != null && otherType.IsInterface && ((TypeRef)otherType).IsImplementationOf((TypeRef)declaringType))
                                    {
                                        isHidden = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!isHidden)
                            nonHiddenMatches.Add(matches[i]);
                    }

                    // Only keep the results if we found at least one match
                    if (nonHiddenMatches.Count > 0)
                        matches = nonHiddenMatches;
                }

                _unresolvedRef.Matches = matches;
            }
        }

        protected void FilterToInaccessibleOnly()
        {
            // Try filtering to inaccessible matches only, and if it results in a single match, force it to be
            // accessible (this routine is used only when in documentation comments).
            MatchCandidates matches = _unresolvedRef.Matches;
            MatchCandidates inaccessibleMatches = matches.New();
            foreach (MatchCandidate result in matches)
            {
                if (result.IsMismatchDueToAccessibilityOnly())
                    inaccessibleMatches.Add(result);
            }

            // Only keep the results if we found at least one match
            if (inaccessibleMatches.Count > 0)
                matches = inaccessibleMatches;
            // Furthermore, force a single match to fake being accessible
            if (matches.Count == 1)
                matches[0].IsAccessible = true;

            _unresolvedRef.Matches = matches;
        }

        /// <summary>
        /// We have multiple matching user-defined explicit conversion operators - try to find a single best conversion.
        /// </summary>
        /// <returns>The single best conversion if found, otherwise null.</returns>
        protected MatchCandidate FindBestExplicitConversion(MatchCandidates matches, bool isMethodCategory, List<Expression> methodArguments)
        {
            MatchCandidate bestConversion = null;
            TypeRefBase sourceType = methodArguments[0].EvaluateType();
            TypeRefBase targetType = ((Cast)_unresolvedRef.Parent).Type.EvaluateType();

            // Find the most specific source type and target type
            TypeRefBase Sx = null;
            TypeRefBase Tx = null;
            bool encompassesSource = false;
            bool encompassedByTarget = false;
            TypeRefBase mostEncompassedSource = null;
            TypeRefBase mostEncompassingSource = null;
            TypeRefBase mostEncompassedTarget = null;
            TypeRefBase mostEncompassingTarget = null;
            for (int i = 0; i < matches.Count; ++i)
            {
                MatchCandidate candidate = matches[i];
                object obj = candidate.Object;
                if (isMethodCategory || obj is MethodDecl || obj is MethodDefinition || obj is MethodInfo)
                {
                    TypeRefBase parameterTypeRef = ParameterRef.GetParameterType(MethodRef.GetParameters(obj), 0, _parentExpression);
                    TypeRefBase returnTypeRef = MethodRef.GetReturnType(obj, _parentExpression);

                    // If any of the operators in U convert from S, then Sx is S.
                    if (parameterTypeRef.IsSameRef(sourceType))
                        Sx = sourceType;
                    // If any of the operators in U convert to T, then Tx is T.
                    if (returnTypeRef.IsSameRef(targetType))
                        Tx = targetType;

                    // Otherwise, if any of the operators in U convert from types that encompass S, then Sx is the most encompassed type in the
                    // combined set of source types of those operators. If no most encompassed type can be found, then the conversion is ambiguous.
                    // Otherwise, Sx is the most encompassing type in the combined set of source types of the operators in U. If exactly one most
                    // encompassing type cannot be found, then the conversion is ambiguous.
                    if (TypeRef.IsEncompassedBy(sourceType, parameterTypeRef))
                        encompassesSource = true;

                    // Otherwise, if any of the operators in U convert to types that are encompassed by T, then Tx is the most encompassing type in the
                    // combined set of target types of those operators. If exactly one most encompassing type cannot be found, then the conversion is ambiguous.
                    // Otherwise, Tx is the most encompassed type in the combined set of target types of the operators in U. If no most encompassed type
                    // can be found, then the conversion is ambiguous.
                    if (TypeRef.IsEncompassedBy(returnTypeRef, targetType))
                        encompassedByTarget = true;

                    if (Sx == null)
                    {
                        if (i == 0)
                            mostEncompassedSource = mostEncompassingSource = parameterTypeRef;
                        else
                        {
                            if (mostEncompassedSource != null)
                            {
                                if (TypeRef.IsEncompassedBy(parameterTypeRef, mostEncompassedSource))
                                    mostEncompassedSource = parameterTypeRef;
                                else if (!TypeRef.IsEncompassedBy(mostEncompassedSource, parameterTypeRef))
                                    mostEncompassedSource = null;  // No single most encompassed source exists
                            }
                            if (mostEncompassingSource != null)
                            {
                                if (TypeRef.IsEncompassedBy(mostEncompassingSource, parameterTypeRef))
                                    mostEncompassingSource = parameterTypeRef;
                                else if (!TypeRef.IsEncompassedBy(parameterTypeRef, mostEncompassingSource))
                                    mostEncompassingSource = null;  // No single most encompassing source exists
                            }
                        }
                    }
                    if (Tx == null)
                    {
                        if (i == 0)
                            mostEncompassedTarget = mostEncompassingTarget = returnTypeRef;
                        else
                        {
                            if (mostEncompassingTarget != null)
                            {
                                if (TypeRef.IsEncompassedBy(mostEncompassingTarget, returnTypeRef))
                                    mostEncompassingTarget = returnTypeRef;
                                else if (!TypeRef.IsEncompassedBy(returnTypeRef, mostEncompassingTarget))
                                    mostEncompassingTarget = null;  // No single most encompassing target exists
                            }
                            if (mostEncompassedTarget != null)
                            {
                                if (TypeRef.IsEncompassedBy(returnTypeRef, mostEncompassedTarget))
                                    mostEncompassedTarget = returnTypeRef;
                                else if (!TypeRef.IsEncompassedBy(mostEncompassedTarget, returnTypeRef))
                                    mostEncompassedTarget = null;  // No single most encompassed target exists
                            }
                        }
                    }
                }
            }
            if (Sx == null)
                Sx = (encompassesSource ? mostEncompassedSource : mostEncompassingSource);
            if (Tx == null)
                Tx = (encompassedByTarget ? mostEncompassingTarget : mostEncompassedTarget);

            // If U contains exactly one user-defined conversion operator that converts from Sx to Tx, then this is the
            // most specific conversion operator.
            foreach (MatchCandidate candidate in matches)
            {
                object obj = candidate.Object;
                if (isMethodCategory || obj is MethodDecl || obj is MethodDefinition || obj is MethodInfo)
                {
                    TypeRefBase parameterTypeRef = ParameterRef.GetParameterType(MethodRef.GetParameters(obj), 0, _parentExpression);
                    TypeRefBase returnTypeRef = MethodRef.GetReturnType(obj, _parentExpression);
                    if (parameterTypeRef.IsSameRef(Sx) && returnTypeRef.IsSameRef(Tx))
                    {
                        if (bestConversion == null)
                            bestConversion = candidate;
                        else
                        {
                            bestConversion = null;
                            break;
                        }
                    }
                }
            }

            return bestConversion;
        }

        /// <summary>
        /// Get the IsPrivate access right of the object, and if not private then also get the IsProtected and IsInternal rights.
        /// </summary>
        protected void GetAccessRights(object obj, out bool isPrivate, out bool isProtected, out bool isInternal)
        {
            // Check for code objects
            if (obj is CodeObject)
            {
                if (obj is IModifiers)
                    ((IModifiers)obj).GetAccessRights(_unresolvedRef.IsTargetOfAssignment, out isPrivate, out isProtected, out isInternal);
                else
                    isPrivate = isProtected = isInternal = false;
                return;
            }

            // Check external objects.  For efficiency, get IsPrivate first, then continue only if necessary.
            // If anything goes wrong, we default to IsPrivate being true.
            isPrivate = true;
            isProtected = isInternal = false;
            bool isPublic = false;

            if (obj is MemberReference)
            {
                if (obj is TypeDefinition)
                {
                    TypeDefinition typeDefinition = (TypeDefinition)obj;
                    if (typeDefinition.IsNested)
                    {
                        isPrivate = typeDefinition.IsNestedPrivate;
                        if (!isPrivate)
                        {
                            isProtected = (typeDefinition.IsNestedFamily || typeDefinition.IsNestedFamilyOrAssembly);
                            isInternal = (typeDefinition.IsNestedAssembly || typeDefinition.IsNestedFamilyOrAssembly);
                            if (!isProtected && !isInternal)
                                isPublic = typeDefinition.IsNestedPublic;
                        }
                    }
                    else
                    {
                        isPrivate = false;
                        isInternal = typeDefinition.IsNotPublic;
                        if (!isInternal)
                            isPublic = typeDefinition.IsPublic;
                    }
                }
                else if (obj is MethodDefinition)
                {
                    MethodDefinition methodDefinition = (MethodDefinition)obj;
                    isPrivate = methodDefinition.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = (methodDefinition.IsFamily || methodDefinition.IsFamilyOrAssembly);
                        isInternal = (methodDefinition.IsAssembly || methodDefinition.IsFamilyOrAssembly);
                        if (!isProtected && !isInternal)
                            isPublic = methodDefinition.IsPublic;
                    }
                }
                else if (obj is PropertyDefinition)
                {
                    // The access rights of a property/indexer actually depend on the rights of the corresponding
                    // getter/setter, depending upon whether we're assigning to it or not.
                    PropertyDefinition propertyDefinition = (PropertyDefinition)obj;
                    MethodDefinition methodDefinition = (_unresolvedRef.IsTargetOfAssignment
                                                             ? propertyDefinition.SetMethod ?? propertyDefinition.GetMethod
                                                             : propertyDefinition.GetMethod ?? propertyDefinition.SetMethod);
                    if (methodDefinition != null)
                    {
                        isPrivate = methodDefinition.IsPrivate;
                        if (!isPrivate)
                        {
                            isProtected = (methodDefinition.IsFamily || methodDefinition.IsFamilyOrAssembly);
                            isInternal = (methodDefinition.IsAssembly || methodDefinition.IsFamilyOrAssembly);
                            if (!isProtected && !isInternal)
                                isPublic = methodDefinition.IsPublic;
                        }
                    }
                }
                else if (obj is FieldDefinition)
                {
                    FieldDefinition fieldDefinition = (FieldDefinition)obj;
                    isPrivate = fieldDefinition.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = (fieldDefinition.IsFamily || fieldDefinition.IsFamilyOrAssembly);
                        isInternal = (fieldDefinition.IsAssembly || fieldDefinition.IsFamilyOrAssembly);
                        if (!isProtected && !isInternal)
                            isPublic = fieldDefinition.IsPublic;
                    }
                }
                else if (obj is EventDefinition)
                {
                    // The access rights of an event actually depend on the rights of the corresponding
                    // adder/remover, depending upon whether we're assigning to it or not.
                    EventDefinition eventDefinition = (EventDefinition)obj;
                    MethodDefinition methodDefinition = (_unresolvedRef.IsTargetOfAssignment ? eventDefinition.AddMethod : eventDefinition.RemoveMethod);
                    if (methodDefinition != null)
                    {
                        isPrivate = methodDefinition.IsPrivate;
                        if (!isPrivate)
                        {
                            isProtected = (methodDefinition.IsFamily || methodDefinition.IsFamilyOrAssembly);
                            isInternal = (methodDefinition.IsAssembly || methodDefinition.IsFamilyOrAssembly);
                            if (!isProtected && !isInternal)
                                isPublic = methodDefinition.IsPublic;
                        }
                    }
                }
                else //if (obj is GenericParameter)
                {
                    isPrivate = false;
                    isPublic = true;
                }
            }
            else if (obj is ParameterDefinition)
            {
                isPrivate = false;
                isPublic = true;
            }
            else if (obj is MemberInfo)
            {
                if (obj is Type)
                {
                    Type type = (Type)obj;
                    if (type.IsNested)
                    {
                        isPrivate = type.IsNestedPrivate;
                        if (!isPrivate)
                        {
                            isProtected = (type.IsNestedFamily || type.IsNestedFamORAssem);
                            isInternal = (type.IsNestedAssembly || type.IsNestedFamORAssem);
                            if (!isProtected && !isInternal)
                                isPublic = type.IsNestedPublic;
                        }
                    }
                    else
                    {
                        isPrivate = false;
                        isInternal = type.IsNotPublic;
                        if (!isInternal)
                            isPublic = type.IsPublic;
                    }
                }
                else if (obj is MethodBase)
                {
                    MethodBase methodBase = (MethodBase)obj;
                    isPrivate = methodBase.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = (methodBase.IsFamily || methodBase.IsFamilyOrAssembly);
                        isInternal = (methodBase.IsAssembly || methodBase.IsFamilyOrAssembly);
                        if (!isProtected && !isInternal)
                            isPublic = methodBase.IsPublic;
                    }
                }
                else if (obj is PropertyInfo)
                {
                    // The access rights of a property/indexer actually depend on the rights of the corresponding
                    // getter/setter, depending upon whether we're assigning to it or not.
                    PropertyInfo propertyInfo = (PropertyInfo)obj;
                    MethodInfo methodInfo = (_unresolvedRef.IsTargetOfAssignment
                                                 ? propertyInfo.GetSetMethod(true) ?? propertyInfo.GetGetMethod(true)
                                                 : propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true));
                    if (methodInfo != null)
                    {
                        isPrivate = methodInfo.IsPrivate;
                        if (!isPrivate)
                        {
                            isProtected = (methodInfo.IsFamily || methodInfo.IsFamilyOrAssembly);
                            isInternal = (methodInfo.IsAssembly || methodInfo.IsFamilyOrAssembly);
                            if (!isProtected && !isInternal)
                                isPublic = methodInfo.IsPublic;
                        }
                    }
                }
                else if (obj is FieldInfo)
                {
                    FieldInfo fieldInfo = (FieldInfo)obj;
                    isPrivate = fieldInfo.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = (fieldInfo.IsFamily || fieldInfo.IsFamilyOrAssembly);
                        isInternal = (fieldInfo.IsAssembly || fieldInfo.IsFamilyOrAssembly);
                        if (!isProtected && !isInternal)
                            isPublic = fieldInfo.IsPublic;
                    }
                }
                else if (obj is EventInfo)
                {
                    // The access rights of an event actually depend on the rights of the corresponding
                    // adder/remover, depending upon whether we're assigning to it or not.
                    EventInfo eventInfo = (EventInfo)obj;
                    MethodInfo methodInfo = (_unresolvedRef.IsTargetOfAssignment ? eventInfo.GetAddMethod(true) : eventInfo.GetRemoveMethod(true));
                    if (methodInfo != null)
                    {
                        isPrivate = methodInfo.IsPrivate;
                        if (!isPrivate)
                        {
                            isProtected = (methodInfo.IsFamily || methodInfo.IsFamilyOrAssembly);
                            isInternal = (methodInfo.IsAssembly || methodInfo.IsFamilyOrAssembly);
                            if (!isProtected && !isInternal)
                                isPublic = methodInfo.IsPublic;
                        }
                    }
                }
            }
            else if (obj is ParameterInfo)
            {
                isPrivate = false;
                isPublic = true;
            }

            // It's possible for external types (from IL or other languages) to actually not have ANY
            // access rights set, so default to Private in such a situation.
            if (!isPrivate && !isProtected && !isInternal && !isPublic)
                isPrivate = true;
        }

        /// <summary>
        /// Find the better conversion of a type to two candidate types.
        /// Returns 1 if the conversion to typeRefBase1 is better, 2 if the conversion to typeRefBase2 is better, 0 if neither is better.
        /// </summary>
        protected static int FindBetterConversion(TypeRefBase typeRefBase, TypeRefBase typeRefBase1, TypeRefBase typeRefBase2)
        {
            // Given a conversion C1 that converts from a type S to a type T1, and a conversion C2 that converts from
            // a type S to a type T2, C1 is a better conversion than C2 if at least one of the following holds:

            // - An identity conversion exists from S to T1 but not from S to T2
            bool identity1 = typeRefBase.ImplicitIdentityConversionExists(typeRefBase1);
            bool identity2 = typeRefBase.ImplicitIdentityConversionExists(typeRefBase2);
            if (identity1 && !identity2)
                return 1;
            if (identity2 && !identity1)
                return 2;

            // - T1 is a better conversion target than T2
            return FindBetterConversionTarget(typeRefBase1, typeRefBase2);
        }

        /// <summary>
        /// Find the better conversion target of two candidate types.
        /// Returns 1 if typeRefBase1 is better, 2 if typeRefBase2 is better, 0 if neither is better.
        /// </summary>
        protected static int FindBetterConversionTarget(TypeRefBase typeRefBase1, TypeRefBase typeRefBase2)
        {
            // Given two different types T1 and T2, T1 is a better conversion target than T2 if at least one of the following holds:

            // - An implicit conversion from T1 to T2 exists, and no implicit conversion from T2 to T1 exists
            bool convert1To2 = typeRefBase1.IsImplicitlyConvertibleTo(typeRefBase2);
            bool convert2To1 = typeRefBase2.IsImplicitlyConvertibleTo(typeRefBase1);
            if (convert1To2 && !convert2To1)
                return 1;
            if (convert2To1 && !convert1To2)
                return 2;

            if (typeRefBase1 is TypeRef && typeRefBase2 is TypeRef)
            {
                TypeRef typeRef1 = (TypeRef)typeRefBase1;
                TypeRef typeRef2 = (TypeRef)typeRefBase2;

                // - T1 is a signed integral type and T2 is an unsigned integral type or vice-versa. Specifically:
                if (typeRef1.IsPrimitive && typeRef2.IsPrimitive)
                {
                    TypeCode typeCode1 = typeRef1.GetTypeCode();
                    TypeCode typeCode2 = typeRef2.GetTypeCode();

                    // - T1 is sbyte and T2 is byte, ushort, uint, or ulong
                    if (typeCode1 == TypeCode.SByte && (typeCode2 == TypeCode.Byte || typeCode2 == TypeCode.UInt16 || typeCode2 == TypeCode.UInt32 || typeCode2 == TypeCode.UInt64))
                        return 1;
                    if (typeCode2 == TypeCode.SByte && (typeCode1 == TypeCode.Byte || typeCode1 == TypeCode.UInt16 || typeCode1 == TypeCode.UInt32 || typeCode1 == TypeCode.UInt64))
                        return 2;
                    // - T1 is short and T2 is ushort, uint, or ulong
                    if (typeCode1 == TypeCode.Int16 && (typeCode2 == TypeCode.UInt16 || typeCode2 == TypeCode.UInt32 || typeCode2 == TypeCode.UInt64))
                        return 1;
                    if (typeCode2 == TypeCode.Int16 && (typeCode1 == TypeCode.UInt16 || typeCode1 == TypeCode.UInt32 || typeCode1 == TypeCode.UInt64))
                        return 2;
                    // - T1 is int and T2 is uint, or ulong
                    if (typeCode1 == TypeCode.Int32 && (typeCode2 == TypeCode.UInt32 || typeCode2 == TypeCode.UInt64))
                        return 1;
                    if (typeCode2 == TypeCode.Int32 && (typeCode1 == TypeCode.UInt32 || typeCode1 == TypeCode.UInt64))
                        return 2;
                    // - T1 is long and T2 is ulong
                    if (typeCode1 == TypeCode.Int64 && typeCode2 == TypeCode.UInt64)
                        return 1;
                    if (typeCode2 == TypeCode.Int64 && typeCode1 == TypeCode.UInt64)
                        return 2;
                }
            }

            // Neither conversion is better
            return 0;
        }

        /// <summary>
        /// Find the more specific of two candidate types.
        /// Returns 1 if typeRef1 is more specific, 2 if typeRef2 is more specific, 0 if neither is more specific.
        /// </summary>
        protected static int FindMoreSpecificType(TypeRefBase typeRefBase1, TypeRefBase typeRefBase2)
        {
            // Handle unresolved types
            if (typeRefBase1 == null || typeRefBase2 == null || typeRefBase1 is UnresolvedRef || typeRefBase2 is UnresolvedRef)
                return 0;

            TypeRef typeRef1 = (TypeRef)typeRefBase1;
            TypeRef typeRef2 = (TypeRef)typeRefBase2;

            // A type parameter is less specific than a non-type parameter.
            if (!typeRef1.IsGenericParameter && typeRef2.IsGenericParameter)
                return 1;
            if (!typeRef2.IsGenericParameter && typeRef1.IsGenericParameter)
                return 2;

            // Recursively, a constructed type is more specific than another constructed type (with the same number of type arguments) if at
            // least one type argument is more specific and no type argument is less specific than the corresponding type argument in the other.
            if (typeRef1.IsGenericType && typeRef2.IsGenericType)
            {
                ChildList<Expression> type1Arguments = typeRef1.TypeArguments;
                ChildList<Expression> type2Arguments = typeRef2.TypeArguments;
                if (type1Arguments.Count == type2Arguments.Count)
                {
                    bool type1ArgumentIsMoreSpecific = false;
                    bool type2ArgumentIsMoreSpecific = false;
                    for (int index = 0; index < type1Arguments.Count; ++index)
                    {
                        int result = FindMoreSpecificType(type1Arguments[index].EvaluateType(), type2Arguments[index].EvaluateType());
                        if (result == 1)
                            type1ArgumentIsMoreSpecific = true;
                        else if (result == 2)
                            type2ArgumentIsMoreSpecific = true;
                    }
                    if (type1ArgumentIsMoreSpecific && !type2ArgumentIsMoreSpecific)
                        return 1;
                    if (type2ArgumentIsMoreSpecific && !type1ArgumentIsMoreSpecific)
                        return 2;
                }
            }

            if (typeRef1.IsArray)
            {
                if (typeRef2.IsArray)
                {
                    // An array type is more specific than another array type (with the same number of dimensions) if the element type of the first
                    // is more specific than the element type of the second.
                    if (CollectionUtil.CompareList(typeRef1.ArrayRanks, typeRef2.ArrayRanks))
                        return FindMoreSpecificType(typeRef1.GetElementType(), typeRef2.GetElementType());
                }
                else
                    return 1;  // An array type is more specific than a non-array type
            }
            else if (typeRef2.IsArray)
                return 2;  // An array type is more specific than a non-array type

            return 0;
        }

        /// <summary>
        /// Get the parameter at the specified index without evaluating any type parameters.
        /// </summary>
        protected static bool GetParameterType(ICollection parameters, int index, ref TypeRefBase parameterTypeRef, TypeRefBase argumentTypeRef)
        {
            // If the index exceeds the parameter collection size, abort (re-use the last type)
            if (index >= parameters.Count)
                return true;  // Indicate that expanded form is in use

            // Get the parameter type
            if (parameters is List<ParameterDecl>)
                parameterTypeRef = (((List<ParameterDecl>)parameters)[index].EvaluateType());
            else if (parameters is Collection<ParameterDefinition>)
                parameterTypeRef = TypeRef.Create(((Collection<ParameterDefinition>)parameters)[index].ParameterType);
            else //if (parameters is ParameterInfo[])
                parameterTypeRef = TypeRef.Create(((ParameterInfo[])parameters)[index].ParameterType);

            if (parameterTypeRef != null)
            {
                // Check for a 'params' parameter if we're on the last one
                if (index == parameters.Count - 1 && ParameterRef.ParameterIsParams(parameters, index) && parameterTypeRef.IsArray && argumentTypeRef != null)
                {
                    // If the argument has fewer total array ranks, or the first one isn't one-dimensional,
                    // then use the expanded form.  We specifically don't want to use the expanded form whenever
                    // the types don't match exactly for normal form, because FindBetterMethod() will then drop
                    // possible matches in favor of non-expanded ones even though there was no exact match.
                    if ((argumentTypeRef.ArrayRanks != null ? argumentTypeRef.ArrayRanks.Count : 0) < parameterTypeRef.ArrayRanks.Count || argumentTypeRef.ArrayRanks[0] != 1)
                    {
                        parameterTypeRef = parameterTypeRef.GetElementType();
                        return true;  // Indicate that expanded form is in use
                    }
                }
            }
            return false;
        }

        protected static bool IsMethodGeneric(object obj)
        {
            if (obj is MethodDeclBase)
                return ((MethodDeclBase)obj).IsGenericMethod;
            if (obj is MethodDefinition)
                return ((MethodDefinition)obj).HasGenericParameters;
            if (obj is MethodBase)  // MethodInfo or ConstructorInfo
                return ((MethodBase)obj).IsGenericMethod;
            return false;
        }

        /// <summary>
        /// Find the better conversion of an expression to two candidate types.
        /// Returns 1 if the conversion to typeRef1 is better, 2 if the conversion to typeRef2 is better, 0 if neither is better.
        /// </summary>
        protected int FindBetterConversion(Expression expression, TypeRefBase typeRefBase1, TypeRefBase typeRefBase2)
        {
            // Given an implicit conversion C1 that converts from an expression E to a type T1, and an implicit conversion C2 that
            // converts from an expression E to a type T2, C1 is a better conversion than C2 if at least one of the following holds:
            if (expression == null)
                return 0;

            // - E has a type S and an identity conversion exists from S to T1 but not from S to T2
            TypeRefBase expressionType = expression.EvaluateType();
            bool identity1 = expressionType.ImplicitIdentityConversionExists(typeRefBase1);
            bool identity2 = expressionType.ImplicitIdentityConversionExists(typeRefBase2);
            if (identity1 && !identity2)
                return 1;
            if (identity2 && !identity1)
                return 2;

            // - E is not an anonymous function and T1 is a better conversion target than T2
            if (!(expression is AnonymousMethod))
                return FindBetterConversionTarget(typeRefBase1, typeRefBase2);

            // - E is an anonymous function, T1 is a delegate type D1, T2 is a delegate type D2 and one of the following holds:
            if (typeRefBase1.IsDelegateType && typeRefBase2.IsDelegateType)
            {
                // - D1 is a better conversion target than D2
                int result = FindBetterConversionTarget(typeRefBase1, typeRefBase2);
                if (result > 0)
                    return result;

                // - D1 and D2 have identical parameter lists, and one of the following holds:
                ICollection typeRef1Parameters = typeRefBase1.GetDelegateParameters();
                ICollection typeRef2Parameters = typeRefBase2.GetDelegateParameters();
                int typeRef1ParameterCount = (typeRef1Parameters != null ? typeRef1Parameters.Count : 0);
                if (typeRef1ParameterCount == (typeRef2Parameters != null ? typeRef2Parameters.Count : 0))
                {
                    bool parametersMatch = true;
                    for (int i = 0; i < typeRef1ParameterCount; ++i)
                    {
                        TypeRefBase parameterType1 = ParameterRef.GetParameterType(typeRef1Parameters, i, typeRefBase1);
                        TypeRefBase parameterType2 = ParameterRef.GetParameterType(typeRef2Parameters, i, typeRefBase2);
                        if (!parameterType1.IsSameRef(parameterType2))
                        {
                            parametersMatch = false;
                            break;
                        }
                    }
                    if (parametersMatch)
                    {
                        // - D1 has a return type Y1, and D2 has a return type Y2, an inferred return type X exists for E in the
                        //   context of that parameter list, and the conversion from X to Y1 is better than the conversion from X to Y2
                        // - D1 has a return type Y, and D2 is void returning
                        AnonymousMethod anonymousMethod = (AnonymousMethod)expression;
                        TypeRefBase returnType = anonymousMethod.GetReturnType();
                        TypeRefBase returnType1 = typeRefBase1.GetDelegateReturnType();
                        TypeRefBase returnType2 = typeRefBase2.GetDelegateReturnType();
                        if (returnType2 == null)
                            return (returnType1 == null ? 0 : 1);
                        if (returnType1 == null)
                            return 2;
                        returnType1 = returnType1.EvaluateTypeArgumentTypes(typeRefBase1);
                        returnType2 = returnType2.EvaluateTypeArgumentTypes(typeRefBase2);
                        bool returnType1Void = returnType1.IsSameRef(TypeRef.VoidRef);
                        bool returnType2Void = returnType2.IsSameRef(TypeRef.VoidRef);
                        if (returnType2Void && !returnType1Void)
                            return 1;
                        if (returnType1Void && !returnType2Void)
                            return 2;
                        return FindBetterConversion(returnType, returnType1, returnType2);
                    }
                }
            }

            // Neither conversion is better
            return 0;
        }

        /// <summary>
        /// Find the method that better matches the specified arguments.
        /// Returns the better-matching method object, or null if neither is better.
        /// Assumes that the two candidate methods have already been determined to be valid matches for
        /// the given arguments, using implicit conversions where necessary.
        /// </summary>
        /// <param name="methodCandidate1">MatchCandidate for 1st method object.</param>
        /// <param name="methodCandidate2">MatchCandidate for 2nd method object.</param>
        /// <param name="arguments">The method arguments.</param>
        /// <param name="unresolvedRef">The parent UnresolvedRef used to track down generic type parameter types.</param>
        /// <returns>The better method object.</returns>
        protected object FindBetterMethod(MatchCandidate methodCandidate1, MatchCandidate methodCandidate2, List<Expression> arguments, UnresolvedRef unresolvedRef)
        {
            // Get the method parameters for the objects to be compared
            object method1 = methodCandidate1.Object;
            object method2 = methodCandidate2.Object;
            ICollection parameters1 = MethodRef.GetParameters(method1);
            ICollection parameters2 = MethodRef.GetParameters(method2);
            int originalParameters1Count = (parameters1 != null ? parameters1.Count : 0);
            int originalParameters2Count = (parameters2 != null ? parameters2.Count : 0);
            int parameters1Count = originalParameters1Count;
            int parameters2Count = originalParameters2Count;
            int argumentsCount = (arguments != null ? arguments.Count : 0);

            if (argumentsCount == 0)
            {
                // Special handling for an empty argument list:
                // If one method has FEWER parameters, it's better.
                if (parameters1Count < parameters2Count)
                    return method1;
                if (parameters2Count < parameters1Count)
                    return method2;
            }

            // At this point, we know we have at least one argument, so:
            // If either parameter list is empty, then the other is better, or neither if they're both empty
            if (parameters1Count == 0)
                return (parameters2Count == 0 ? null : method2);
            if (parameters2Count == 0)
                return method1;

            // Unlike the spec, we allow ref/out mismatches (although the type must match exactly) in AddMatch(),
            // relying on the analysis phase to flag them as errors.  Since it's possible to overload methods
            // with only ref/out differences, we could get dups, and need to filter them here:
            bool method1ParameterMismatch = false;
            bool method2ParameterMismatch = false;
            for (int index = 0; index < arguments.Count; ++index)
            {
                bool argIsRef = (arguments[index] is Ref);
                bool argIsOut = (arguments[index] is Out);
                bool isRef1, isOut1, isRef2, isOut2;
                ParameterRef.GetParameterType(parameters1, index, out isRef1, out isOut1);
                ParameterRef.GetParameterType(parameters2, index, out isRef2, out isOut2);
                if (!method1ParameterMismatch)
                    method1ParameterMismatch = (isRef1 != argIsRef || isOut1 != argIsOut);
                if (!method2ParameterMismatch)
                    method2ParameterMismatch = (isRef2 != argIsRef || isOut2 != argIsOut);
            }
            // Prefer a method that doesn't have ref/out mismatches over one that does
            if (!method1ParameterMismatch && method2ParameterMismatch)
                return method1;
            if (!method2ParameterMismatch && method1ParameterMismatch)
                return method2;

            // Given an argument list A with a set of argument expressions { E1, E2, ..., EN } and two applicable function
            // members MP and MQ with parameter types { P1, P2, ..., PN } and { Q1, Q2, ..., QN }, MP is defined to be a
            // better function member than MQ if:
            //   - for each argument, the implicit conversion from EX to QX is not better than the implicit conversion from EX to PX, and
            //   - for at least one argument, the conversion from EX to PX is better than the conversion from EX to QX.
            bool method1IsExpanded = false;
            bool method2IsExpanded = false;
            bool method1ParameterIsBetter = false;
            bool method2ParameterIsBetter = false;
            TypeRefBase typeRefBase1 = null;
            TypeRefBase typeRefBase2 = null;

            // Determine the parent object for purposes of evaluating type arguments.  If there are inferred type arguments
            // for either method, then move them to a temporary MethodRef so they can be used in the evaluation.
            CodeObject parent = unresolvedRef.Parent;
            parent = ((parent is Dot && ((Dot)parent).Right == unresolvedRef) ? ((Dot)parent).Left : parent.Parent);
            MethodRef method1Inferred = null;
            MethodRef method2Inferred = null;
            if (methodCandidate1.InferredTypeArguments != null)
            {
                method1Inferred = (MethodRef)unresolvedRef.CreateRef(method1, true);
                methodCandidate1.CopyInferredTypeArguments(method1Inferred);
            }
            if (methodCandidate2.InferredTypeArguments != null)
            {
                method2Inferred = (MethodRef)unresolvedRef.CreateRef(method2, true);
                methodCandidate2.CopyInferredTypeArguments(method2Inferred);
            }

            // Check the arguments of the two methods to see if one of them has better type conversions
            for (int index = 0; index < arguments.Count; ++index)
            {
                TypeRefBase argumentTypeRef = arguments[index].EvaluateType();

                // Get the parameter of each method at the current position, evaluating any type parameters,
                // and determine if the argument type has a better conversion for one of the two types.
                // Evaluate type parameters using any temporary MethodRefs with inferred types, but always
                // also check the 'parent', since there might be a generic declaring type with type arguments.
                method1IsExpanded = GetParameterType(parameters1, index, ref typeRefBase1, argumentTypeRef);
                if (method1Inferred != null)
                    typeRefBase1 = typeRefBase1.EvaluateTypeArgumentTypes(method1Inferred);
                typeRefBase1 = typeRefBase1.EvaluateTypeArgumentTypes(parent);
                method2IsExpanded = GetParameterType(parameters2, index, ref typeRefBase2, argumentTypeRef);
                if (method2Inferred != null)
                    typeRefBase2 = typeRefBase2.EvaluateTypeArgumentTypes(method2Inferred);
                typeRefBase2 = typeRefBase2.EvaluateTypeArgumentTypes(parent);
                int result = FindBetterConversion(arguments[index], typeRefBase1, typeRefBase2);
                if (result == 1)
                    method1ParameterIsBetter = true;
                else if (result == 2)
                    method2ParameterIsBetter = true;
            }

            // Dispose any temporary MethodRefs
            if (method1Inferred != null)
                method1Inferred.Dispose();
            if (method2Inferred != null)
                method2Inferred.Dispose();

            // Determine if one method is better than the other
            if (method1ParameterIsBetter && !method2ParameterIsBetter)
                return method1;
            if (method2ParameterIsBetter && !method1ParameterIsBetter)
                return method2;

            // If either method has more parameters than the available arguments, meaning that it has a params array
            // that isn't being used, then that is also considered to be "expanded form".
            if (parameters1Count > argumentsCount)
                method1IsExpanded = true;
            if (parameters2Count > argumentsCount)
                method2IsExpanded = true;

            // TIE BREAKING RULES:
            // In case the parameter type sequences {P1, P2, …, PN} and {Q1, Q2, …, QN} are equivalent (each Pi has an
            // identity conversion to the corresponding Qi), the following tie-breaking rules are applied, in order, to
            // determine the better function member:

            // If MP is a non-generic method and MQ is a generic method, then MP is better than MQ.
            bool method1Generic = IsMethodGeneric(method1);
            bool method2Generic = IsMethodGeneric(method2);
            if (!method1Generic && method2Generic)
                return method1;
            if (!method2Generic && method1Generic)
                return method2;

            // Otherwise, if MP is applicable in its normal form and MQ has a params array and is applicable only in
            // its expanded form, then MP is better than MQ.
            if (!method1IsExpanded && method2IsExpanded)
                return method1;
            if (!method2IsExpanded && method1IsExpanded)
                return method2;
            // Otherwise, if MP has more declared parameters than MQ, then MP is better than MQ. This can occur if
            // both methods have params arrays and are applicable only in their expanded forms.
            if (parameters1Count > parameters2Count)
                return method1;
            if (parameters2Count > parameters1Count)
                return method2;

            // Otherwise, if MP has more specific parameter types than MQ, then MP is better than MQ. Let {R1, R2, …, RN} and {S1, S2, …, SN}
            // represent the uninstantiated and unexpanded parameter types of MP and MQ. MP’s parameter types are more specific than MQ’s if,
            // for each parameter, RX is not less specific than SX, and, for at least one parameter, RX is more specific than SX:
            //   - A type parameter is less specific than a non-type parameter.
            //   - Recursively, a constructed type is more specific than another constructed type (with the same number of type arguments) if at
            //     least one type argument is more specific and no type argument is less specific than the corresponding type argument in the other.
            //   - An array type is more specific than another array type (with the same number of dimensions) if the element type of the first
            //     is more specific than the element type of the second.
            //   - An array type is more specific than a non-array type.
            //     For example: M<T>(T[] p) where T=string is better than M<T>(T p) where T=string[], because T[] is more specific than T
            bool method1ParameterIsMoreSpecific = false;
            bool method2ParameterIsMoreSpecific = false;
            typeRefBase1 = null;
            typeRefBase2 = null;
            for (int index = 0; index < parameters1Count; ++index)  // The parameter counts for the methods should be the same
            {
                // The arguments count might be smaller due to an unused 'params' parameter
                TypeRefBase argumentTypeRef = (index < argumentsCount ? arguments[index].EvaluateType() : null);
                // Don't evaluate type arguments for the parameters here - we need to know if they are type arguments
                GetParameterType(parameters1, index, ref typeRefBase1, argumentTypeRef);
                GetParameterType(parameters2, index, ref typeRefBase2, argumentTypeRef);
                int result = FindMoreSpecificType(typeRefBase1, typeRefBase2);
                if (result == 1)
                    method1ParameterIsMoreSpecific = true;
                else if (result == 2)
                    method2ParameterIsMoreSpecific = true;
            }
            if (method1ParameterIsMoreSpecific && !method2ParameterIsMoreSpecific)
                return method1;
            if (method2ParameterIsMoreSpecific && !method1ParameterIsMoreSpecific)
                return method2;

            // Otherwise, neither function member is better.
            return null;
        }

        /// <summary>
        /// Add matching code object(s).
        /// </summary>
        /// <param name="obj">The CodeObject, MemberInfo, ICollection, or null.</param>
        public void AddMatch(object obj)
        {
            if (obj != null)
            {
                if (obj is IEnumerable)
                {
                    // Breakup collections into individual objects
                    foreach (object @object in (IEnumerable)obj)
                        AddMatchInternal(@object);
                }
                else
                    AddMatchInternal(obj);
            }
        }

        /// <summary>
        /// Get any CoClass attribute type for the specified object.
        /// </summary>
        protected static object GetCoClassAttributeType(object obj)
        {
            const string CoClassAttributeName = "CoClassAttribute";
            object coClassType = null;
            if (obj is ITypeDecl)
            {
                Expression attributeExpression = ((ITypeDecl)obj).GetAttribute(CoClassAttributeName);
                if (attributeExpression is Call)
                {
                    ChildList<Expression> arguments = ((Call)attributeExpression).Arguments;
                    if (arguments != null && arguments.Count > 0 && arguments[0] is TypeOf)
                        coClassType = ((TypeOf)arguments[0]).Expression.EvaluateType().Reference;
                }
            }
            else if (obj is TypeDefinition)
            {
                CustomAttribute customAttribute = ICustomAttributeProviderUtil.GetCustomAttribute((TypeDefinition)obj, CoClassAttributeName);
                if (customAttribute != null)
                    coClassType = customAttribute.ConstructorArguments[0].Value;
            }
            else //if (obj is Type)
            {
                CustomAttributeData customAttributeData = MemberInfoUtil.GetCustomAttribute((Type)obj, CoClassAttributeName);
                if (customAttributeData != null)
                    coClassType = customAttributeData.ConstructorArguments[0].Value;
            }
            return coClassType;
        }

        protected void AddMatchInternal(object obj, bool lookInAllParts, TypeRefBase parentTypeRef)
        {
            // Also treat a Method category as a constructor category if we're in a DocCodeRefBase
            bool isConstructorCategory = (ResolveCategoryHelpers.IsConstructor[(int)_resolveCategory] || IsDocCodeRefToMethod);

            // If we're looking for a constructor and we found a type with the same name, get the constructors on the type instead
            if (isConstructorCategory && (obj is ITypeDecl || obj is TypeDefinition || obj is Type))
            {
                // If the type is a partial type, then only look in all parts if instructed (this is used by NewObject when looking for
                // all constructors on the already-resolved TypeRef, otherwise we only want to find constructors on the current part,
                // since we will end up searching, finding, and calling this method for all parts of the type).
                NamedCodeObjectGroup constructors = TypeRef.GetConstructors(obj, !lookInAllParts);
                if (constructors != null && constructors.Count > 0)
                {
                    // We may have found constructors from inside the type on the way up the code tree, so only add the constructors
                    // if the first one doesn't already exist.  This can only occur for ITypeDecls, not for TypeDefinition/Types.
                    if (obj is TypeDefinition || obj is Type || !_unresolvedRef.HasMatches || !_unresolvedRef.Matches.Contains((CodeObject)constructors[0]))
                        AddMatch(constructors);
                }
                else
                {
                    // If there aren't any constructors on the type, check for a CoClassAttribute type, and add it if found
                    object coClassType = GetCoClassAttributeType(obj);
                    if (coClassType != null)
                        AddMatchInternal(coClassType, lookInAllParts, parentTypeRef);
                }

                return;
            }

            // Create a wrapper object for the match candidate
            MatchCandidate candidate = new MatchCandidate(obj, _unresolvedRef, _resolveFlags);

            // Check that the type of the object is valid for the category (if the category is null, allow it to pass - this
            // is used by UnresolvedRef.ResolveMethodGroup() when resolving method groups against a delegate type).
            if (_isValidCategory == null || _isValidCategory(obj))
            {
                candidate.IsCategoryMatch = true;

                // If there are type arguments, the match isn't complete if the counts don't match
                if (!DoTypeArgumentCountsMatch(obj))
                    candidate.IsTypeArgumentsMatch = false;

                bool isMethodCategory = ResolveCategoryHelpers.IsMethod[(int)_resolveCategory];

                // If there are method arguments, the match isn't complete unless they match.
                // If we have a Method category, then we know that we're resolving a method or delegate invocation.  Our parent
                // will be an ArgumentsOperator (Call, Index, or NewObject) - or our grandparent if our parent is a Dot - but we
                // don't need to verify that here (the subroutine below will take care of that).  We check the category instead
                // of the type of the object because it's easier than checking all the Method category delegate types.
                // If we have a method object, but our category wasn't Method, that means the category is Expression (we don't
                // need to verify that), and we've got a "method group" being passed as an argument or initializing a variable.
                // In this case, we need to find the delegate type we're being assigned to, and get our "arguments" from it.
                if (isMethodCategory || candidate.IsMethod)
                {
                    // Ignore overridden methods.  We must ignore 'override' methods so that we match the 'virtual' declaration
                    // in the base class, so that any overloads in the base class are also considered (per the spec).
                    if (MethodRef.IsOverridden(obj))
                        return;

                    bool argumentsMismatchDueToUnresolvedOnly = true;

                    // Get the method arguments
                    bool hasUnresolvedDelegateTypes;
                    List<Expression> methodArguments = GetParentArguments(isMethodCategory, out hasUnresolvedDelegateTypes);
                    if (methodArguments == null)
                    {
                        // Null means either no arguments exist (no parens were specified AND no associated delegate type exists,
                        // such as "MethodName is object" or "MethodName" in a doc comment cref), or they couldn't be determined
                        // due to the associated delegate type being unresolved.  Treat the first as a match, the 2nd as an error.
                        if (hasUnresolvedDelegateTypes)
                            candidate.IsMethodArgumentsMatch = false;
                    }
                    else
                    {
                        // Get the parameters of the method or delegate type
                        ICollection parameters = MethodRef.GetParameters(obj, _unresolvedRef);
                        int parameterCount = (parameters != null ? parameters.Count : 0);

                        // Perform Type Inference if the candidate has inferred types
                        if (candidate.InferredTypeArguments != null && parameterCount > 0)
                        {
                            // For now, if type inference fails OR if any of the inferred types are unresolved, treat it as the type arguments not matching, so
                            // that it will be displayed as an error (instead of possibly as a warning).  We might want to eventually improve the error reporting
                            // to distinguish between these events and normal type argument mismatches.
                            if (candidate.InferTypeArguments(parameters, methodArguments, _unresolvedRef))
                            {
                                // Evaluate any inferred OpenTypeParameterRefs in any parent expression or type declarations now that
                                // type inference is complete (this would cause problems if it were done during type inference).
                                for (int i = 0; i < candidate.InferredTypeArguments.Length; ++i)
                                {
                                    TypeRefBase typeRefBase = candidate.InferredTypeArguments[i];
                                    if (typeRefBase is OpenTypeParameterRef)
                                        candidate.InferredTypeArguments[i] = typeRefBase.EvaluateTypeArgumentTypes(_unresolvedRef.Parent, _unresolvedRef);
                                }

                                // Fail if any of the inferred types are unresolved (unless we're in a doc comment)
                                if (Enumerable.Any(candidate.InferredTypeArguments, delegate (TypeRefBase typeRef) { return typeRef.HasUnresolvedRef(); }) && !_resolveFlags.HasFlag(ResolveFlags.InDocComment))
                                    candidate.IsTypeInferenceFailure = true;
                            }
                            else
                                candidate.IsTypeInferenceFailure = true;
                        }

                        // Determine if the parameter count and types match.  A special first pass (-1) is done to match or
                        // eliminate zero parameters, and a final pass (maxIndex) eliminates extra parameters.
                        TypeRefBase paramsTypeRef = null;
                        int argumentCount = methodArguments.Count;
                        int maxIndex = (argumentCount == 0 || parameterCount == 0 ? -1 : argumentCount);
                        for (int i = -1; i <= maxIndex; ++i)
                        {
                            bool parameterMatches;
                            bool failedBecauseUnresolved = false;
                            if (i == -1)
                            {
                                // An index of -1 means we're matching or excluding an empty parameter list
                                if (argumentCount == 0)
                                    parameterMatches = (parameterCount == 0 || (parameterCount == 1 && ParameterRef.ParameterIsParams(parameters, 0)));
                                else
                                {
                                    parameterMatches = (parameterCount > 0);
                                    if (!parameterMatches)
                                    {
                                        // Check if we failed to match because of a variable with an unresolved type (instead of a delegate type)
                                        if (obj is IVariableDecl && ((CodeObject)obj).HasUnresolvedRef())
                                            failedBecauseUnresolved = true;
                                    }
                                }
                            }
                            else if (i == maxIndex)
                            {
                                // An index of 'maxIndex' means we're matching on the total number of arguments.
                                // We also have to check for a 'params' parameter (used more than once, or unused).
                                parameterMatches = ((i == parameterCount) || (i >= parameterCount - 1 && ParameterRef.ParameterIsParams(parameters, parameterCount - 1)));
                            }
                            else
                            {
                                // Evaluate the argument type, and any type arguments if it's a nested generic type
                                TypeRefBase argumentTypeRef = methodArguments[i].EvaluateType();
                                if (argumentTypeRef is TypeRef && ((TypeRef)argumentTypeRef).IsNested && ((TypeRef)argumentTypeRef).IsGenericType)
                                    argumentTypeRef = argumentTypeRef.EvaluateTypeArgumentTypes(_unresolvedRef.Parent, _unresolvedRef);

                                // Check if the type of the current argument matches the method parameter type
                                parameterMatches = ParameterRef.MatchParameter(_unresolvedRef, obj, candidate, parameters, i,
                                    argumentCount, argumentTypeRef, ref paramsTypeRef, true, _parentExpression, out failedBecauseUnresolved);
                            }
                            if (!parameterMatches)
                            {
                                // Mark the first argument that fails to match
                                if (candidate.IsMethodArgumentsMatch)
                                {
                                    candidate.IsMethodArgumentsMatch = false;
                                    candidate.MethodArgumentMismatchIndex = i;
                                }
                                // If the failure was due to something other than an unresolved parameter, record
                                // it as such and skip checking any more parameters.
                                if (!failedBecauseUnresolved)
                                {
                                    argumentsMismatchDueToUnresolvedOnly = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (candidate.IsMethodArgumentsMatch)
                    {
                        // For user-defined explicit conversions, verify that the operator is applicable
                        if (_resolveCategory == ResolveCategory.OperatorOverload && _unresolvedRef.Name == "op_Explicit" && _unresolvedRef.Parent is Cast)
                        {
                            // Find operators that convert from a type encompassing or encompassed by S to a type encompassing or encompassed by T
                            TypeRefBase sourceTypeRef = methodArguments[0].EvaluateType();
                            TypeRefBase targetTypeRef = ((Cast)_unresolvedRef.Parent).Type.EvaluateType();
                            TypeRefBase parameterTypeRef = ParameterRef.GetParameterType(MethodRef.GetParameters(obj), 0, _parentExpression);
                            TypeRefBase returnTypeRef = MethodRef.GetReturnType(obj, _parentExpression);
                            bool matches = ((TypeRef.IsEncompassedBy(sourceTypeRef, parameterTypeRef) || TypeRef.IsEncompassedBy(parameterTypeRef, sourceTypeRef))
                                            && (TypeRef.IsEncompassedBy(targetTypeRef, returnTypeRef) || TypeRef.IsEncompassedBy(returnTypeRef, targetTypeRef)));
                            if (!matches && targetTypeRef is TypeRef && ((TypeRef)targetTypeRef).IsEnum)
                            {
                                // If the return type wasn't convertible to the cast type, and the cast type is an enum, get the underlying type
                                // of the enum and try again.
                                targetTypeRef = ((TypeRef)targetTypeRef).GetUnderlyingTypeOfEnum();
                                matches = (TypeRef.IsEncompassedBy(targetTypeRef, returnTypeRef) || TypeRef.IsEncompassedBy(returnTypeRef, targetTypeRef));
                            }
                            if (!matches)
                                candidate.IsMethodArgumentsMatch = false;  // Treat as an argument mismatch
                        }
                    }
                    else
                        candidate.ArgumentsMismatchDueToUnresolvedOnly = argumentsMismatchDueToUnresolvedOnly;
                }

                // Verify that the static modifier matches for cases where it matters:
                // If we have a parent expression, and the reference is to a property or method, static matters - if
                // the parent is a TypeRef, the reference must be static, otherwise it must not be.  If the reference
                // is a constructor, it must not be static (even though it might have a TypeRef for a parent if it's the
                // constructor of a nested type).  In all other situations, the reference can be either static or non-static.
                // Specifically, if the reference is to a type or namespace, don't check the static mode.
                // If we're evaluating a reference inside a 'cref' doc comment, ignore static checking (thus allowing static
                // references to non-static type members).
                if (((_parentExpression != null && ResolveCategoryHelpers.IsPropertyOrMethod[(int)_resolveCategory])
                    || isConstructorCategory) && !(obj is ITypeDecl || obj is TypeDefinition || obj is Type || obj is Namespace) && !_resolveFlags.HasFlag(ResolveFlags.InDocCodeRef))
                {
                    // Default to requiring the reference to NOT be static
                    bool isStatic = false;
                    if (!isConstructorCategory)
                    {
                        // If the parent expression is a TypeRef, then the reference must be static or const
                        TypeRef typeRef = (_parentExpression.SkipPrefixes() as TypeRef);
                        if (typeRef != null)
                        {
                            // If the parent expression is an Interface (an explicit interface implementation, or
                            // a bogus reference to a static member of Object, such as ReferenceEquals), then abort
                            // (don't check the static mode after all).
                            if (typeRef.IsInterface)
                                goto skip;
                            isStatic = true;
                        }
                    }

                    // Verify that the static modifier matches
                    candidate.IsStaticModeMatch = (IsStatic(obj) == isStatic);
                }
            skip:

                // Verify that the match is accessible - if it's not, we must treat it as an incomplete match so that
                // we'll continue looking for other possible matches that are complete and accessible.  For example,
                // multiple overloaded methods (with or without matching parameters) where a private one in a derived
                // class should be ignored in favor of others in base classes, or non-private events that are backed by
                // private delegate fields with the same name in the same class, or invalid code where two members are
                // otherwise duplicates but have different access rights.

                // We have to call the individual access right properties to determine access instead of looking at the
                // Modifiers bit flags, in order to handle default access rights and the actual access rights of property/
                // indexer/event accessors.  Use a special method for performance that gets the access rights that we need.
                bool isPrivate, isProtected, isInternal;
                GetAccessRights(obj, out isPrivate, out isProtected, out isInternal);
                if (isPrivate)
                {
                    // If the ref is inside the same TypeDecl as the definition OR a nested type, then it's accessible, otherwise it's not
                    if (obj is INamedCodeObject)
                    {
                        TypeDecl refDeclaringTypeDecl = _unresolvedRef.FindParent<TypeDecl>();
                        if (refDeclaringTypeDecl != null)
                        {
                            TypeDecl objDeclaringTypeDecl = ((INamedCodeObject)obj).FindParent<TypeDecl>() ?? obj as TypeDecl;
                            // Check if the TypeDecls are identical OR have the same name (could be different parts)
                            candidate.IsAccessible = refDeclaringTypeDecl.IsSameAs(objDeclaringTypeDecl);
                            while (!candidate.IsAccessible && refDeclaringTypeDecl.Parent is TypeDecl)
                            {
                                refDeclaringTypeDecl = (TypeDecl)refDeclaringTypeDecl.Parent;
                                if (refDeclaringTypeDecl.IsSameAs(objDeclaringTypeDecl))
                                    candidate.IsAccessible = true;
                            }
                        }
                    }
                    else
                        candidate.IsAccessible = false;
                }
                else
                {
                    bool isInternalAccessible = true, isProtectedAccessible = true;
                    if (isInternal)
                    {
                        // If the reference is in the same Project as the definition OR it's in another project or assembly that has
                        // specifically made its internals visible to the referring project, then it's accessible.
                        Project refDeclaringProject = _unresolvedRef.FindParent<Project>();
                        if (refDeclaringProject != null)
                        {
                            if (obj is INamedCodeObject)
                            {
                                Project objDeclaringProject = ((INamedCodeObject)obj).FindParent<Project>();
                                isInternalAccessible = (refDeclaringProject == objDeclaringProject || objDeclaringProject.AreInternalTypesVisibleTo(refDeclaringProject));
                            }
                            else
                            {
                                string assemblyName = null;
                                if (obj is MemberReference)
                                    assemblyName = ((MemberReference)obj).Module.Assembly.FullName;
                                else if (obj is MemberInfo)
                                    assemblyName = ((MemberInfo)obj).Module.Assembly.FullName;
                                if (assemblyName != null)
                                {
                                    assemblyName = AssemblyUtil.GetNormalizedDisplayName(assemblyName);
                                    LoadedAssembly objDeclaringAssembly = refDeclaringProject.FrameworkContext.ApplicationContext.FindLoadedAssembly(assemblyName);
                                    isInternalAccessible = (objDeclaringAssembly != null && objDeclaringAssembly.AreInternalTypesVisibleTo(refDeclaringProject));
                                }
                            }
                        }
                    }
                    if (isProtected)
                    {
                        // If the ref is inside the same type as the definition or any derived type OR nested type, then it's accessible
                        TypeDecl refDeclaringTypeDecl = _unresolvedRef.FindParent<TypeDecl>();
                        if (refDeclaringTypeDecl != null)
                        {
                            object objDeclaringType = (obj is INamedCodeObject ? (((INamedCodeObject)obj).FindParent<TypeDecl>() ?? obj as TypeDecl)
                                : (obj is MemberReference ? (object)((MemberReference)obj).DeclaringType : ((MemberInfo)obj).DeclaringType));
                            isProtectedAccessible = IsSameOrBaseTypeOf(objDeclaringType, refDeclaringTypeDecl);
                            while (!isProtectedAccessible && refDeclaringTypeDecl.Parent is TypeDecl)
                            {
                                refDeclaringTypeDecl = (TypeDecl)refDeclaringTypeDecl.Parent;
                                if (IsSameOrBaseTypeOf(objDeclaringType, refDeclaringTypeDecl))
                                    isProtectedAccessible = true;
                            }
                        }
                    }
                    if (isInternal)
                    {
                        if (isProtected)
                        {
                            if (!isInternalAccessible && !isProtectedAccessible)
                                candidate.IsAccessible = false;
                        }
                        else
                        {
                            if (!isInternalAccessible)
                                candidate.IsAccessible = false;
                        }
                    }
                    else if (isProtected && !isProtectedAccessible)
                        candidate.IsAccessible = false;
                }
            }

            // Conditionally add the candidate to the collection (depending upon what is already in it)
            _hasCompleteMatch = _unresolvedRef.AddMatch(candidate);
        }

        protected void AddMatchInternal(object obj, bool lookInAllParts)
        {
            AddMatchInternal(obj, lookInAllParts, null);
        }

        protected void AddMatchInternal(object obj)
        {
            AddMatchInternal(obj, false, null);
        }

        /// <summary>
        /// Check if the type arguments match, based upon the object type.
        /// </summary>
        protected bool DoTypeArgumentCountsMatch(object obj)
        {
            if (obj is CodeObject)
            {
                if (obj is ITypeParameters)
                    return ((ITypeParameters)obj).DoTypeArgumentCountsMatch(_unresolvedRef);
                return (_unresolvedRef.TypeArgumentCount == 0);
            }

            int typeArgumentCount = _unresolvedRef.TypeArgumentCount;
            if (obj is TypeDefinition)
                return (TypeDefinitionUtil.GetLocalGenericArgumentCount((TypeDefinition)obj) == typeArgumentCount);
            if (obj is MethodDefinition)
            {
                MethodDefinition methodDefinition = (MethodDefinition)obj;
                int typeParameterCount = methodDefinition.GenericParameters.Count;
                if (typeParameterCount == typeArgumentCount)
                    return true;
                if (typeArgumentCount == 0)
                {
                    // Type arguments to generic methods can be omitted and inferred from the parameter types, so if
                    // the actual count is 0, it's still considered a match *if* we have at least one method parameter.
                    if (methodDefinition.Parameters != null && methodDefinition.Parameters.Count > 0)
                        return true;

                    // If the UnresolvedRef is part of an explicit interface implementation of a GenericMethodDecl,
                    // then match the actual type argument count.
                    if (_unresolvedRef.IsExplicitInterfaceImplementation && _unresolvedRef.Parent.Parent is GenericMethodDecl)
                        return (typeParameterCount == ((GenericMethodDecl)_unresolvedRef.Parent.Parent).TypeParameterCount);
                }
                return false;
            }
            if (obj is Type)
                return (TypeUtil.GetLocalGenericArgumentCount((Type)obj) == typeArgumentCount);
            if (obj is MethodInfo)
            {
                MethodInfo methodInfo = (MethodInfo)obj;
                int typeParameterCount = methodInfo.GetGenericArguments().Length;
                if (typeParameterCount == typeArgumentCount)
                    return true;
                if (typeArgumentCount == 0)
                {
                    // Type arguments to generic methods can be omitted and inferred from the parameter types, so if
                    // the actual count is 0, it's still considered a match *if* we have at least one method parameter.
                    if (methodInfo.GetParameters().Length > 0)
                        return true;

                    // If the UnresolvedRef is part of an explicit interface implementation of a GenericMethodDecl,
                    // then match the actual type argument count.
                    if (_unresolvedRef.IsExplicitInterfaceImplementation && _unresolvedRef.Parent.Parent is GenericMethodDecl)
                        return (typeParameterCount == ((GenericMethodDecl)_unresolvedRef.Parent.Parent).TypeParameterCount);
                }
                return false;
            }

            // Other object types can't have type arguments, so the count must be 0
            return (typeArgumentCount == 0);
        }

        private bool IsSameOrBaseTypeOf(object typeReference, TypeDecl typeDecl)
        {
            if (typeReference is TypeDecl && ((TypeDecl)typeReference).IsSameAs(typeDecl))
                return true;
            TypeRef typeRef = (TypeRef)TypeRef.CreateTypeRef(typeReference, null, false);
            TypeRef baseTypeRef = typeDecl.GetBaseType();
            do
            {
                if (typeRef.IsSameGenericType(baseTypeRef))
                    return true;
                baseTypeRef = baseTypeRef.GetBaseType() as TypeRef;
            }
            while (baseTypeRef != null);
            return false;
        }
    }
}