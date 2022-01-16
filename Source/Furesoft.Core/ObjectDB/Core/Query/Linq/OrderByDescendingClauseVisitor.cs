using System.Collections.Generic;
using System.Linq.Expressions;
using Furesoft.Core.ObjectDB.Api.Query;

namespace Furesoft.Core.ObjectDB.Core.Query.Linq
{
	internal sealed class OrderByDescendingClauseVisitor : OrderByClauseVisitorBase
	{
		private static readonly Dictionary<Expression, IQueryBuilderRecord> cache =
			new();

		protected override Dictionary<Expression, IQueryBuilderRecord> GetCachingStrategy()
		{
			return cache;
		}

		protected override void ApplyDirection(IQuery query)
		{
			query.OrderDescending();
		}
	}
}