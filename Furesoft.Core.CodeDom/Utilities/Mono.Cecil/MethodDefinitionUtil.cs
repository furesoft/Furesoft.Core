// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Linq;

using Mono.Cecil;
using Mono.Collections.Generic;

namespace Nova.Utilities
{
    /// <summary>
    /// Static helper methods for <see cref="MethodDefinition"/>.
    /// </summary>
    public static class MethodDefinitionUtil
    {
        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Check if the method is an override.
        /// </summary>
        public static bool IsOverride(MethodDefinition methodDefinition)
        {
            // If it's virtual and IsReuseSlot is true, then it's probably 'override', but we also have to rule
            // out 'abstract', AND unfortunately we have to actually look for a base method to be 100% sure (in
            // some cases, such as external managed code in other languages, there isn't any - so it's really
            // just 'virtual' and NOT 'override').
            return (methodDefinition.IsVirtual && methodDefinition.IsReuseSlot && !methodDefinition.IsAbstract && FindBaseMethod(methodDefinition) != null);
        }

        /// <summary>
        /// Get the parameter with the specified name.
        /// </summary>
        public static ParameterDefinition GetParameter(MethodDefinition methodDefinition, string name)
        {
            foreach (ParameterDefinition parameterInfo in methodDefinition.Parameters)
            {
                if (parameterInfo.Name == name)
                    return parameterInfo;
            }
            return null;
        }

        /// <summary>
        /// Find the type argument for the specified type parameter.
        /// </summary>
        public static GenericParameter FindTypeArgument(MethodDefinition methodDefinition, GenericParameter typeParameter)
        {
            if (methodDefinition.HasGenericParameters)
            {
                foreach (GenericParameter genericParameter in methodDefinition.GenericParameters)
                {
                    if (genericParameter == typeParameter)
                        return genericParameter;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the index of the specified type parameter.
        /// </summary>
        public static int FindTypeParameterIndex(MethodDefinition methodDefinition, GenericParameter typeParameter)
        {
            if (methodDefinition.HasGenericParameters)
            {
                foreach (GenericParameter genericParameter in methodDefinition.GenericParameters)
                {
                    if (genericParameter == typeParameter)
                        return genericParameter.Position;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the type parameter at the specified index.
        /// </summary>
        public static GenericParameter GetTypeParameter(MethodDefinition methodDefinition, int index)
        {
            if (methodDefinition.HasGenericParameters)
            {
                Collection<GenericParameter> typeParameters = methodDefinition.GenericParameters;
                if (typeParameters != null)
                {
                    if (index >= 0 && index < typeParameters.Count)
                        return typeParameters[index];
                }
            }
            return null;
        }

        /// <summary>
        /// Find the base virtual method for this method if it's an override.
        /// </summary>
        public static MethodDefinition FindBaseMethod(MethodDefinition methodDefinition)
        {
            if (methodDefinition.IsVirtual)  // Can't distinguish 'override', but 'virtual' should be true
            {
                TypeDefinition declaringType = methodDefinition.DeclaringType;
                TypeReference baseType = declaringType.BaseType;
                TypeReference[] parameterTypes = Enumerable.ToArray(Enumerable.Select<ParameterDefinition, TypeReference>(methodDefinition.Parameters,
                    delegate(ParameterDefinition parameterDefinition) { return parameterDefinition.ParameterType; }));
                while (baseType != null)
                {
                    TypeDefinition baseTypeDefinition = null;
                    try { baseTypeDefinition = baseType.Resolve(); }
                    catch { } 
                    if (baseTypeDefinition == null)
                        break;
                    MethodDefinition baseMethodDefinition = TypeDefinitionUtil.GetMethod(baseTypeDefinition, methodDefinition.Name, parameterTypes);
                    if (baseMethodDefinition != null)
                        return baseMethodDefinition;
                    baseType = baseTypeDefinition.BaseType;
                }
            }
            return null;
        }

        #endregion
    }
}
