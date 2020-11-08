using System;
using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Core.Query.Execution;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query.Values
{
	internal sealed class SumAction : AbstractQueryFieldAction
	{
		private decimal _sum;

		public SumAction(string attributeName, string alias) : base(attributeName, alias, false)
		{
			_sum = new decimal(0);
		}

		public override void Execute(OID oid, AttributeValuesMap values)
		{
			var number = Convert.ToDecimal(values[AttributeName]);
			_sum = decimal.Add(_sum, ValuesUtil.Convert(number));
		}

		public decimal GetSum()
		{
			return _sum;
		}

		public override object GetValue()
		{
			return _sum;
		}

		public override void End()
		{
		}

		public override void Start()
		{
		}

		public override IQueryFieldAction Copy()
		{
			return new SumAction(AttributeName, Alias);
		}
	}
}