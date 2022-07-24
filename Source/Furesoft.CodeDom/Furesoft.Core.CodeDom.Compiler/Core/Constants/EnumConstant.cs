namespace Furesoft.Core.CodeDom.Compiler.Core.Constants
{
    public sealed class EnumConstant : Constant
    {
        /// <summary>
        /// Creates a constant from a value.
        /// </summary>
        /// <param name="value">The constant value.</param>
        public EnumConstant(object value, IType type)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Type = type;
        }

        /// <summary>
        /// Gets the value represented by this constant.
        /// </summary>
        /// <returns>The constant value.</returns>
        public object Value { get; private set; }

        public IType Type { get; }

        /// <inheritdoc/>
        public override bool Equals(Constant other)
        {
            return other is EnumConstant
                && Value == ((EnumConstant)other).Value;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}