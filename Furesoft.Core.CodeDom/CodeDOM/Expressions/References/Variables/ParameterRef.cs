// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Furesoft.Core.CodeDom.Utilities.Reflection;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to a <see cref="ParameterDecl"/> or a <see cref="ParameterInfo"/>.
    /// Similar to a <see cref="LocalRef"/>, but represents a parameter passed to the current method.
    /// </summary>
    /// <remarks>
    /// Although references to <see cref="ParameterInfo"/>s aren't common, they might occur in some special circumstances.
    /// </remarks>
    public class ParameterRef : VariableRef
    {
        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterDecl parameterDecl, bool isFirstOnLine)
            : base(parameterDecl, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterDecl parameterDecl)
            : base(parameterDecl, false)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterInfo parameterInfo, bool isFirstOnLine)
            : base(parameterInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create a <see cref="ParameterRef"/>.
        /// </summary>
        public ParameterRef(ParameterInfo parameterInfo)
            : base(parameterInfo, false)
        { }

        /// <summary>
        /// True if the referenced parameter is an 'out' parameter.
        /// </summary>
        public bool IsOut
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).IsOut;
                return ParameterInfoUtil.IsOut((ParameterInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced parameter is a 'params' parameter.
        /// </summary>
        public bool IsParams
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).IsParams;
                return ParameterInfoUtil.IsParams((ParameterInfo)_reference);
            }
        }

        /// <summary>
        /// True if the referenced parameter is a 'ref' parameter.
        /// </summary>
        public bool IsRef
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).IsRef;
                return ParameterInfoUtil.IsRef((ParameterInfo)_reference);
            }
        }

        /// <summary>
        /// The name of the <see cref="SymbolicRef"/>.
        /// </summary>
        public override string Name
        {
            get
            {
                if (_reference is ParameterDecl)
                    return ((ParameterDecl)_reference).Name;
                return ((ParameterInfo)_reference).Name;
            }
        }

        public static void AsTextParameterInfo(CodeWriter writer, ParameterInfo parameterInfo, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;

            Attribute.AsTextAttributes(writer, parameterInfo);

            ParameterModifier modifier = GetParameterModifier(parameterInfo);
            if (modifier != ParameterModifier.None)
                writer.Write(ParameterDecl.ParameterModifierToString(modifier) + " ");

            Type parameterType = parameterInfo.ParameterType;
            if (parameterType.IsByRef)
            {
                // Dereference (remove the trailing '&') if it's a reference type
                parameterType = parameterType.GetElementType();
            }
            TypeRefBase.AsTextType(writer, parameterType, passFlags);
            writer.Write(" " + parameterInfo.Name);
        }

        /// <summary>
        /// Find the parameter on the specified <see cref="MethodDeclBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodDeclBase methodDeclBase, string name, bool isFirstOnLine)
        {
            if (methodDeclBase != null)
            {
                ParameterRef parameterRef = methodDeclBase.GetParameter(name);
                if (parameterRef != null)
                {
                    parameterRef.IsFirstOnLine = isFirstOnLine;
                    return parameterRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find the parameter on the specified <see cref="MethodDeclBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodDeclBase methodDeclBase, string name)
        {
            return Find(methodDeclBase, name, false);
        }

        /// <summary>
        /// Find the parameter of the specified <see cref="MethodInfo"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodInfo methodInfo, string name, bool isFirstOnLine)
        {
            if (methodInfo != null)
            {
                ParameterInfo parameterInfo = MethodInfoUtil.GetParameter(methodInfo, name);
                if (parameterInfo != null)
                    return new ParameterRef(parameterInfo, isFirstOnLine);
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find the parameter of the specified <see cref="MethodInfo"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(MethodInfo methodInfo, string name)
        {
            return Find(methodInfo, name, false);
        }

        /// <summary>
        /// Find the parameter on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name, bool isFirstOnLine)
        {
            if (typeRefBase is MethodRef)
            {
                ParameterRef parameterRef = ((MethodRef)typeRefBase).GetParameter(name);
                if (parameterRef != null)
                {
                    parameterRef.IsFirstOnLine = isFirstOnLine;
                    return parameterRef;
                }
            }
            return new UnresolvedRef(name, isFirstOnLine);
        }

        /// <summary>
        /// Find the parameter on the specified <see cref="TypeRefBase"/> with the specified name.
        /// </summary>
        /// <returns>A <see cref="ParameterRef"/> to the parameter, or an <see cref="UnresolvedRef"/> if no match was found.</returns>
        public static SymbolicRef Find(TypeRefBase typeRefBase, string name)
        {
            return Find(typeRefBase, name, false);
        }

        /// <summary>
        /// Get the <see cref="ParameterModifier"/> for the specified <see cref="ParameterInfo"/>.
        /// </summary>
        public static ParameterModifier GetParameterModifier(ParameterInfo parameterInfo)
        {
            ParameterModifier modifier = ParameterModifier.None;
            if (ParameterInfoUtil.IsParams(parameterInfo))
                modifier = ParameterModifier.Params;
            else if (ParameterInfoUtil.IsRef(parameterInfo))
                modifier = ParameterModifier.Ref;
            else if (ParameterInfoUtil.IsOut(parameterInfo))
                modifier = ParameterModifier.Out;
            return modifier;
        }

        /// <summary>
        /// Get the type of the parameter in the collection with the specified index, using the specified parent expression to evaluate any type argument types.
        /// </summary>
        public static TypeRefBase GetParameterType(ICollection parameters, int index, Expression parentExpression)
        {
            TypeRefBase parameterTypeRef;
            if (parameters is List<ParameterDecl>)
                parameterTypeRef = ((List<ParameterDecl>)parameters)[index].Type.SkipPrefixes() as TypeRefBase;
            else //if (parameters is ParameterInfo[])
                parameterTypeRef = TypeRef.Create(((ParameterInfo[])parameters)[index].ParameterType);
            return parameterTypeRef;
        }

        /// <summary>
        /// Determine if the parameter in the collection with the specified index is a 'params' parameter.
        /// </summary>
        public static bool ParameterIsParams(ICollection parameters, int index)
        {
            bool isParams;
            if (parameters is List<ParameterDecl>)
                isParams = (((List<ParameterDecl>)parameters)[index].IsParams);
            else //if (parameters is ParameterInfo[])
                isParams = ParameterInfoUtil.IsParams(((ParameterInfo[])parameters)[index]);
            return isParams;
        }
    }
}