using System;
using Furesoft.Core.ObjectDB.Cache;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Session
{
	public interface ISession : IComparable
	{
		IOdbCache GetCache();

		IReadObjectsCache GetTmpCache();

		void Rollback();

		void Close();

		bool IsRollbacked();

		IStorageEngine GetStorageEngine();

		bool TransactionIsPending();

		void Commit();

		ITransaction GetTransaction();

		void SetFileSystemInterfaceToApplyTransaction(IFileSystemInterface fsi);

		IMetaModel GetMetaModel();

		string GetId();

		void RemoveObjectFromCache(object @object);

		IObjectWriter GetObjectWriter();
	}
}