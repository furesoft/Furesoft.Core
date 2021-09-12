// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Base
{
    /// <summary>
    /// These modifiers are usable on various code objects to specify access and special behaviors.
    /// </summary>
    /// <remarks>
    /// The order of appearance in this enum determines the display order of the modifiers.
    /// </remarks>
    [Flags]
    public enum Modifiers
    {
        //                          * = only if nested                                 Event,
        None      = 0x00000000,  // Class  Struct  Interface  Delegate  Enum  Method  Property  Indexer  Field  Constant  Ctor  Operator  Accessor  Destructor
        Public    = 0x00000001,  //   Y       Y        Y         Y       Y       Y        Y        Y       Y       Y       Y       Y
        Protected = 0x00000002,  //   *       *        *         *       *       Y        Y        Y       Y       Y       Y                 Y
        Internal  = 0x00000004,  //   Y       Y        Y         Y       Y       Y        Y        Y       Y       Y       Y                 Y
        // Protected + Internal  //   *       *        *         *       *       Y        Y        Y       Y       Y       Y                 Y
        Private   = 0x00000008,  //   *       *        *         *       *       Y        Y        Y       Y       Y       Y                 Y
        Static    = 0x00000010,  //   Y                                          Y        Y                Y               Y       Y
        New       = 0x00000020,  //   *                          *       *       Y        Y        Y       Y       Y
        Abstract  = 0x00000040,  //   Y                                          Y        Y        Y
        Sealed    = 0x00000080,  //   Y                                          Y        Y        Y
        Virtual   = 0x00000100,  //                                              Y        Y        Y
        Override  = 0x00000200,  //                                              Y        Y        Y
        Extern    = 0x00000400,  //                                              Y        Y        Y                       Y       Y                    Y
        Unsafe    = 0x00000800,  //   Y       Y        Y         Y               Y        Y        Y       Y       Y       Y       Y         Y
        // NOTE: Partial must appear as the last modifier for both types and methods.
        Partial   = 0x00001000,  //   Y       Y        Y                         Y
        Implicit  = 0x00004000,  //                                                                                                Y (conv ops only)
        Explicit  = 0x00008000,  //                                                                                                Y (conv ops only)
        Const     = 0x00010000,  //                                                                                Y (also local vars)
        ReadOnly  = 0x00020000,  //                                                                        Y
        Volatile  = 0x00040000,  //                                                                        Y
        Event     = 0x00100000   //                                                                        Y

        // RULES:
        // - Namespaces have no access modifiers (they are implicitly public).
        // - Top-level type declarations can be only 'internal' or 'public', and default to 'internal'.
        // - Nested type declarations in a class or struct default to 'private'.
        // - Interface and enum members are implicitly 'public' (no access modifiers are allowed).
        // - Class can't be both 'abstract' and 'sealed'.
        // - If any method of a class is 'abstract', the class must be 'abstract'.
        // - A non-abstract Class with an 'abstract' base must implement all 'abstract' members.
        // - An 'abstract' class must implement all interface members, although it may map them onto 'abstract' methods.
        // - Structs are implicitly 'sealed', and can't have default constructors or a destructor.
        // - Struct members can't have 'protected' or 'protected internal' access.
        // - An 'abstract' method can't be 'static', 'virtual' (it's implicitly virtual), or 'extern'.
        // - An 'abstract' or 'extern' method has no body.
        // - An 'abstract' property behaves like an 'abstract' method.
        // - An 'abstract' property can't be 'static'.
        // - Nested types can access 'private' members of their parent.
        // - A 'readonly' field may only be assigned in the declaration or in a constructor of the parent class.
        // - 'const' or type declaration members of a class are implicitly 'static'.
        // - 'const' reference types can only be null or a string.
        // - 'virtual' can't be used with 'static', 'abstract', or 'override'.
        // - 'volatile' fields must be a reference type, or an integral type (excluding long/ulong), or a float.
    }

    #region /* STATIC HELPER CLASS */

    /// <summary>
    /// Static helper methods for Modifiers.
    /// </summary>
    public static class ModifiersHelpers
    {
        #region /* STATIC FIELDS */

        private static readonly string[] _names;
        private static readonly Array _values;
        private static readonly Dictionary<string, Modifiers> _nameToModifierMap = new Dictionary<string, Modifiers>();

        #endregion

        #region /* STATIC CONSTRUCTOR */

        // Setup arrays of names, values, and a map of names to values.
        static ModifiersHelpers()
        {
            _names = Enum.GetNames(typeof(Modifiers));
            _values = Enum.GetValues(typeof(Modifiers));
            for (int i = 0; i < _values.Length; ++i)
            {
                _names[i] = _names[i].ToLower();
                _nameToModifierMap.Add(_names[i], (Modifiers)_values.GetValue(i));
            }
        }

        #endregion

        #region /* STATIC HELPER METHODS */

        /// <summary>
        /// Format Modifiers as a string.
        /// </summary>
        public static string AsString(Modifiers modifiers)
        {
            if (modifiers == Modifiers.None)
                return "";
            using (CodeWriter writer = new CodeWriter())
            {
                AsText(modifiers, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Convert Modifiers to text.
        /// </summary>
        public static void AsText(Modifiers modifiers, CodeWriter writer)
        {
            if (modifiers != Modifiers.None)
            {
                for (int i = 1; i < _values.Length; ++i)
                {
                    if (modifiers.HasFlag((Modifiers)_values.GetValue(i)))
                        writer.Write(_names[i] + ' ');
                }
            }
        }

        /// <summary>
        /// Returns true if the specified text is a valid modifier name.
        /// </summary>
        public static bool IsModifier(string modifier)
        {
            return (modifier != null && _nameToModifierMap.ContainsKey(modifier));
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse tokens into Modifiers bit flags.
        /// </summary>
        public static Modifiers Parse(Parser parser, CodeObject parent)
        {
            // Search the parser's unused list for valid modifier tokens
            Modifiers modifiers = 0;
            bool hasSandwichedModifiers = false;
            bool hasConditionalModifiers = false;
            bool needsElseCondition = true;
            bool elseIsNotActive = false;
            bool foundEndIf = false;
            List<ParsedObject> unusedList = parser.Unused;

            // First, pre-scan the unused list to check for the special case of conditional directives
            // being used on the modifiers.  Set a flag if we detect this for later processing below.
            for (int i = unusedList.Count - 1; i >= 0; --i)
            {
                ParsedObject parsedObject = unusedList[i];
                if (parsedObject is Token)
                {
                    // Abort if there's a blank line, or if we get a token that isn't a modifier
                    Token token = (Token)parsedObject;
                    if (token.NewLines > 1 || !IsModifier(token.Text))
                        break;
                    if (foundEndIf)
                        hasSandwichedModifiers = true;
                }
                else
                {
                    // Abort if we get anything other than the expected #if/#else/#endif chain, or if
                    // we have any comments before we hit the #endif (going backwards).
                    CodeObject codeObject = ((UnusedCodeObject)parsedObject).CodeObject;
                    if (codeObject is EndIfDirective)
                    {
                        if (foundEndIf || parsedObject.HasTrailingComments)
                            break;
                        foundEndIf = true;
                    }
                    else if (codeObject is ConditionalDirective)
                    {
                        if (!foundEndIf)
                            break;
                        if (codeObject is ElseDirective)
                            needsElseCondition = false;
                        else
                        {
                            if (((ConditionalDirective)codeObject).IsActive)
                                elseIsNotActive = true;
                            if (codeObject is IfDirective)
                            {
                                if (hasSandwichedModifiers)
                                    hasConditionalModifiers = true;
                                break;
                            }
                        }
                    }
                    else
                        break;
                }
            }

            // Now, reset and do the real parse scan
            bool postDirective = true;
            string declarationText = null;
            string declarationModifiersText = null;
            Token lastUnrecognizedToken = null;
            List<ConditionalDirective> inactiveConditions = new List<ConditionalDirective>();
            for (int i = unusedList.Count - 1; i >= 0; --i)
            {
                ParsedObject parsedObject = unusedList[i];
                if (parsedObject is Token)
                {
                    // Check for modifier tokens
                    Token token = (Token)unusedList[i];
                    Modifiers value;
                    if (_nameToModifierMap.TryGetValue(token.Text, out value))
                    {
                        modifiers |= value;
                        parent.MoveFormatting(token);
                        parent.MoveAllComments(token, true);
                        unusedList.RemoveAt(i);

                        // If we've passed the conditionals and have prefixed modifiers, add them
                        // to the skipped text of all of the inactive conditions.
                        if (!hasConditionalModifiers && inactiveConditions.Count > 0)
                        {
                            foreach (ConditionalDirective conditionalDirective in inactiveConditions)
                                conditionalDirective.SkippedText = AsString(value) + conditionalDirective.SkippedText;
                        }
                    }
                    else
                    {
                        // If the token isn't recognized, continue processing in case there are other unused tokens
                        // that we do recognize as modifiers (there might be new modifiers added in later C# versions).
                        // Also, keep track of the last unrecognized token so we can transfer any newlines to it.
                        lastUnrecognizedToken = (Token)parsedObject;
                    }
                }
                else if (hasConditionalModifiers)
                {
                    // Check for compiler directives - specifically, we handle an #endif immediately
                    // preceeding the parent declaration, allowing for a chain of conditional directives
                    // used to change the modifiers on the declaration at compile-time.  We have already
                    // verified that we have a valid sequence (above), so we process conditional directives
                    // backwards here until we get to the starting #if.
                    UnusedCodeObject unused = (UnusedCodeObject)parsedObject;
                    if (unused.CodeObject is CompilerDirective)
                    {
                        // Store any compiler directives as pre or post annotations on the parent
                        CompilerDirective compilerDirective = (CompilerDirective)unused.CodeObject;
                        if (compilerDirective is EndIfDirective)
                        {
                            declarationText = parent.AsString();
                            declarationModifiersText = AsString(modifiers);
                        }
                        else if (compilerDirective is ConditionalDirective)
                        {
                            ConditionalDirective conditionalDirective = (ConditionalDirective)compilerDirective;

                            // Create an #else if none existed
                            if (needsElseCondition)
                            {
                                ElseDirective elseDirective = new ElseDirective();
                                if (elseIsNotActive)
                                {
                                    elseDirective.SkippedText = declarationText;
                                    inactiveConditions.Add(elseDirective);
                                }
                                else
                                    postDirective = false;
                                parent.AttachAnnotation(elseDirective, elseIsNotActive ? AnnotationFlags.IsPostfix : AnnotationFlags.None, true);
                                needsElseCondition = false;
                            }

                            // When we find the active condition, switch from post to pre
                            if (conditionalDirective.IsActive)
                                postDirective = false;
                            else
                            {
                                // Update any skipped conditions to reflect the entire declaration
                                Modifiers orderedModifiers = Parse(conditionalDirective.SkippedText + " " + declarationModifiersText);
                                conditionalDirective.SkippedText = AsString(orderedModifiers) + declarationText;
                                inactiveConditions.Add(conditionalDirective);
                            }

                            // Stop processing conditionals when we get to the #if
                            if (compilerDirective is IfDirective)
                                hasConditionalModifiers = false;
                        }
                        else
                            break;  // For now, stop if we find any other directives

                        parent.AttachAnnotation(compilerDirective, postDirective ? AnnotationFlags.IsPostfix : AnnotationFlags.None, true);
                        parent.MoveComments(unused.LastToken, true);
                        unusedList.RemoveAt(i);
                    }
                }
                else
                    break;
            }

            // If we had any unrecognized tokens, they'll be emitted before the parent object, so if the last one
            // doesn't have any newlines, we need to give it the parent's newlines.
            if (lastUnrecognizedToken != null)
            {
                if (lastUnrecognizedToken.NewLines == 0)
                    lastUnrecognizedToken.NewLines = (ushort)parent.NewLines;
                parent.NewLines = 0;
            }

            return modifiers;
        }

        /// <summary>
        /// Parse a string of space-delimited modifiers.
        /// </summary>
        public static Modifiers Parse(string input)
        {
            Modifiers modifiers = 0;
            foreach (string modifier in input.Split(' '))
            {
                Modifiers value;
                if (_nameToModifierMap.TryGetValue(modifier, out value))
                    modifiers |= value;
            }
            return modifiers;
        }

        #endregion
    }

    #endregion
}
