namespace Furesoft.Core.Signals
{
	public static class ObjectExtentions
	{
		public static Optional<T> ToOptional<T>(this T target)
		{
			return Optional<T>.Some(target);
		}
	}

	public class Optional<T>
	{
		public static Optional<T> None => new Optional<T>();

		public bool HasValue { get; }

		public T Value { get; }

		public Optional(T value)
		{
			if (value != null)
			{
				HasValue = true;
				Value = value;
			}
		}

		public Optional()
		{
		}

		public static implicit operator bool(Optional<T> opt)
		{
			return opt.HasValue;
		}

		public static implicit operator Optional<T>(bool value) => None;

		public static implicit operator T(Optional<T> opt)
		{
			if (opt.HasValue)
			{
				return opt.Value;
			}

			return default;
		}

		public static Optional<TValue> Some<TValue>(TValue value)
		{
			return new Optional<TValue>(value);
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}