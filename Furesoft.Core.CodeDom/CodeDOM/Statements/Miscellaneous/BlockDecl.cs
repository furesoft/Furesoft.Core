using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a stand-alone block of code restricted to a local scope (surrounded by braces).
    /// </summary>
    public class BlockDecl : BlockStatement
    {
        /// <summary>
        /// Create a <see cref="BlockDecl"/>.
        /// </summary>
        public BlockDecl(CodeObject body)
            : base(body, false)
        { }

        /// <summary>
        /// Create a <see cref="BlockDecl"/>.
        /// </summary>
        public BlockDecl()
            : base(null, false)
        { }

        /// <summary>
        /// Create a <see cref="BlockDecl"/> with the specified <see cref="CodeObject"/>s in the body.
        /// </summary>
        public BlockDecl(params CodeObject[] codeObjects)
            : base(codeObjects)
        { }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public override bool HasHeader
        {
            get { return false; }
        }

        /// <summary>
        /// Attach an <see cref="Annotation"/> (<see cref="Comment"/>, <see cref="DocComment"/>, <see cref="Attribute"/>, <see cref="CompilerDirective"/>, or <see cref="Message"/>) to the <see cref="CodeObject"/>.
        /// </summary>
        /// <param name="annotation">The <see cref="Annotation"/>.</param>
        /// <param name="atFront">Inserts at the front if true, otherwise adds at the end.</param>
        public override void AttachAnnotation(Annotation annotation, bool atFront)
        {
            // Don't allow EOL comments on a BlockDecl since it has nothing to display them on - move
            // them to the child Block so that they can be displayed.
            if (annotation.IsEOL)
                Body.AttachAnnotation(annotation, atFront);
            else
                base.AttachAnnotation(annotation, atFront);
        }

        /// <summary>
        /// Parse a <see cref="BlockDecl"/>.
        /// </summary>
        public BlockDecl(Parser parser, CodeObject parent, params string[] terminators)
            : base(parser, parent)
        {
            new Block(out _body, parser, this, true, terminators);  // Parse the body
        }

        public static void AddParsePoints()
        {
            // Use a parse-priority of 300 (GenericMethodDecl uses 0, UnresolvedRef uses 100, PropertyDeclBase uses 200, Initializer uses 400)
            Parser.AddParsePoint(Block.ParseTokenStart, 300, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="BlockDecl"/>.
        /// </summary>
        public static BlockDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new BlockDecl(parser, parent);
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// Reformat the <see cref="Block"/> body.
        /// </summary>
        public override void ReformatBlock()
        {
            // BlockDecls must always have braces and start on a new line, and default to the ending
            // brace being on a new line without any preceeding blank lines.
            _body.HasBraces = true;
            _body.IsFirstOnLine = true;
            _body.SetNewLines(1);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            if (flags.HasFlag(RenderFlags.Description))
                TypeRefBase.AsTextType(writer, GetType(), RenderFlags.None);
            else
                base.AsText(writer, flags);
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextAfter(writer, flags | RenderFlags.SuppressNewLine);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
        }
    }
}