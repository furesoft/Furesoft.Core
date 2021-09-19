using Furesoft.Core.CodeDom.Utilities;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef.Base;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.CodeRef.Base
{
    /// <summary>
    /// The common base class of all documentation comment tags that have a 'cref' attribute (<see cref="DocSee"/>,
    /// <see cref="DocSeeAlso"/>, <see cref="DocException"/>, <see cref="DocPermission"/>).
    /// </summary>
    public abstract class DocCodeRefBase : DocComment
    {
        /// <summary>
        /// The name of the code-ref attribute.
        /// </summary>
        public const string AttributeName = "cref";

        /// <summary>
        /// Determines if code references are parsed as code or plain text.
        /// </summary>
        public static bool ParseRefsAsCode = true;

        // Should evaluate to a code object reference (Expression - SymbolicRef, Dot, Call), but can also be a string.
        protected object _codeRef;

        protected DocCodeRefBase(Expression codeRef, string text)
            : base(text)
        {
            CodeRef = codeRef;
        }

        protected DocCodeRefBase(Expression codeRef, params DocComment[] docComments)
            : base(docComments)
        {
            CodeRef = codeRef;
        }

        protected DocCodeRefBase(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);
        }

        /// <summary>
        /// The associated <see cref="CodeObject"/> or string.
        /// </summary>
        public object CodeRef
        {
            get { return _codeRef; }
            set { SetField(ref _codeRef, value, true); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_codeRef == null || (_codeRef is string && ((string)_codeRef).IndexOf('\n') < 0)
                    || (_codeRef is CodeObject && !((CodeObject)_codeRef).IsFirstOnLine && ((CodeObject)_codeRef).IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_codeRef is CodeObject)
                {
                    if (value)
                        ((CodeObject)_codeRef).IsFirstOnLine = false;
                    ((CodeObject)_codeRef).IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            DocCodeRefBase clone = (DocCodeRefBase)base.Clone();
            clone.CloneField(ref clone._codeRef, _codeRef);
            return clone;
        }

        protected override void AsTextStart(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.Description))
                writer.Write("<" + TagName + " " + AttributeName + "=\"");
            if (_codeRef != null)
            {
                // Turn on translation of '<', '&', and '>' for content
                writer.InDocCommentContent = true;
                if (_codeRef is Expression)
                    ((Expression)_codeRef).AsText(writer, flags);
                else if (_codeRef is string)
                    DocText.AsTextText(writer, (string)_codeRef, flags);
                writer.InDocCommentContent = false;
            }
            if (!flags.HasFlag(RenderFlags.Description))
                writer.Write("\"" + (_content == null && !MissingEndTag ? "/>" : ">"));
        }

        protected override object ParseAttributeValue(Parser parser, string name)
        {
            // Parse the value as code unless disabled or the value is empty
            if (StringUtil.NNEqualsIgnoreCase(name, AttributeName))
            {
                // Override parsing of the 'cref' attribute to parse as an expression
                if (parser.TokenText == ParseTokenValueQuote1 || parser.TokenText == ParseTokenValueQuote2)
                {
                    string quote = parser.TokenText;
                    if (ParseRefsAsCode && parser.Char != quote[0])
                    {
                        // Some people seem to misunderstand and put type prefixes on the value - these should
                        // only exist in the generated XML file, not the tag values, so we'll just skip over
                        // any such prefix here.
                        if (parser.PeekChar == ':' && "NTFPME".IndexOf(parser.Char) >= 0)
                        {
                            parser.NextChar();
                            parser.NextChar();
                        }

                        // Parse the code expression
                        SetField(ref _codeRef, parser.ParseCodeExpressionUntil(quote, this), false);
                    }
                    else
                        _codeRef = parser.GetToDelimiter(parser.TokenText[0]);

                    // Move past the ending quote (if one was found)
                    if (parser.TokenText == quote)
                        parser.NextToken(true);
                }
                else
                {
                    // Parse a single token, and move past it
                    _codeRef = parser.TokenText;
                    parser.NextToken(true);
                }

                if (_codeRef == null)
                    _codeRef = "";
                return _codeRef;
            }
            return base.ParseAttributeValue(parser, name);
        }
    }
}
