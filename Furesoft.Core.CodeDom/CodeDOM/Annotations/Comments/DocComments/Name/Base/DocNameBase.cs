// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using Furesoft.Core.CodeDom.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all documentation comment tags that have a 'name' attribute (<see cref="DocParam"/>,
    /// <see cref="DocParamRef"/>, <see cref="DocTypeParam"/>, <see cref="DocTypeParamRef"/>).
    /// </summary>
    public abstract class DocNameBase : DocComment
    {
        /// <summary>
        /// The name of the name attribute.
        /// </summary>
        public const string AttributeName = "name";

        // Should evaluate to a ParameterRef (if DocParam/DocParamRef)
        // or TypeParameterRef (if DocTypeParam/DocTypeParamRef) or an UnresolvedRef.
        protected SymbolicRef _nameRef;

        protected DocNameBase(SymbolicRef nameRef, string text)
            : base(text)
        {
            NameRef = nameRef;
        }

        protected DocNameBase(SymbolicRef nameRef, params DocComment[] docComments)
            : base(docComments)
        {
            NameRef = nameRef;
        }

        protected DocNameBase(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_nameRef == null || (!_nameRef.IsFirstOnLine && _nameRef.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_nameRef != null)
                {
                    if (value)
                        _nameRef.IsFirstOnLine = false;
                    _nameRef.IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// The <see cref="SymbolicRef"/> of the associated code object.
        /// </summary>
        public SymbolicRef NameRef
        {
            get { return _nameRef; }
            set { SetField(ref _nameRef, value, true); }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            DocNameBase clone = (DocNameBase)base.Clone();
            clone.CloneField(ref clone._nameRef, _nameRef);
            return clone;
        }

        protected override void AsTextStart(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.Description))
                writer.Write("<" + TagName + " " + AttributeName + "=\"");
            if (_nameRef != null)
            {
                // Turn on translation of '<', '&', and '>' for content
                writer.InDocCommentContent = true;
                _nameRef.AsText(writer, flags);
                writer.InDocCommentContent = false;
            }
            if (!flags.HasFlag(RenderFlags.Description))
                writer.Write("\"" + (_content == null && !MissingEndTag ? "/>" : ">"));
        }

        protected override object ParseAttributeValue(Parser parser, string name)
        {
            // Override parsing of the 'name' attribute to parse as a reference
            if (StringUtil.NNEqualsIgnoreCase(name, AttributeName))
            {
                // By default, parse a string value (including any whitespace) delimited by single or double quotes.
                // If there's no delimiter, just use the text of the token (perhaps a single word).
                string value;
                int lineNumber = parser.Token.LineNumber;
                ushort columnNumber = parser.Token.ColumnNumber;
                if (parser.TokenText == ParseTokenValueQuote1 || parser.TokenText == ParseTokenValueQuote2)
                {
                    ++columnNumber;  // Start just past the quote
                    value = parser.GetToDelimiter(parser.TokenText[0]);
                }
                else
                    value = parser.TokenText;
                parser.NextToken(true);  // Move past delimiter (or token)

                NameRef = new UnresolvedRef(value.Trim(), lineNumber, columnNumber);
                return _nameRef;
            }
            return base.ParseAttributeValue(parser, name);
        }
    }
}