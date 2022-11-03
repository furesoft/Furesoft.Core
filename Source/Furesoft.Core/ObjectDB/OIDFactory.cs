using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Oid;

namespace Furesoft.Core.ObjectDB;

	/// <summary>
	/// Factory class to create OIDs
	/// </summary>
	public static class OIDFactory
	{
		/// <summary>
		/// Build object oid based on long number
		/// </summary>
		/// <param name="oid">long number as the base for OID</param>
		/// <returns>Newly created OID</returns>
		public static OID BuildObjectOID(long oid)
		{
			return new ObjectOID(oid);
		}

		/// <summary>
		/// Build class oid based on long number
		/// </summary>
		/// <param name="oid">long number as the base for OID</param>
		/// <returns>Newly created OID</returns>
		public static OID BuildClassOID(long oid)
		{
			return new ClassOID(oid);
		}
	}