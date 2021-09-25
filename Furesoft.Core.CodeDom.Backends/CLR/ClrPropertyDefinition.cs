using Flame;
using Flame.Collections;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Furesoft.Core.CodeDom.Backends.CLR
{
    /// <summary>
    /// A Flame property that wraps an IL property definition.
    /// </summary>
    public sealed class ClrPropertyDefinition : IProperty
    {
        private Lazy<IReadOnlyList<ClrAccessorDefinition>> accessorDefs;

        private AttributeMap attributeMap;

        private DeferredInitializer contentsInitializer;

        private IReadOnlyList<Parameter> indexerParams;

        private IType propertyTypeValue;

        /// <summary>
        /// Creates a wrapper around an IL property definition.
        /// </summary>
        /// <param name="definition">
        /// The definition to wrap in a Flame property.
        /// </param>
        /// <param name="parentType">
        /// The definition's declaring type.
        /// </param>
        public ClrPropertyDefinition(
            PropertyDefinition definition,
            ClrTypeDefinition parentType)
        {
            this.Definition = definition;
            this.ParentType = parentType;
            this.FullName = new SimpleName(definition.Name)
                .Qualify(parentType.FullName);
            this.contentsInitializer = parentType.Assembly
                .CreateSynchronizedInitializer(AnalyzeContents);
            this.accessorDefs = parentType.Assembly
                .CreateSynchronizedLazy(AnalyzeAccessors);
        }

        /// <summary>
        /// Gets a list of all accessors defined by this
        /// property.
        /// </summary>
        /// <returns>All accessors defined by this property.</returns>
        public IReadOnlyList<ClrAccessorDefinition> Accessors
        {
            get
            {
                return accessorDefs.Value;
            }
        }

        /// <inheritdoc/>
        IReadOnlyList<IAccessor> IProperty.Accessors => Accessors;

        /// <inheritdoc/>
        public AttributeMap Attributes
        {
            get
            {
                contentsInitializer.Initialize();
                return attributeMap;
            }
        }

        /// <summary>
        /// Gets the IL property definition wrapped by this Flame property.
        /// </summary>
        /// <returns>An IL property definition.</returns>
        public PropertyDefinition Definition { get; private set; }

        /// <inheritdoc/>
        public QualifiedName FullName { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<Parameter> IndexerParameters
        {
            get
            {
                contentsInitializer.Initialize();
                return indexerParams;
            }
        }

        /// <inheritdoc/>
        public UnqualifiedName Name => FullName.FullyUnqualifiedName;

        /// <summary>
        /// Gets the type that defines this property.
        /// </summary>
        /// <returns>The type that defines this property.</returns>
        public ClrTypeDefinition ParentType { get; private set; }

        /// <inheritdoc/>
        IType ITypeMember.ParentType => ParentType;

        /// <inheritdoc/>
        public IType PropertyType
        {
            get
            {
                contentsInitializer.Initialize();
                return propertyTypeValue;
            }
        }

        private IReadOnlyList<ClrAccessorDefinition> AnalyzeAccessors()
        {
            var results = new List<ClrAccessorDefinition>();
            if (Definition.GetMethod != null)
            {
                results.Add(
                    new ClrAccessorDefinition(
                        Definition.GetMethod,
                        AccessorKind.Get,
                        this));
            }
            if (Definition.SetMethod != null)
            {
                results.Add(
                    new ClrAccessorDefinition(
                        Definition.SetMethod,
                        AccessorKind.Set,
                        this));
            }
            foreach (var accessor in Definition.OtherMethods)
            {
                results.Add(
                    new ClrAccessorDefinition(
                        accessor,
                        AccessorKind.Other,
                        this));
            }
            return results;
        }

        private void AnalyzeContents()
        {
            var assembly = ParentType.Assembly;

            propertyTypeValue = TypeHelpers.BoxIfReferenceType(
                assembly.Resolve(Definition.PropertyType, ParentType));

            // Analyze the parameter list.
            indexerParams = Definition.Parameters
                .Select(param =>
                    ClrMethodDefinition.WrapParameter(
                        param,
                        assembly,
                        ParentType))
                .ToArray();

            var attrBuilder = new AttributeMapBuilder();
            // TODO: analyze attributes.
            attributeMap = new AttributeMap(attrBuilder);
        }
    }
}