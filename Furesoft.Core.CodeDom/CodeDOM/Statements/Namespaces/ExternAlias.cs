using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces
{
    /// <summary>
    /// Used together with compiler command-line options to create additional root-level namespaces.
    ///  </summary>
    /// <remarks>
    /// Allows multiple assemblies with the same namespaces (such as different versions of an assembly)
    /// to be used simultaneously.
    /// </remarks>
    public class ExternAlias : Statement, INamedCodeObject
    {
        /// <summary>
        /// The constant name of the global extern alias.
        /// </summary>
        public const string GlobalName = "global";

        /// <summary>
        /// The referenced root-level namespace.
        /// </summary>
        protected SymbolicRef _rootNamespaceRef;

        /// <summary>
        /// Create an <see cref="ExternAlias"/> with the specified <see cref="RootNamespace"/>.
        /// </summary>
        public ExternAlias(RootNamespace rootNamespace)
        {
            RootNamespaceRef = rootNamespace.CreateRef();
        }

        /// <summary>
        /// Create an <see cref="ExternAlias"/> with the specified namespace reference.
        /// </summary>
        public ExternAlias(SymbolicRef rootNamespaceRef)
        {
            RootNamespaceRef = rootNamespaceRef;
        }

        /// <summary>
        /// Create an <see cref="ExternAlias"/> with the specified name.
        /// </summary>
        public ExternAlias(string name)
        {
            RootNamespaceRef = new UnresolvedRef(name);
        }

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public string Category
        {
            get { return "extern alias"; }
        }

        /// <summary>
        /// True if the extern alias is 'global'.
        /// </summary>
        public bool IsGlobal
        {
            get { return (_rootNamespaceRef.Name == GlobalName); }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken1 + " " + ParseToken2; }
        }

        /// <summary>
        /// The name of the referenced root-level namespace.
        /// </summary>
        public string Name
        {
            get { return _rootNamespaceRef.Name; }
        }

        /// <summary>
        /// The referenced root-level namespace.
        /// </summary>
        public SymbolicRef RootNamespaceRef
        {
            get { return _rootNamespaceRef; }
            set { SetField(ref _rootNamespaceRef, value, false); }
        }

        /// <summary>
        /// Add the <see cref="CodeObject"/> to the specified dictionary.
        /// </summary>
        public virtual void AddToDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Add(Name, this);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ExternAlias clone = (ExternAlias)base.Clone();
            clone.CloneField(ref clone._rootNamespaceRef, _rootNamespaceRef);
            return clone;
        }

        /// <summary>
        /// Create a reference to the <see cref="ExternAlias"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>An <see cref="ExternAliasRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new ExternAliasRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName(bool descriptive)
        {
            return Name;
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        public string GetFullName()
        {
            return Name;
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> from the specified dictionary.
        /// </summary>
        public virtual void RemoveFromDictionary(NamedCodeObjectDictionary dictionary)
        {
            dictionary.Remove(Name, this);
        }

        /// <summary>
        /// The primary token used to parse the code object.
        /// </summary>
        public const string ParseToken1 = "extern";

        /// <summary>
        /// The secondary token used to parse the code object.
        /// </summary>
        public const string ParseToken2 = "alias";

        protected ExternAlias(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Token token = (Token)parser.RemoveLastUnused();
            NewLines = token.NewLines;
            SetLineCol(token);
            parser.NextToken();                        // Move past 'alias' keyword
            token = parser.Token;
            string name = parser.GetIdentifierText();  // Parse the name
            RootNamespaceRef = new UnresolvedRef(name, token.LineNumber, token.ColumnNumber);
            ParseTerminator(parser);
        }

        public static void AddParsePoints()
        {
            // Set parse-point on 2nd of the 2 keywords
            Parser.AddParsePoint(ParseToken2, Parse, typeof(NamespaceDecl));
        }

        /// <summary>
        /// Parse an <see cref="ExternAlias"/>.
        /// </summary>
        public static ExternAlias Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Validate 'extern' keyword
            if (parser.LastUnusedTokenText == ParseToken1)
                return new ExternAlias(parser, parent);
            return null;
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_rootNamespaceRef == null || (!_rootNamespaceRef.IsFirstOnLine && _rootNamespaceRef.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _rootNamespaceRef != null)
                {
                    _rootNamespaceRef.IsFirstOnLine = false;
                    _rootNamespaceRef.IsSingleLine = true;
                }
            }
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations, or if
            // it's not another extern alias.
            if (HasFirstOnLineAnnotations || !(previous is ExternAlias))
                return 2;
            return 1;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            _rootNamespaceRef.AsText(writer, flags);
        }
    }
}
