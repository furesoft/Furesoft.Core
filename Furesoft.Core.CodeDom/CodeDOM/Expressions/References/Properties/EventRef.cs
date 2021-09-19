using System.Reflection;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a reference to an <see cref="EventDecl"/> or <see cref="EventInfo"/>.
    /// </summary>
    public class EventRef : VariableRef
    {
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
        public EventRef(EventInfo eventInfo, bool isFirstOnLine)
            : base(eventInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="EventRef"/>.
        /// </summary>
        public EventRef(EventInfo eventInfo)
            : base(eventInfo, false)
        { }

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

        /// <summary>
        /// Get the declaring type of the specified event object.
        /// </summary>
        /// <param name="eventObj">The event object (an <see cref="EventDecl"/> or <see cref="EventInfo"/>).</param>
        /// <returns>The <see cref="TypeRef"/> of the declaring type, or null if it can't be determined.</returns>
        public static TypeRefBase GetDeclaringType(object eventObj)
        {
            TypeRefBase declaringTypeRef;
            if (eventObj is EventDecl)
            {
                TypeDecl declaringTypeDecl = ((EventDecl)eventObj).DeclaringType;
                declaringTypeRef = (declaringTypeDecl != null ? declaringTypeDecl.CreateRef() : null);
            }
            else //if (eventObj is EventInfo)
                declaringTypeRef = TypeRef.Create(((EventInfo)eventObj).DeclaringType);
            return declaringTypeRef;
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
    }
}