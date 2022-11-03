using System.Text;
using Furesoft.Core.ObjectDB.Api.Query;
using Furesoft.Core.ObjectDB.Meta;
using Furesoft.Core.ObjectDB.Tool.Wrappers;

namespace Furesoft.Core.ObjectDB.Core.Query.Criteria;

	internal sealed class Not : AbstractExpression
	{
		private readonly IInternalConstraint _criterion;

		public Not(IQuery query, IConstraint criterion) : base(query)
		{
			_criterion = (IInternalConstraint)criterion;
		}

		public override bool Match(object @object)
		{
			return !_criterion.Match(@object);
		}

		public override string ToString()
		{
			var buffer = new StringBuilder();
			buffer.Append(" not ").Append(_criterion);
			return buffer.ToString();
		}

		public override IOdbList<string> GetAllInvolvedFields()
		{
			return _criterion.GetAllInvolvedFields();
		}

		public override AttributeValuesMap GetValues()
		{
			return new AttributeValuesMap();
		}
	}