using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Exceptions;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Cache;

	/// <summary>
	///   A temporary cache of objects.
	/// </summary>
	internal sealed class ReadObjectsCache : IReadObjectsCache
	{
		/// <summary>
		///   To resolve cyclic reference, keep track of objects being read
		/// </summary>
		private readonly IDictionary<OID, NonNativeObjectInfo> _readingObjectInfo;

		public ReadObjectsCache()
		{
			_readingObjectInfo = new Dictionary<OID, NonNativeObjectInfo>();
		}

		#region IReadObjectsCache Members

		public bool IsReadingObjectInfoWithOid(OID oid)
		{
			return oid != null && _readingObjectInfo.ContainsKey(oid);
		}

		public NonNativeObjectInfo GetObjectInfoByOid(OID oid)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			_readingObjectInfo.TryGetValue(oid, out var value);

			return value;
		}

		public void StartReadingObjectInfoWithOid(OID oid, NonNativeObjectInfo objectInfo)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			var success = _readingObjectInfo.TryGetValue(oid, out var nnoi);

			if (!success)
				_readingObjectInfo[oid] = objectInfo;
		}

		public void ClearObjectInfos()
		{
			_readingObjectInfo.Clear();
		}

		#endregion IReadObjectsCache Members
	}