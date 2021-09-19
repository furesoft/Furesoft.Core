using System;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments
{
    /// <summary>
    /// Represents plain-text user documentation of code.
    /// </summary>
    /// <remarks>
    /// This class is used to mix portions of plain-text with other embedded documentation comment objects, such as <see cref="DocSee"/> or <see cref="DocCode"/>.
    /// If a documentation comment object contains only plain-text, it will store it as a simple string instead of using this object.
    /// </remarks>
    public class DocText : DocComment
    {
        /// <summary>
        /// Create a <see cref="DocText"/>.
        /// </summary>
        public DocText(string text)
            : base(text)
        { }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        /// <summary>
        /// The text content of the object.
        /// </summary>
        public override string Text
        {
            get { return (string)_content; }
        }

        public static void AsTextText(CodeWriter writer, string text, RenderFlags flags)
        {
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; ++i)
            {
                if (i > 0)
                    AsTextDocNewLines(writer, 1);
                if (lines[i].Length > 0)
                {
                    // Turn on translation of '<', '&', and '>' for content
                    if (!flags.HasFlag(RenderFlags.NoTranslations))
                        writer.InDocCommentContent = true;
                    writer.Write(lines[i]);
                    writer.InDocCommentContent = false;
                }
            }
        }

        /// <summary>
        /// Add the specified text to the documentation comment.
        /// </summary>
        public override void Add(string text)
        {
            _content += text;
        }

        /// <summary>
        /// Throws an exception if called.
        /// </summary>
        public override void Add(DocComment docComment)
        {
            throw new Exception("Can't add DocComment objects to DocText objects (add them both to a parent DocComment instead).");
        }

        protected override void AsTextContent(CodeWriter writer, RenderFlags flags)
        {
            writer.EscapeUnicode = false;
            AsTextText(writer, (string)_content, flags);
            writer.EscapeUnicode = true;
        }
    }
}
