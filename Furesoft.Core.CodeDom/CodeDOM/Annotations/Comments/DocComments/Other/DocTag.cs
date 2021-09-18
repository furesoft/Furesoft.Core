// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an unrecognized or orphaned (missing start or end) tag in a documentation comment.
    /// </summary>
    /// <remarks>
    /// A <see cref="DocTag"/> can also have one or more attributes.  Attributes are ignored on other documentation objects
    /// except for those which are expected, such as for classes derived from DocNameBase or DocCrefBase.
    /// </remarks>
    public class DocTag : DocComment
    {
        #region /* FIELDS */

        protected string _tag;
        protected Dictionary<string, object> _attributes;

        #endregion

        #region /* CONSTRUCTORS */

        // This class should only be instantiated during the parsing process.

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return _tag; }
        }

        /// <summary>
        /// A dictionary of child attributes.
        /// </summary>
        public Dictionary<string, object> Attributes
        {
            get { return _attributes; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create the dictionary of attributes, or return the existing one.
        /// </summary>
        public Dictionary<string, object> CreateAttributes()
        {
            if (_attributes == null)
                _attributes = new Dictionary<string, object>();
            return _attributes;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            DocTag clone = (DocTag)base.Clone();
            if (_attributes != null && _attributes.Count > 0)
                clone._attributes = new Dictionary<string, object>(_attributes);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse a <see cref="DocTag"/>.
        /// </summary>
        public DocTag(string tag, Parser parser, CodeObject parent)
        {
            _tag = tag;
            _attributes = ParseTag(parser, parent);
        }

        /// <summary>
        /// Parse an unexpected end tag.
        /// </summary>
        public DocTag(Token tagToken, int newLines, Parser parser, CodeObject parent)
        {
            Parent = parent;
            NewLines = newLines;
            SetLineCol(tagToken);
            _tag = tagToken.Text;
            _annotationFlags |= AnnotationFlags.NoStartTag;
            parser.AttachMessage(this, "End tag '</" + _tag + ">' without matching start tag!", tagToken);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextStart(CodeWriter writer, RenderFlags flags)
        {
            if (!MissingStartTag)
            {
                writer.Write("<" + TagName);
                if (_attributes != null && _attributes.Count > 0)
                {
                    foreach (KeyValuePair<string, object> pair in _attributes)
                    {
                        writer.Write(" " + pair.Key + "=\"");
                        if (pair.Value is string)
                            writer.Write((string)pair.Value);
                        else
                            ((CodeObject)pair.Value).AsText(writer, flags);
                        writer.Write("\"");
                    }
                }
                writer.Write(_content == null && !MissingEndTag ? "/>" : ">");
            }
        }

        protected override void AsTextEnd(CodeWriter writer, RenderFlags flags)
        {
            if (!MissingEndTag && (_content != null || MissingStartTag))
            {
                string tagName = TagName;
                if (tagName != null)
                    writer.Write("</" + tagName + ">");
            }
        }

        protected override void AsTextContent(CodeWriter writer, RenderFlags flags)
        {
            // Don't trim newlines for unrecognized tags, since the tags are always rendered
            base.AsTextContent(writer, flags & ~RenderFlags.NoTagNewLines);
        }

        #endregion
    }
}
