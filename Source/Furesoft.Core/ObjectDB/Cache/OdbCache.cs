using System;
using System.Collections.Generic;
using System.Text;
using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Exceptions;
using Furesoft.Core.ObjectDB.Meta;
using Furesoft.Core.ObjectDB.Storage;
using Furesoft.Core.ObjectDB.Tool.Wrappers;

namespace Furesoft.Core.ObjectDB.Cache;

	/// <summary>
	///   A cache of objects.
	/// </summary>
	internal sealed class OdbCache : IOdbCache
	{
		private static readonly List<Action> cleanMethods = new();

		/// <summary>
		///   Entry to get object info pointers (position,next object pos, previous object pos and class info pos) from the id
		/// </summary>
		private IDictionary<OID, ObjectInfoHeader> _objectInfoHeadersCacheFromOid;

		/// <summary>
		///   <pre>To resolve the update of an id object position:
		///     When an object is full updated(the current object is being deleted and a new one os being created),
		///     the id remain the same but its position change.</pre>
		/// </summary>
		/// <remarks>
		///   <pre>To resolve the update of an id object position:
		///     When an object is full updated(the current object is being deleted and a new one os being created),
		///     the id remain the same but its position change.
		///     But the update is done in transaction, so it is not flushed until the commit happens
		///     So after the update when i need the position to make the old object a pointer, i have no way to get
		///     the right position. To resolve this, i keep a cache of ids where i keep the non commited value</pre>
		/// </remarks>
		private IDictionary<OID, IdInfo> _objectPositionsByIds;

		/// <summary>
		///   object cache - used to know if object exist in the cache TODO use hashcode instead?
		/// </summary>
		private IDictionary<object, OID> _objects;

		/// <summary>
		///   Entry to get an object from its oid
		/// </summary>
		private IDictionary<OID, object> _oids;

		/// <summary>
		///   To keep track of the oid that have been created or modified in the current transaction
		/// </summary>
		private IDictionary<OID, OID> _unconnectedZoneOids;

		public OdbCache()
		{
			_objects = new OdbHashMap<object, OID>();
			_oids = new OdbHashMap<OID, object>();
			_unconnectedZoneOids = new OdbHashMap<OID, OID>();
			_objectInfoHeadersCacheFromOid = new OdbHashMap<OID, ObjectInfoHeader>();
			_objectPositionsByIds = new OdbHashMap<OID, IdInfo>();
		}

		#region IOdbCache Members

		public void AddObject(OID oid, object o, ObjectInfoHeader objectInfoHeader)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			_oids[oid] = o;
			_objects[o] = oid;
			_objectInfoHeadersCacheFromOid[oid] = objectInfoHeader;
		}

		/// <summary>
		///   Only adds the Object info - used for non committed objects
		/// </summary>
		public void AddObjectInfoOfNonCommitedObject(ObjectInfoHeader objectInfoHeader)
		{
			if (objectInfoHeader.GetOid() == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			if (objectInfoHeader.GetClassInfoId() == null)
			{
				throw new OdbRuntimeException(
					NDatabaseError.CacheObjectInfoHeaderWithoutClassId.AddParameter(objectInfoHeader.GetOid()));
			}

			_objectInfoHeadersCacheFromOid[objectInfoHeader.GetOid()] = objectInfoHeader;
		}

		public void StartInsertingObjectWithOid<T>(T plainObject, OID oid) where T : class
		{
			// In this case oid can be -1,because object is beeing inserted and do
			// not have yet a defined oid.
			if (plainObject == null)
				return;

			Cache<T>.InsertingObjects.TryGetValue(plainObject, out var objectInsertingOID);

			if (objectInsertingOID == null)
				Cache<T>.InsertingObjects[plainObject] = oid;
		}

		public void UpdateIdOfInsertingObject<T>(T plainObject, OID oid) where T : class
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			var insertingObjectOid = Cache<T>.InsertingObjects[plainObject];
			if (insertingObjectOid != null)
				Cache<T>.InsertingObjects[plainObject] = oid;
		}

		public void RemoveObjectByOid(OID oid)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			_oids.TryGetValue(oid, out var value);

			_oids.Remove(oid);
			if (value != null)
				_objects.Remove(value);

			_objectInfoHeadersCacheFromOid.Remove(oid);
			_unconnectedZoneOids.Remove(oid);
		}

		public void RemoveObject(object o)
		{
			if (o == null)
			{
				throw new OdbRuntimeException(
					NDatabaseError.CacheNullObject.AddParameter(" while removing object from the cache"));
			}

			var success = _objects.TryGetValue(o, out var oid);
			if (!success)
				return;

			_oids.Remove(oid);
			_objects.Remove(o);
			_objectInfoHeadersCacheFromOid.Remove(oid);
			_unconnectedZoneOids.Remove(oid);
		}

		public bool Contains(object @object)
		{
			return _objects.ContainsKey(@object);
		}

		public object GetObject(OID oid)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid.AddParameter("oid"));

			_oids.TryGetValue(oid, out var value);

			return value;
		}

		public ObjectInfoHeader GetObjectInfoHeaderFromObject(object o)
		{
			var success = _objects.TryGetValue(o, out var oid);

			if (!success)
				return null;

			_objectInfoHeadersCacheFromOid.TryGetValue(oid, out var objectInfoHeader);

			return objectInfoHeader;
		}

		public ObjectInfoHeader GetObjectInfoHeaderByOid(OID oid, bool throwExceptionIfNotFound)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			_objectInfoHeadersCacheFromOid.TryGetValue(oid, out var objectInfoHeader);
			if (objectInfoHeader == null && throwExceptionIfNotFound)
				throw new OdbRuntimeException(NDatabaseError.ObjectWithOidDoesNotExistInCache.AddParameter(oid));

			return objectInfoHeader;
		}

		public OID GetOid(object o)
		{
			_objects.TryGetValue(o, out var oid);

			return oid ?? StorageEngineConstant.NullObjectId;
		}

		public void SavePositionOfObjectWithOid(OID oid, long objectPosition)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			var idInfo = new IdInfo(objectPosition, IDStatus.Active);
			_objectPositionsByIds[oid] = idInfo;
		}

		public void MarkIdAsDeleted(OID oid)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			_objectPositionsByIds.TryGetValue(oid, out var idInfo);
			if (idInfo != null)
			{
				idInfo.Status = IDStatus.Deleted;
			}
			else
			{
				idInfo = new IdInfo(-1, IDStatus.Deleted);
				_objectPositionsByIds[oid] = idInfo;
			}
		}

		public bool IsDeleted(OID oid)
		{
			if (oid == null)
				throw new OdbRuntimeException(NDatabaseError.CacheNullOid);

			_objectPositionsByIds.TryGetValue(oid, out var idInfo);
			if (idInfo != null)
				return idInfo.Status == IDStatus.Deleted;

			return false;
		}

		/// <summary>
		///   Returns the position or -1 if it is not is the cache or StorageEngineConstant.NULL_OBJECT_ID_ID if it has been marked as deleted
		/// </summary>
		public long GetObjectPositionByOid(OID oid)
		{
			if (oid == null)
				return StorageEngineConstant.NullObjectIdId;

			_objectPositionsByIds.TryGetValue(oid, out var idInfo);

			if (idInfo == null)
				return StorageEngineConstant.ObjectIsNotInCache;

			// object is not in the cache
			return !IDStatus.IsActive(idInfo.Status)
					   ? StorageEngineConstant.DeletedObjectPosition
					   : idInfo.Position;
		}

		public void ClearOnCommit()
		{
			_objectPositionsByIds.Clear();
			_unconnectedZoneOids.Clear();
		}

		public void Clear(bool setToNull)
		{
			if (_objects != null)
			{
				_objects.Clear();
				_oids.Clear();
				_objectInfoHeadersCacheFromOid.Clear();

				foreach (var clear in cleanMethods)
					clear();

				_objectPositionsByIds.Clear();
				_unconnectedZoneOids.Clear();
			}

			if (!setToNull)
				return;

			_objects = null;
			_oids = null;
			_objectInfoHeadersCacheFromOid = null;
			_objectPositionsByIds = null;
			_unconnectedZoneOids = null;
		}

		public void ClearInsertingObjects()
		{
			foreach (var clear in cleanMethods)
				clear();
		}

		public OID IdOfInsertingObject<T>(T plainObject) where T : class
		{
			if (plainObject == null)
				return StorageEngineConstant.NullObjectId;

			Cache<T>.InsertingObjects.TryGetValue(plainObject, out var objectInsertingOid);

			return objectInsertingOid ?? StorageEngineConstant.NullObjectId;
		}

		public bool IsInCommitedZone(OID oid)
		{
			return !_unconnectedZoneOids.ContainsKey(oid);
		}

		public void AddOIDToUnconnectedZone(OID oid)
		{
			_unconnectedZoneOids.Add(oid, oid);
		}

		#endregion IOdbCache Members

		public override string ToString()
		{
			var buffer = new StringBuilder();
			buffer.Append("Cache=");
			buffer.Append(_objects.Count).Append(" objects ");
			buffer.Append(_oids.Count).Append(" oids ");
			buffer.Append(_objectInfoHeadersCacheFromOid.Count).Append(" object headers ");
			buffer.Append(_objectPositionsByIds.Count).Append(" positions by oid");
			return buffer.ToString();
		}

		#region Nested type: Cache

		private static class Cache<T>
		{
			/// <summary>
			///   To resolve cyclic reference, keep track of objects being inserted
			/// </summary>
			public static readonly IDictionary<T, OID> InsertingObjects = new OdbHashMap<T, OID>();

			static Cache()
			{
				cleanMethods.Add(InsertingObjects.Clear);
			}
		}

		#endregion Nested type: Cache
	}