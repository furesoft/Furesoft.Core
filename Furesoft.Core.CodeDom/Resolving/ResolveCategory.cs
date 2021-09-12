
namespace Furesoft.Core.CodeDom.Resolving
{
    /// <summary>
    /// The resolve category is used to restrict <see cref="UnresolvedRef"/>s to a particular sub-category when resolving them.
    /// </summary>
    public enum ResolveCategory
    {
        Unspecified,
        Type,                // type, type parameter, type alias
        Namespace,           // namespace
        NamespaceOrType,     // namespace or parent type (prefixed with a '.' before a Type, Interface, Constructor, or Attribute)
        Interface,           // interface
        Method,              // method, local variable or field of delegate type, event
        Constructor,         // constructor
        Attribute,           // attribute
        OperatorOverload,    // overloaded operator
        Property,            // property
        Indexer,             // indexer
        Event,               // event
        TypeParameter,       // type parameter
        LocalTypeParameter,  // local type parameter (looks at current method/class only - used by constraints and doc comments)
        Parameter,           // parameter (looks at current method only - used by doc comments)
        GotoTarget,          // label, switch item (case, default)
        NamespaceAlias,      // namespace alias
        RootNamespace,       // root-level namespace
        Expression,          // literal constant, local constant, local variable, parameter, field, field constant, enum value, property
        CodeObject           // used only as a placeholder while resolving a tree of code objects
    }

    #region /* STATIC HELPER CLASS */

    /// <summary>
    /// Static helper methods for ResolveCategory.
    /// </summary>
    public static class ResolveCategoryHelpers
    {
        #region /* STATIC FIELDS */

        /// <summary>
        /// Determines if a ResolveCategory is a method category (has parameters).
        /// </summary>
        public static bool[] IsMethod =
            {
                false,  // Unspecified,
                false,  // Type,               // type, type parameter, type alias
                false,  // Namespace,          // namespace
                false,  // NamespaceOrType,    // namespace or parent type (prefixed with a '.' before a Type, Interface, Constructor, or Attribute)
                false,  // Interface,          // interface
                true,   // Method,             // method, local variable or field of delegate type, event
                true,   // Constructor,        // constructor
                true,   // Attribute,          // attribute
                false,  // OperatorOverload,   // overloaded operator
                false,  // Property,           // property
                true,   // Indexer,            // indexer
                false,  // Event,              // event
                false,  // TypeParameter,      // type parameter
                false,  // LocalTypeParameter, // local type parameter (looks at current method/class only - used by constraints and doc comments)
                false,  // Parameter,          // parameter (looks at current method only - used by doc comments)
                false,  // GotoTarget,         // label, switch item (case, default)
                false,  // ExternAlias,        // external namespace alias
                false,  // RootNamespace,      // root-level namespace
                false,  // Expression,         // literal constant, local constant, local variable, parameter, field, field constant, enum value, property
                false   // CodeObject          // used only as a placeholder while resolving a tree of code objects
            };

        /// <summary>
        /// Determines if a ResolveCategory is a constructor category.
        /// </summary>
        public static bool[] IsConstructor =
            {
                false,  // Unspecified,
                false,  // Type,               // type, type parameter, type alias
                false,  // Namespace,          // namespace
                false,  // NamespaceOrType,    // namespace or parent type (prefixed with a '.' before a Type, Interface, Constructor, or Attribute)
                false,  // Interface,          // interface
                false,  // Method,             // method, local variable or field of delegate type, event
                true,   // Constructor,        // constructor
                true,   // Attribute,          // attribute
                false,  // OperatorOverload,   // overloaded operator
                false,  // Property,           // property
                false,  // Indexer,            // indexer
                false,  // Event,              // event
                false,  // TypeParameter,      // type parameter
                false,  // LocalTypeParameter, // local type parameter (looks at current method/class only - used by constraints and doc comments)
                false,  // Parameter,          // parameter (looks at current method only - used by doc comments)
                false,  // GotoTarget,         // label, switch item (case, default)
                false,  // ExternAlias,        // external namespace alias
                false,  // RootNamespace,      // root-level namespace
                false,  // Expression,         // literal constant, local constant, local variable, parameter, field, field constant, enum value, property
                false   // CodeObject          // used only as a placeholder while resolving a tree of code objects
            };

        /// <summary>
        /// Determines if a ResolveCategory is a type or constructor category.
        /// </summary>
        public static bool[] IsTypeOrConstructor =
            {
                false,  // Unspecified,
                true,   // Type,               // type, type parameter, type alias
                false,  // Namespace,          // namespace
                false,  // NamespaceOrType,    // namespace or parent type (prefixed with a '.' before a Type, Interface, Constructor, or Attribute)
                false,  // Interface,          // interface
                false,  // Method,             // method, local variable or field of delegate type, event
                true,   // Constructor,        // constructor
                true,   // Attribute,          // attribute
                false,  // OperatorOverload,   // overloaded operator
                false,  // Property,           // property
                false,  // Indexer,            // indexer
                false,  // Event,              // event
                false,  // TypeParameter,      // type parameter
                false,  // LocalTypeParameter, // local type parameter (looks at current method/class only - used by constraints and doc comments)
                false,  // Parameter,          // parameter (looks at current method only - used by doc comments)
                false,  // GotoTarget,         // label, switch item (case, default)
                false,  // ExternAlias,        // external namespace alias
                false,  // RootNamespace,      // root-level namespace
                false,  // Expression,         // literal constant, local constant, local variable, parameter, field, field constant, enum value, property
                false   // CodeObject          // used only as a placeholder while resolving a tree of code objects
            };

