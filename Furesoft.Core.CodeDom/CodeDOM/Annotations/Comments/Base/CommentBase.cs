using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base
{
    /// <summary>
    /// The common base class of all comment classes (<see cref="Comment"/> and <see cref="DocComment"/>).
    /// </summary>
    public abstract class CommentBase : Annotation
    {
        #region /* FIELDS */

        protected byte _prefixSpaceCount;

        #endregion

        #region /* CONSTRUCTORS */

        protected CommentBase()
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The count of prefix spaces (if any) before the comment delimiter.
        /// </summary>
        public int PrefixSpaceCount
        {
            get { return _prefixSpaceCount; }
        }

        #endregion

        #region /* PARSING */

        protected CommentBase(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Always default to a blank line before a stand-alone comment
            return 2;
        }

        #endregion

        #region /* RENDERING */

        #endregion
    }
}
