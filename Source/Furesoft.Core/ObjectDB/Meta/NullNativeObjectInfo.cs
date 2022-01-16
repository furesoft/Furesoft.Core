using System.Collections.Generic;
using Furesoft.Core.ObjectDB.Api;

namespace Furesoft.Core.ObjectDB.Meta
{
	/// <summary>
	///   Meta representation of a null native object
	/// </summary>
	internal sealed class NullNativeObjectInfo : NativeObjectInfo
	{
		private static readonly NullNativeObjectInfo instance = new();

		private NullNativeObjectInfo() : base(null, OdbType.Null)
		{
		}

		public NullNativeObjectInfo(int odbTypeId) : base(null, odbTypeId)
		{
		}

		public override string ToString()
		{
			return "null";
		}

		public override bool IsNull()
		{
			return true;
		}

		public override bool IsNative()
		{
			return true;
		}

		public override AbstractObjectInfo CreateCopy(IDictionary<OID, AbstractObjectInfo> cache, bool onlyData)
		{
			return GetInstance();
		}

		public static NullNativeObjectInfo GetInstance()
		{
			return instance;
		}
	}
}