// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Reflection;
using Mono.Cecil;

using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="EventDecl"/> or <see cref="EventDefinition"/>/<see cref="EventInfo"/>.
    /// </summary>
    public class EventRef : VariableRef
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="EventRef"/>.
        /// </summary>
        public EventRef(EventDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="EventRef"/>.
        /// </summary>
        public EventRef(EventDecl declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create an <see cref="EventRef"/>.
        /// </summary>
        public EventRef(EventDefinition eventDefinition, bool isFirstOnLine)
            : base(eventDefinition, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="EventRef"/>.
        /// </summary>
        public EventRef(EventDefinition eventDefinition)
            : base(eventDefinition, false)
        { }

        /// <summary>
        /// Create an <see cref="EventRef"/>.
        /// </summary>
        public EventRef(EventInfo eventInfo, bool isFirstOnLine)
            : base(eventInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="EventRef"/>.
        /// </summary>
        public EventRef(EventInfo eventInfo)
            : base(eventInfo, false)
        { }

        #endregion

        #region /* STATIC METHODS */

        /// <summary>
        /// Construct an <see cref="EventRef"/> from a <see cref="EventReference"/>.
        /// </summary>
        public static EventRef Create(EventReference eventReference, bool isFirstOnLine)
        {
            EventDefinition eventDefinition = eventReference.Resolve();
            return (eventDefinition != null ? new EventRef(eventDefinition, isFirstOnLine) : null);
        }

        /// <summary>
        /// Construct an <see cref="EventRef"/> from a <see cref="EventReference"/>.
        /// </summary>
        public static EventRef Create(EventReference eventReference)
        {
            return Create(eventReference, false);
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="EventInfo"/>.
        /// </summary>
        public static Modifiers GetEventModifiers(EventDefinition eventDefinition)
        {
            Modifiers modifiers = 0;
            // An event doesn't actually have modifiers - get them from the adder/remover methods
            MethodDefinition adder = eventDefinition.AddMethod;
            MethodDefinition remover = eventDefinition.RemoveMethod;
            if (adder != null)
            {
                modifiers = MethodRef.GetMethodModifiers(adder);
                if (remover != null)
                {
                    // Combine the two sets of modifiers, removing any extraneous access modifiers
                    modifiers |= MethodRef.GetMethodModifiers(remover);
                    if (modifiers.HasFlag(Modifiers.Public))
                        modifiers &= ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Private);
                    else if (modifiers.HasFlag(Modifiers.Protected) || modifiers.HasFlag(Modifiers.Internal))
                        modifiers &= ~Modifiers.Private;
                }
            }
            else if (remover != null)
                modifiers = MethodRef.GetMethodModifiers(remover);
            return modifiers;
        }

        /// <summary>
        /// Get any modifiers from the specified <see cref="EventInfo"/>.
        /// </summary>
        public static Modifiers GetEventModifiers(EventInfo eventInfo)
        {
            Modifiers modifiers = 0;
            // An event doesn't actually have modifiers - get them from the adder/remover methods
            MethodInfo adder = eventInfo.GetAddMethod(true);
            MethodInfo remover = eventInfo.GetRemoveMethod(true);
            if (adder != null)
            {
                modifiers = MethodRef.GetMethodModifiers(adder);
                if (remover != null)
                {
                    // Combine the two sets of modifiers, removing any extraneous access modifiers
                    modifiers |= MethodRef.GetMethodModifiers(remover);
                    if (modifiers.HasFlag(Modifiers.Public))
                        modifiers &= ~(Modifiers.Protected | Modifiers.Internal | Modifiers.Private);
                    else if (modifiers.HasFlag(Modifiers.Protected) || modifiers.HasFlag(Modifiers.Internal))
                        modifiers &= ~Modifiers.Private;
                }
            }
            else if (remover != null)
                modifiers = MethodRef.GetMethodModifiers(remover);
            return modifiers;
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Get the declaring type of the referenced event.
        /// </summary>
        public override TypeRefBase GetDeclaringType()
        {
            TypeRefBase declaringTypeRef = GetDeclaringType(_reference);

            // An event reference doesn't store any type arguments for a parent type instance, so any
            // type arguments in any generic declaring type or its parent types will always default to
            // the declared type arguments.  Convert them from OpenTypeParameterRefs to TypeParameterRefs
            // so that they don't show up as Red in the GUI.
            if (declaringTypeRef != null && declaringTypeRef.HasTypeArguments)
                declaringTypeRef.ConvertOpenTypeParameters();

            return declaringTypeRef;
        }

        /// <summary>
        /// Get the declaring type of the specified event object.
        /// </summary>
        /// <param name="eventObj">The event object (an <see cref="EventDecl"/> or <see cref="EventDefinition"/>/<see cref="EventInfo"/>).</param>
        /// <returns>The <see cref="TypeRef"/> of the declaring type, or null if it can't be determined.</returns>
        public static TypeRefBase GetDeclaringType(object eventObj)
        {
            TypeRefBase declaringTypeRef;
            if (eventObj is EventDecl)
            {
                TypeDecl declaringTypeDecl = ((EventDecl)eventObj).DeclaringType;
                declaringTypeRef = (declaringTypeDecl != null ? declaringTypeDecl.CreateRef() : null);
            }
            else if (eventObj is EventDefinition)
                declaringTypeRef = TypeRef.Create(((EventDefinition)eventObj).DeclaringType);
            else //if (eventObj is EventInfo)
                declaringTypeRef = TypeRef.Create(((EventInfo)eventObj).DeclaringType);
            return declaringTypeRef;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            TypeRefBase typeRefBase;
            if (_reference is EventDecl)
                typeRefBase = ((EventDecl)_reference).EvaluateType(withoutConstants);
            else if (_reference is EventDefinition)
                typeRefBase = TypeRef.Create(((EventDefinition)_reference).EventType);
            else //if (_reference is EventInfo)
                typeRefBase = TypeRef.Create(((EventInfo)_reference).EventHandlerType);

            // Evaluate any type arguments (this is necessary even for a EventInfo, because it's type might
            // be a generic type with a type argument that is specified in a base type list declaration).
            if (typeRefBase != null)
                typeRefBase = typeRefBase.EvaluateTypeArgumentTypes(_parent, this);

            return typeRefBase;
        }

        #endregion

        #region /* RENDERING */

        public static void AsTextEventDefinition(CodeWriter writer, EventDefinition eventDefinition, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;
            if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                Attribute.AsTextAttributes(writer, eventDefinition);
            writer.Write(ModifiersHelpers.AsString(GetEventModifiers(eventDefinition)));
            TypeRefBase.AsTextTypeReference(writer, eventDefinition.EventType, passFlags);
            writer.Write(" ");
            TypeRefBase.AsTextTypeReference(writer, eventDefinition.DeclaringType, passFlags);
            writer.Write(Dot.ParseToken + eventDefinition.Name);
        }

        public static void AsTextEventInfo(CodeWriter writer, EventInfo eventInfo, RenderFlags flags)
        {
            RenderFlags passFlags = flags & ~RenderFlags.Description;
            if (!flags.HasFlag(RenderFlags.NoPreAnnotations))
                Attribute.AsTextAttributes(writer, eventInfo);
            writer.Write(ModifiersHelpers.AsString(GetEventModifiers(eventInfo)));
            TypeRefBase.AsTextType(writer, eventInfo.EventHandlerType, passFlags);
            writer.Write(" ");
            TypeRefBase.AsTextType(writer, eventInfo.DeclaringType, passFlags);
            writer.Write(Dot.ParseToken + eventInfo.Name);
        }

        #endregion
    }
}
