using Furesoft.Core.ObjectDB.Api.Query;
using Furesoft.Core.ObjectDB.Core.Query.Criteria;
using Furesoft.Core.ObjectDB.Core.Query.Execution;
using Furesoft.Core.ObjectDB.Core.Query.Values;
using Furesoft.Core.ObjectDB.Exceptions;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query;

	internal class QueryManager : IQueryManager
	{
		public int[] GetOrderByAttributeIds(ClassInfo classInfo, IInternalQuery query)
		{
			var fieldNames = query.GetOrderByFieldNames();
			var fieldIds = new int[fieldNames.Count];

			for (var i = 0; i < fieldNames.Count; i++)
				fieldIds[i] = classInfo.GetAttributeId(fieldNames[i]);

			return fieldIds;
		}

		/// <summary>
		///   Returns a multi class query executor (polymorphic = true)
		/// </summary>
		public IQueryExecutor GetQueryExecutor(IQuery query, IStorageEngine engine)
		{
			if (query is ValuesCriteriaQuery)
				return new MultiClassGenericQueryExecutor(new ValuesCriteriaQueryExecutor(query, engine));

			if (query is SodaQuery)
				return new MultiClassGenericQueryExecutor(new CriteriaQueryExecutor(query, engine));

			throw new OdbRuntimeException(NDatabaseError.QueryTypeNotImplemented.AddParameter(query.GetType().FullName));
		}
	}