        /// <summary>
        /// Determines if a ResolveCategory is a method or property category.
        /// </summary>
        public static bool[] IsPropertyOrMethod =
            {
                false,  // Unspecified,
                false,  // Type,               // type, type parameter, type alias
                false,  // Namespace,          // namespace
                false,  // NamespaceOrType,    // namespace or parent type (prefixed with a '.' before a Type, Interface, Constructor, or Attribute)
                false,  // Interface,          // interface
                true,   // Method,             // method, local variable or field of delegate type, event
                true,   // Constructor,        // constructor
                true,   // Attribute,          // attribute
                false,  // OperatorOverload,   // overloaded operator
                true,   // Property,           // property
                true,   // Indexer,            // indexer
                true,   // Event,              // event
                false,  // TypeParameter,      // type parameter
                false,  // LocalTypeParameter, // local type parameter (looks at current method/class only - used by constraints and doc comments)
                false,  // Parameter,          // parameter (looks at current method only - used by doc comments)
                false,  // GotoTarget,         // label, switch item (case, default)
                false,  // ExternAlias,        // external namespace alias
                false,  // RootNamespace,      // root-level namespace
                true,   // Expression,         // literal constant, local constant, local variable, parameter, field, field constant, enum value, property
                false   // CodeObject          // used only as a placeholder while resolving a tree of code objects
            };

        /// <summary>
        /// Determines if a ResolveCategory is a type-level category.
        /// </summary>
        /// <remarks>
        /// It's legal for the Expression category to include Type references, such as for "Type.StaticMember" (or enum members),
        /// so it's not included here - only categories that are members of types are included.  We can omit LocalTypeParameter,
        /// because searches will stop at the first TypeDecl regardless.
        /// </remarks>
        public static bool[] IsTypeLevel =
            {
                false,  // Unspecified,
                false,  // Type,               // type, type parameter, type alias
                false,  // Namespace,          // namespace
                false,  // NamespaceOrType,    // namespace or parent type (prefixed with a '.' before a Type, Interface, Constructor, or Attribute)
                false,  // Interface,          // interface
                true,   // Method,             // method, local variable or field of delegate type, event
                false,  // Constructor,        // constructor
                false,  // Attribute,          // attribute
                false,  // OperatorOverload,   // overloaded operator
                true,   // Property,           // property
                true,   // Indexer,            // indexer
                true,   // Event,              // event
                true,   // TypeParameter,      // type parameter
                false,  // LocalTypeParameter, // local type parameter (looks at current method/class only - used by constraints and doc comments)
                true,   // Parameter,          // parameter (looks at current method only - used by doc comments)
                true,   // GotoTarget,         // label, switch item (case, default)
                false,  // ExternAlias,        // external namespace alias
                false,  // RootNamespace,      // root-level namespace
                false,  // Expression,         // literal constant, local constant, local variable, parameter, field, field constant, enum value, property
                false   // CodeObject          // used only as a placeholder while resolving a tree of code objects
            };

        /// <summary>
        /// Determines if a ResolveCategory is a method-level category.
        /// </summary>
        public static bool[] IsMethodLevel =
            {
                false,  // Unspecified,
                false,  // Type,               // type, type parameter, type alias
                false,  // Namespace,          // namespace
                false,  // NamespaceOrType,    // namespace or parent type (prefixed with a '.' before a Type, Interface, Constructor, or Attribute)
                false,  // Interface,          // interface
                false,  // Method,             // method, local variable or field of delegate type, event
                false,  // Constructor,        // constructor
                false,  // Attribute,          // attribute
                false,  // OperatorOverload,   // overloaded operator
                false,  // Property,           // property
                false,  // Indexer,            // indexer
                false,  // Event,              // event
                false,  // TypeParameter,      // type parameter
                false,  // LocalTypeParameter, // local type parameter (looks at current method/class only - used by constraints and doc comments)
                true,   // Parameter,          // parameter (looks at current method only - used by doc comments)
                true,   // GotoTarget,         // label, switch item (case, default)
                false,  // ExternAlias,        // external namespace alias
                false,  // RootNamespace,      // root-level namespace
                false,  // Expression,         // literal constant, local constant, local variable, parameter, field, field constant, enum value, property
                false   // CodeObject          // used only as a placeholder while resolving a tree of code objects
            };

        #endregion

        #region /* STATIC HELPER METHODS */

        #endregion
    }

    #endregion
}
