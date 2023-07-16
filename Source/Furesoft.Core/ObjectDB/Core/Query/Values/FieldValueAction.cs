using System.Collections;
using System.Text;
using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Core.Query.Execution;
using Furesoft.Core.ObjectDB.Core.Query.List;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query.Values;

	/// <summary>
	///   An action to retrieve an object field
	/// </summary>
	internal sealed class FieldValueAction : AbstractQueryFieldAction
	{
		/// <summary>
		///   The value of the attribute
		/// </summary>
		private object _value;

		public FieldValueAction(string attributeName, string alias) : base(attributeName, alias, true)
		{
			_value = null;
		}

		public override void Execute(OID oid, AttributeValuesMap values)
		{
			_value = values[AttributeName];
			if (!(_value is ICollection || IsGenericCollection(_value.GetType())))
				return;

			// For collection,we encapsulate it in an lazy load list that will create objects on demand
			var c = ((IEnumerable)_value).Cast<object>().ToList();
			var l = new LazySimpleListOfAoi<object>(GetInstanceBuilder(), ReturnInstance());
			l.AddRange(c);
			_value = l;
		}

		private static bool IsGenericCollection(Type type)
		{
			return type.GetInterfaces()
							.Any(x => x.IsGenericType &&
							x.GetGenericTypeDefinition() == typeof(ICollection<>));
		}

		public override object GetValue()
		{
			return _value;
		}

		public override string ToString()
		{
			var buffer = new StringBuilder();
			buffer.Append(AttributeName).Append("=").Append(_value);
			return buffer.ToString();
		}

		public override void End()
		{
		}

		public override void Start()
		{
		}

		public override IQueryFieldAction Copy()
		{
			return new FieldValueAction(AttributeName, Alias);
		}
	}