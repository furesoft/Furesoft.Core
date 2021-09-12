// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Reflection;
using Mono.Cecil;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties
{
    /// <summary>
    /// Represents a reference to an <see cref="IndexerDecl"/> or a <see cref="PropertyInfo"/> for an indexer.
    /// </summary>
    public class IndexerRef : PropertyRef
    {
        #region /* CONSTRUCTORS */

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
        public IndexerRef(PropertyDefinition propertyDefinition, bool isFirstOnLine)
            : base(propertyDefinition, isFirstOnLine)
        { }

        /// <summary>
        /// Create an <see cref="IndexerRef"/>.
        /// </summary>
        public IndexerRef(PropertyDefinition propertyDefinition)
            : base(propertyDefinition, false)
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

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            if (_reference is PropertyDeclBase)
            {
                TypeRefBase typeRefBase = null;
                PropertyDeclBase indexerDecl = (PropertyDeclBase)_reference;
                Expression type = indexerDecl.Type;
                if (type != null)
                {
                    typeRefBase = type.EvaluateType(withoutConstants);

                    // Do NOT evaluate TypeParameterRefs here, because it will cause infinite recursion when
                    // calling parent.FindTypeArgument(), which will go back to our parent, Index.EvaluateType().
                    // We don't need to do this, because Index.EvaluateType() will handle it.
                    //typeRefBase = typeRefBase.EvaluateTypeArgumentTypes(Parent);
                }
                return typeRefBase;
            }
            if (_reference is PropertyDefinition)
            {
                PropertyDefinition propertyDefinition = (PropertyDefinition)_reference;
                TypeReference indexerTypeReference = propertyDefinition.PropertyType;
                return TypeRef.Create(indexerTypeReference);
            }
            PropertyInfo propertyInfo = (PropertyInfo)_reference;
            Type indexerType = propertyInfo.PropertyType;
            return TypeRef.Create(indexerType);
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(IndexerDecl.ParseToken);
        }

        #endregion
    }
}
