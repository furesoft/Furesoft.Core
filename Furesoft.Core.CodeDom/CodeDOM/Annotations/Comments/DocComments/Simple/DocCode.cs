using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Simple
{
    /// <summary>
    /// Represents a block of code in a documentation comment.
    /// </summary>
    public class DocCode : DocComment, IBlock
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "code";

        /// <summary>
        /// Determines if <see cref="DocCode"/> content is parsed as code or plain text.
        /// </summary>
        public static bool ParseContentAsCode = true;

        static DocCode()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            ForceReference();
        }

        /// <summary>
        /// Create a <see cref="DocCode"/>.
        /// </summary>
        public DocCode(CodeObject content)
            : base(content)
        { }

        /// <summary>
        /// Parse a <see cref="DocCode"/>.
        /// </summary>
        public DocCode(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// The <see cref="Block"/> body.
        /// </summary>
        public Block Body
        {
            get { return _content as Block; }
            set
            {
                _content = value;
                if (_content != null)
                {
                    ((Block)_content).Parent = this;
                    ReformatBlock();
                }
            }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool HasHeader
        {
            get { return false; }
        }

        /// <summary>
        /// Always <c>true</c>.
        /// </summary>
        public bool IsTopLevel
        {
            get { return true; }
        }

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocCode"/>.
        /// </summary>
        public static new DocComment Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            DocCode docCode = new DocCode(parser, parent);
            if (AutomaticCodeCleanup && docCode.Content is Expression)
                return new DocC((Expression)docCode.Content);
            return docCode;
        }

        /// <summary>
        /// Add a <see cref="CodeObject"/> to the embedded code <see cref="Block"/>.
        /// </summary>
        public virtual void Add(CodeObject obj)
        {
            CreateBody().Add(obj);
        }

        /// <summary>
        /// Add multiple <see cref="CodeObject"/>s to the embedded code <see cref="Block"/>.
        /// </summary>
        public void Add(params CodeObject[] objects)
        {
            CreateBody();
            foreach (CodeObject obj in objects)
                Body.Add(obj);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            DocCode clone = (DocCode)base.Clone();
            clone.CloneField(ref clone._content, _content);
            return clone;
        }

        /// <summary>
        /// Create a body if one doesn't exist yet.
        /// </summary>
        public Block CreateBody()
        {
            if (!(_content is Block))
                Body = new Block();
            return Body;
        }

        /// <summary>
        /// Insert a <see cref="CodeObject"/> at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="obj">The object to be inserted.</param>
        public void Insert(int index, CodeObject obj)
        {
            CreateBody().Insert(index, obj);
        }

        /// <summary>
        /// Reformat the <see cref="Block"/> body.
        /// </summary>
        public void ReformatBlock()
        { }

        /// <summary>
        /// Remove the specified <see cref="CodeObject"/> from the emedded code <see cref="Block"/>.
        /// </summary>
        public void Remove(CodeObject obj)
        {
            if (_content is Block)
                Body.Remove(obj);
        }

        /// <summary>
        /// Remove all objects from the embedded code <see cref="Block"/>.
        /// </summary>
        public void RemoveAll()
        {
            if (_content is Block)
                Body.RemoveAll();
        }

        protected void AfterTextNewLine(CodeWriter writer)
        {
            // Render the '///' prefix at the starting indent level, followed by any additional indentation
            int previousIndentPosition = writer.IndentOffset;
            int startingIndentPosition = writer.GetIndentOffset(this);
            writer.IndentOffset = startingIndentPosition - 4;  // Adjust for '/// ' prefix
            writer.Write(DocComment.ParseToken + " " + new string(' ', previousIndentPosition - startingIndentPosition));
            writer.IndentOffset = previousIndentPosition;
        }

        protected override void AsTextContent(CodeWriter writer, RenderFlags flags)
        {
            if (_content is CodeObject)
            {
                // Adjust the starting indentation for the code to allow for the '/// ' prefix
                writer.BeginOutdentOnNewLine(this, writer.IndentOffset + 4);
                writer.AfterNewLine += AfterTextNewLine;
                base.AsTextContent(writer, flags);
                if (_content is Block || _content is BlockStatement)
                    writer.WriteLine();
                writer.AfterNewLine -= AfterTextNewLine;
                writer.EndIndentation(this);
            }
            else
                base.AsTextContent(writer, flags);
        }

        /// <summary>
        /// Default format the specified child field code object.
        /// </summary>
        protected override void DefaultFormatField(CodeObject field)
        {
            // Just default format the field by default - don't remove any newlines
            field.DefaultFormat();
        }

        protected override bool ParseContent(Parser parser)
        {
            // Parse the content as code unless disabled or the content is empty
            if (ParseContentAsCode && parser.Char != ParseTokenTagOpen[0])
            {
                // Override parsing of the content to parse as a code block until we hit "</code>"
                SetField(ref _content, parser.ParseCodeBlockUntil(ParseTokenTagOpen + ParseTokenEndTag + ParseToken + ParseTokenTagClose, this), false);

                // If the content is null, code parsing was aborted - fall through and parse as non-code instead
                if (_content != null)
                {
                    // Convert a single-line block without braces to the single statement
                    if (_content is Block)
                    {
                        Block block = (Block)_content;
                        if (block.Count == 1 && !block.HasBraces)
                            _content = block[0];
                    }

                    // Look for expected end tag
                    return (parser.InDocComment && ParseEndTag(parser));
                }
            }
            return base.ParseContent(parser);
        }
    }
}
