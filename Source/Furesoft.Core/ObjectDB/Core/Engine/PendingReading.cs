using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Engine
{
	internal sealed class PendingReading
	{
		private readonly OID _attributeOID;
		private readonly ClassInfo _ci;
		private readonly int _id;

		public PendingReading(int id, ClassInfo ci, OID attributeOID)
		{
			_id = id;
			_ci = ci;
			_attributeOID = attributeOID;
		}

		public int GetId()
		{
			return _id;
		}

		public ClassInfo GetCi()
		{
			return _ci;
		}

		public OID GetAttributeOID()
		{
			return _attributeOID;
		}
	}
}