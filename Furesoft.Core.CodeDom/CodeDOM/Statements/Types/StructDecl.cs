using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Declares a struct, includes a name plus a body, along with various optional modifiers.
    /// Unlike classes, structs are value types.
    /// </summary>
    /// <remarks>
    /// Non-nested structs can be only public or internal, and default to internal.
    /// Nested structs can be any of the 5 access types, and default to private.
    /// Other valid modifiers include: new, partial
    /// Members of a struct default to private, and can't be either of the 2 protected options.
    /// Allowed members are: same as classes, except NO default constructor and NO destructor.
    /// Initializing instance fields isn't allowed.
    /// Structs can be created without using new, in which case the object can't be used until
    /// all fields are initialized.
    /// Inheritance isn't allowed, but implementing interfaces is OK.
    /// The optional base list can contain one or more interfaces, but NOT any structs.
    /// Structs are not permitted to declare a parameterless constructor - instead, the compiler implicitly
    /// generates one that sets all value-type fields to their default value, and all references to null.
    /// </remarks>
    public class StructDecl : BaseListTypeDecl
    {
        /// <summary>
        /// Create a <see cref="StructDecl"/> with the specified name.
        /// </summary>
        public StructDecl(string name, Modifiers modifiers)
            : base(name, modifiers)
        { }

        /// <summary>
        /// Create a <see cref="StructDecl"/> with the specified name.
        /// </summary>
        public StructDecl(string name)
            : base(name, Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="StructDecl"/> with the specified name, modifiers, and type parameters.
        /// </summary>
        public StructDecl(string name, Modifiers modifiers, params TypeParameter[] typeParameters)
            : base(name, modifiers, typeParameters)
        { }

        /// <summary>
        /// Create a <see cref="StructDecl"/> with the specified name, modifiers, and base types.
        /// </summary>
        public StructDecl(string name, Modifiers modifiers, params Expression[] baseTypes)
            : base(name, modifiers, baseTypes)
        { }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsStruct
        {
            get { return true; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public override bool IsValueType
        {
            get { return true; }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Get the base type.
        /// </summary>
        public override TypeRef GetBaseType()
        {
            return TypeRef.ValueTypeRef;
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "struct";

        protected StructDecl(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            MoveComments(parser.LastToken);        // Get any comments before 'struct'
            parser.NextToken();                    // Move past 'struct'
            ParseNameTypeParameters(parser);       // Parse the name and any optional type parameters
            ParseModifiersAndAnnotations(parser);  // Parse any attributes and/or modifiers

            // Move any trailing compiler directives to the Infix1 position (assume we have a base-type list)
            MoveAnnotations(AnnotationFlags.IsPostfix, AnnotationFlags.IsInfix1);

            ParseBaseTypeList(parser);       // Parse the optional base-type list
            ParseConstraintClauses(parser);  // Parse any constraint clauses

            // Move any trailing post annotations on the last base type to the first constraint (if any)
            AdjustBaseTypePostComments();

            // If we don't have a base-type list, move any trailing compiler directives to the Postfix position
            if (_baseTypes == null || _baseTypes.Count == 0)
                MoveAnnotations(AnnotationFlags.IsInfix1, AnnotationFlags.IsPostfix);

            new Block(out _body, parser, this, true);  // Parse the body

            // Eat any trailing terminator (they are allowed but not required on non-delegate type declarations)
            if (parser.TokenText == ParseTokenTerminator)
                parser.NextToken();
        }

        public static void AddParsePoints()
        {
            // Structs are only valid with a Namespace or TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="StructDecl"/>.
        /// </summary>
        public static StructDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new StructDecl(parser, parent);
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a struct declaration
            return 2;
        }
    }
}