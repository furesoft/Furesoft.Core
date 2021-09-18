// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Used to manage checksum information for files.
    /// </summary>
    public class PragmaChecksumDirective : PragmaDirective
    {
        #region /* FIELDS */

        protected string _fileName;
        protected string _guid;
        protected string _crc;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="PragmaChecksumDirective"/>.
        /// </summary>
        public PragmaChecksumDirective(string fileName, string guid, string crc)
        {
            FileName = fileName;
            GUID = guid;
            CRC = crc;
        }

        #endregion

        #region /* PROPERTIES */

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

        /// <summary>
        /// The associated CRC value.
        /// </summary>
        public string CRC
        {
            get { return _crc; }
            set { _crc = value; }
        }

        public override string PragmaType { get { return ParseToken; } }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "checksum";

        internal static new void AddParsePoints()
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

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextArgument(writer, flags);
            writer.Write(" " + _fileName + " " + _guid + " " + _crc);
        }

        #endregion
    }
}
