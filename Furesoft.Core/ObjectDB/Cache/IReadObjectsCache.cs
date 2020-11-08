using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Cache
{
	/// <summary>
	///   An interface for temporary cache
	/// </summary>
	public interface IReadObjectsCache
	{
		NonNativeObjectInfo GetObjectInfoByOid(OID oid);

		bool IsReadingObjectInfoWithOid(OID oid);

		void StartReadingObjectInfoWithOid(OID oid, NonNativeObjectInfo objectInfo);

		void ClearObjectInfos();
	}
}