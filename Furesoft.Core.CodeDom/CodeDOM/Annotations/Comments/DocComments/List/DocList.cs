// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a list in a documentation comment.
    /// </summary>
    public class DocList : DocComment
    {
        #region /* FIELDS */

        /// <summary>
        /// The type of the <see cref="DocList"/> (should be 'bullet', 'number', or 'table').
        /// </summary>
        protected string _type;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocList"/> with the specified type.
        /// </summary>
        public DocList(string type, params DocComment[] docComments)
        {
            _type = type;

            foreach (DocComment docComment in docComments)
            {
                // Default-format entries
                Add("\n    ");
                Add(docComment);
            }

            // Default end tag to first-on-line
            Add("\n");
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The type of the <see cref="DocList"/> (should be 'bullet', 'number', or 'table').
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "list";

        /// <summary>
        /// The name of the list 'type' attribute.
        /// </summary>
        public const string AttributeName = "type";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocList"/>.
        /// </summary>
        public static new DocList Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocList(parser, parent);
        }

        protected DocList(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);
        }

        protected override object ParseAttributeValue(Parser parser, string name)
        {
            object value = base.ParseAttributeValue(parser, name);
            if (StringUtil.NNEqualsIgnoreCase(name, AttributeName))
                _type = value.ToString().Trim();
            return value;
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextStart(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.Description))
                writer.Write("<" + TagName + " " + AttributeName + "=\"" + _type + "\"" + (_content == null && !MissingEndTag ? "/>" : ">"));
        }

        #endregion
    }
}
