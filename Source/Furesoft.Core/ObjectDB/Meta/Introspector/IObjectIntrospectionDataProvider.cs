using System;

namespace Furesoft.Core.ObjectDB.Meta.Introspector;

	public interface IObjectIntrospectionDataProvider
	{
		ClassInfo GetClassInfo(Type type);

		void Clear();

		NonNativeObjectInfo EnrichWithOid(NonNativeObjectInfo nnoi, object o);
	}