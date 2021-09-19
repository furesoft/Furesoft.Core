using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Properties;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Properties
{
    /// <summary>
    /// Represents a "property" - a virtual field accessed only by calling get and/or set methods.
    /// </summary>
    public class PropertyDecl : PropertyDeclBase
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="PropertyDecl"/> with the specified name, type, and modifiers.
        /// </summary>
        public PropertyDecl(string name, Expression type, Modifiers modifiers)
            : base(name, type, modifiers)
        { }

        /// <summary>
        /// Create a <see cref="PropertyDecl"/> with the specified name and type.
        /// </summary>
        public PropertyDecl(string name, Expression type)
            : base(name, type, Modifiers.None)
        { }

        /// <summary>
        /// Create a <see cref="PropertyDecl"/> with the specified name and type.
        /// </summary>
        public PropertyDecl(Expression name, Expression type)
            : base(name, type, Modifiers.None)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The descriptive category of the code object.
        /// </summary>
        public override string Category
        {
            get { return "property"; }
        }

        /// <summary>
        /// True if the property has a getter method.
        /// </summary>
        public bool HasGetter
        {
            get { return (_body.FindFirst<GetterDecl>() != null); }
        }

        /// <summary>
        /// True if the property has a setter method.
        /// </summary>
        public bool HasSetter
        {
            get { return (_body.FindFirst<SetterDecl>() != null); }
        }

        /// <summary>
        /// The 'getter' method for the property.
        /// </summary>
        public GetterDecl Getter
        {
            get { return _body.FindFirst<GetterDecl>(); }
            set
            {
                if (_body != null)
                {
                    GetterDecl existing = _body.FindFirst<GetterDecl>();
                    if (existing != null)
                        _body.Remove(existing);
                }
                Insert(0, value);  // Always put the 'getter' first
            }
        }

        /// <summary>
        /// The 'setter' method for the property.
        /// </summary>
        public SetterDecl Setter
        {
            get { return _body.FindFirst<SetterDecl>(); }
            set
            {
                if (_body != null)
                {
                    SetterDecl existing = _body.FindFirst<SetterDecl>();
                    if (existing != null)
                        _body.Remove(existing);
                }
                int pos = (_body != null ? _body.Count : 0);
                Insert(pos, value);  // Always put the 'setter' after any 'getter'
            }
        }

        /// <summary>
        /// True if the property is readable.
        /// </summary>
        public override bool IsReadable { get { return HasGetter; } }

        /// <summary>
        /// True if the property is writable.
        /// </summary>
        public override bool IsWritable { get { return HasSetter; } }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Create a reference to the <see cref="PropertyDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>A <see cref="PropertyRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new PropertyRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Get the IsPrivate access right for the specified usage, and if not private then also get the IsProtected and IsInternal rights.
        /// </summary>
        /// <param name="isTargetOfAssignment">Usage - true if the target of an assignment ('lvalue'), otherwise false.</param>
        /// <param name="isPrivate">True if the access is private.</param>
        /// <param name="isProtected">True if the access is protected.</param>
        /// <param name="isInternal">True if the access is internal.</param>
        public override void GetAccessRights(bool isTargetOfAssignment, out bool isPrivate, out bool isProtected, out bool isInternal)
        {
            isPrivate = isProtected = isInternal = false;

            // The access rights of a property actually depend on the rights of the corresponding
            // getter/setter, depending upon whether we're assigning to it or not.
            if (isTargetOfAssignment)
            {
                SetterDecl setterDecl = Setter;
                if (setterDecl != null)
                {
                    isPrivate = setterDecl.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = setterDecl.IsProtected;
                        isInternal = setterDecl.IsInternal;
                    }
                }
            }
            else
            {
                GetterDecl getterDecl = Getter;
                if (getterDecl != null)
                {
                    isPrivate = getterDecl.IsPrivate;
                    if (!isPrivate)
                    {
                        isProtected = getterDecl.IsProtected;
                        isInternal = getterDecl.IsInternal;
                    }
                }
            }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Parse a <see cref="PropertyDecl"/>.
        /// </summary>
        public PropertyDecl(Parser parser, CodeObject parent)
            : base(parser, parent, true)
        { }

        #endregion
    }
}
