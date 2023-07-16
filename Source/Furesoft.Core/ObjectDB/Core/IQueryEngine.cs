using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Api.Query;

namespace Furesoft.Core.ObjectDB.Core;

	public interface IQueryEngine
	{
		IValues GetValues(IInternalValuesQuery query, int startIndex, int endIndex);

		long Count(Type underlyingType, IConstraint constraint);

		IInternalObjectSet<T> GetObjects<T>(IQuery query, bool inMemory, int startIndex, int endIndex);

		OID GetObjectId<T>(T plainObject, bool throwExceptionIfDoesNotExist) where T : class;

		object GetObjectFromOid(OID oid);
	}