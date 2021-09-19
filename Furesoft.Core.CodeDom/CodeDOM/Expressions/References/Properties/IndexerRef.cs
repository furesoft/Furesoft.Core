using System.Reflection;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties
{
    /// <summary>
    /// Represents a reference to an <see cref="IndexerDecl"/> or a <see cref="PropertyInfo"/> for an indexer.
    /// </summary>
    public class IndexerRef : PropertyRef
    {
        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(IndexerDecl declaration, bool isFirstOnLine)
            : base(declaration, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(IndexerDecl declaration)
            : base(declaration, false)
        { }

        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(PropertyInfo propertyInfo, bool isFirstOnLine)
            : base(propertyInfo, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(PropertyInfo propertyInfo)
            : base(propertyInfo, false)
        { }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(IndexerDecl.ParseToken);
        }
    }
}
