using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Core.Query.Execution;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query.Values
{
	/// <summary>
	///   An action to count objects of a query
	/// </summary>
	internal sealed class CountAction : AbstractQueryFieldAction
	{
		private static readonly decimal one = new decimal(1);

		private decimal _count;

		public CountAction(string alias) : base(alias, alias, false)
		{
			_count = new decimal(0);
		}

		public override void Execute(OID oid, AttributeValuesMap values)
		{
			_count = decimal.Add(_count, one);
		}

		public decimal GetCount()
		{
			return _count;
		}

		public override object GetValue()
		{
			return _count;
		}

		public override void End()
		{
		}

		// Nothing to do
		public override void Start()
		{
		}

		// Nothing to do
		public override IQueryFieldAction Copy()
		{
			return new CountAction(Alias);
		}
	}
}