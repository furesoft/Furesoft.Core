using Furesoft.Core.ObjectDB.Api.Query;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query;

	internal interface IQueryManager
	{
		int[] GetOrderByAttributeIds(ClassInfo classInfo, IInternalQuery query);

		/// <summary>
		///   Returns a multi class query executor (polymorphic = true)
		/// </summary>
		IQueryExecutor GetQueryExecutor(IQuery query, IStorageEngine engine);
	}