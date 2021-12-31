using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Pragmas.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Pragmas
{
    /// <summary>
    /// Used to manage checksum information for files.
    /// </summary>
    public class PragmaChecksumDirective : PragmaDirective
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "checksum";

        protected string _crc;
        protected string _fileName;
        protected string _guid;

        /// <summary>
        /// Create a <see cref="PragmaChecksumDirective"/>.
        /// </summary>
        public PragmaChecksumDirective(string fileName, string guid, string crc)
        {
            FileName = fileName;
            GUID = guid;
            CRC = crc;
        }

        /// <summary>
        /// Parse a <see cref="PragmaChecksumDirective"/>.
        /// </summary>
        public PragmaChecksumDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'pragma'
            Token token = parser.NextTokenSameLine(false);  // Move past 'checksum'
            if (token != null)
            {
                // Get the filename
                _fileName = token.Text;
                token = parser.NextTokenSameLine(false);  // Move past the filename
                if (token != null)
                {
                    // Get the GUID
                    _guid = token.Text;
                    token = parser.NextTokenSameLine(false);  // Move past the GUID
                    if (token != null)
                    {
                        // Get the CRC
                        _crc = token.Text;
                        parser.NextToken();  // Move past the CRC
                    }
                }
            }
            MoveEOLComment(parser.LastToken);
        }

        /// <summary>
        /// The associated CRC value.
        /// </summary>
        public string CRC
        {
            get { return _crc; }
            set { _crc = value; }
        }

        /// <summary>
        /// The associated file name.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// The associated GUID.
        /// </summary>
        public string GUID
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public override string PragmaType { get { return ParseToken; } }

        public static new void AddParsePoints()
        {
            AddPragmaParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="PragmaChecksumDirective"/>.
        /// </summary>
        public static new PragmaChecksumDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new PragmaChecksumDirective(parser, parent);
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextArgument(writer, flags);
            writer.Write(" " + _fileName + " " + _guid + " " + _crc);
        }
    }
}