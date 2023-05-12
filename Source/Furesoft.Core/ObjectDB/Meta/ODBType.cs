using System;
using System.Collections.Generic;
using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Exceptions;
using Furesoft.Core.ObjectDB.Oid;
using Furesoft.Core.ObjectDB.Tool;
using Furesoft.Core.ObjectDB.TypeResolution;

namespace Furesoft.Core.ObjectDB.Meta;

	/// <summary>
	///   Contains the list for the ODB types
	/// </summary>
	public sealed class OdbType
	{
		private static readonly IDictionary<int, OdbType> typesById = new Dictionary<int, OdbType>();
		private static readonly IDictionary<string, OdbType> typesByName = new Dictionary<string, OdbType>();

		/// <summary>
		///   This cache is used to cache non default types.
		/// </summary>
		/// <remarks>
		///   This cache is used to cache non default types.
		///   Instead or always testing if a class is an array
		///   or a collection or any other, we put the odb type in this cache
		/// </remarks>
		private static readonly Dictionary<string, OdbType> cacheOfTypesByName =
			new();

		public static readonly string DefaultArrayComponentClassName = OdbClassNameResolver.GetFullName(typeof(object));

		private readonly int _id;
		private readonly bool _isPrimitive;
		private readonly string _name;
		private readonly int _size;

		/// <summary>
		///   For array element type
		/// </summary>
		private OdbType _subType;

		static OdbType()
		{
			IList<OdbType> allTypes = new List<OdbType>(32);
			//// DO NOT FORGET DO ADD THE TYPE IN THIS LIST WHEN CREATING A NEW ONE!!!
			allTypes.Add(Null);
			allTypes.Add(Byte);
			allTypes.Add(SByte);
			allTypes.Add(Short);
			allTypes.Add(UShort);
			allTypes.Add(Integer);
			allTypes.Add(UInteger);
			allTypes.Add(Long);
			allTypes.Add(ULong);
			allTypes.Add(Float);
			allTypes.Add(Double);
			allTypes.Add(Decimal);
			allTypes.Add(Character);
			allTypes.Add(Boolean);
			allTypes.Add(Date);
			allTypes.Add(String);
			allTypes.Add(Enum);
			allTypes.Add(Array);
			allTypes.Add(Oid);
			allTypes.Add(ObjectOid);
			allTypes.Add(ClassOid);
			allTypes.Add(NonNative);

			foreach (var type in allTypes)
			{
				typesByName[type.Name] = type;
				typesById[type.Id] = type;
			}
		}

		private OdbType(bool isPrimitive, int id, string name, int size)
		{
			_isPrimitive = isPrimitive;
			_id = id;
			_name = name;
			_size = size;
		}

		public int Id
		{
			get { return _id; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int Size
		{
			get { return _size; }
		}

		public OdbType SubType
		{
			get { return _subType; }
			set { _subType = value; }
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_id * 397) ^ (_name != null
										  ? _name.GetHashCode()
										  : 0);
			}
		}

		public OdbType Copy(string name)
		{
			return new(_isPrimitive, _id, name, _size) { _subType = SubType };
		}

		public OdbType Copy()
		{
			return Copy(_name);
		}

		public static OdbType GetFromId(int id)
		{
			var odbType = typesById[id];

			if (odbType == null)
				throw new OdbRuntimeException(NDatabaseError.OdbTypeIdDoesNotExist.AddParameter(id));

			return odbType;
		}

		public static string GetNameFromId(int id)
		{
			return GetFromId(id).Name;
		}

		public static OdbType GetFromName(string fullName)
		{
			typesByName.TryGetValue(fullName, out var odbType);

			return odbType ?? new OdbType(NonNative._isPrimitive, NonNativeId, fullName, 0);
		}

		public static OdbType GetFromClass(Type clazz)
		{
			if (clazz.IsEnum)
				return new(Enum._isPrimitive, EnumId, OdbClassNameResolver.GetFullName(clazz), 0);

			var className = OdbClassNameResolver.GetFullName(clazz);

			// First check if it is a 'default type'
			var success = typesByName.TryGetValue(className, out var odbType);
			if (success)
				return odbType;

			// Then check if it is a 'non default type'
			success = cacheOfTypesByName.TryGetValue(className, out odbType);
			if (success)
				return odbType;

			if (clazz.IsArray)
			{
				var type = new OdbType(Array._isPrimitive, ArrayId, Array.Name, 0)
				{ _subType = GetFromClass(clazz.GetElementType()) };

				return cacheOfTypesByName.GetOrAdd(className, type);
			}

			var nonNative = new OdbType(NonNative._isPrimitive, NonNativeId, className, 0);
			return cacheOfTypesByName.GetOrAdd(className, nonNative);
		}

		public static bool IsNative(Type clazz)
		{
			var success = typesByName.TryGetValue(OdbClassNameResolver.GetFullName(clazz), out var odbType);

			return success || clazz.IsArray;
		}

		public bool IsArray()
		{
			return _id == ArrayId;
		}

		public static bool IsArray(int odbTypeId)
		{
			return odbTypeId == ArrayId;
		}

		public bool IsNative()
		{
			return _id != NonNativeId;
		}

		public override string ToString()
		{
			return string.Concat(_id.ToString(), " - ", _name);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != typeof(OdbType))
				return false;
			var type = (OdbType)obj;
			return Id == type.Id;
		}

		public Type GetNativeClass()
		{
			switch (_id)
			{
				case BooleanId:
					return typeof(bool);

				case ByteId:
					return typeof(byte);

				case SByteId:
					return typeof(sbyte);

				case CharacterId:
					return typeof(char);

				case DoubleId:
					return typeof(double);

				case FloatId:
					return typeof(float);

				case IntegerId:
					return typeof(int);

				case UIntegerId:
					return typeof(uint);

				case LongId:
					return typeof(long);

				case ULongId:
					return typeof(ulong);

				case ShortId:
					return typeof(short);

				case UShortId:
					return typeof(ushort);

				case DecimalId:
					return typeof(decimal);

				case ObjectOidId:
					return typeof(ObjectOID);

				case ClassOidId:
					return typeof(ClassOID);

				case OidId:
					return typeof(OID);
			}

			return TypeResolutionUtils.ResolveType(Name);
		}

		public bool IsNonNative()
		{
			return _id == NonNativeId;
		}

		public static bool IsNull(int odbTypeId)
		{
			return odbTypeId == NullId;
		}

		public static bool IsAtomicNative(int odbTypeId)
		{
			return (odbTypeId > 0 && odbTypeId <= NativeMaxId);
		}

		public bool IsAtomicNative()
		{
			return IsAtomicNative(_id);
		}

		public static bool IsEnum(int odbTypeId)
		{
			return odbTypeId == EnumId;
		}

		public bool IsEnum()
		{
			return IsEnum(_id);
		}

		public static bool TypesAreCompatible(OdbType type1, OdbType type2)
		{
			if (type1 == null || type2 == null)
				return false;

			if (type1.IsArray() && type2.IsArray())
				return TypesAreCompatible(type1.SubType, type2.SubType);

			if (type1.Name.Equals(type2.Name))
				return true;

			if (type1.IsEnum() && type2.IsEnum())
				return type1.GetNativeClass() == type2.GetNativeClass();

			if (type1.IsNative() && type2.IsNative())
				return type1.IsEquivalent(type2);

			if (type1.IsNonNative() && type2.IsNonNative())
			{
				return (type1.GetNativeClass() == type2.GetNativeClass()) ||
					   (type1.GetNativeClass().IsAssignableFrom(type2.GetNativeClass()));
			}

			return false;
		}

		private bool IsEquivalent(OdbType type2)
		{
			return (_id == IntegerId && type2._id == IntegerId);
		}

		#region Odb Type Ids

		public const int NullId = 0;

		public const int BooleanId = 10;

		/// <summary>
		///   1 byte
		/// </summary>
		public const int ByteId = 20;

		/// <summary>
		///   1 byte
		/// </summary>
		public const int SByteId = 21;

		public const int CharacterId = 30;

		/// <summary>
		///   2 byte
		/// </summary>
		public const int ShortId = 40;

		/// <summary>
		///   2 byte
		/// </summary>
		public const int UShortId = 41;

		/// <summary>
		///   4 byte
		/// </summary>
		public const int IntegerId = 50;

		/// <summary>
		///   4 byte
		/// </summary>
		public const int UIntegerId = 51;

		/// <summary>
		///   8 bytes
		/// </summary>
		public const int LongId = 60;

		/// <summary>
		///   8 bytes
		/// </summary>
		public const int ULongId = 61;

		/// <summary>
		///   4 byte
		/// </summary>
		public const int FloatId = 70;

		/// <summary>
		///   8 byte
		/// </summary>
		public const int DoubleId = 80;

		/// <summary>
		///   16 byte
		/// </summary>
		public const int DecimalId = 100;

		public const int DateId = 170;

		public const int OidId = 180;

		public const int ObjectOidId = 181;

		public const int ClassOidId = 182;

		public const int StringId = 210;

		/// <summary>
		///   Enums are internally stored as String: the enum name
		/// </summary>
		public const int EnumId = 211;

		private const int NativeMaxId = StringId;

		public const int ArrayId = 260;

		public const int NonNativeId = 300;

		#endregion Odb Type Ids

		#region Odb Types

		public static readonly OdbType Null = new(true, NullId, "null", 1);

		/// <summary>
		///   1 byte
		/// </summary>
		public static readonly OdbType Byte = new(true, ByteId, OdbClassNameResolver.GetFullName(typeof(byte)), 1);

		/// <summary>
		///   1 byte
		/// </summary>
		public static readonly OdbType SByte = new(true, SByteId, OdbClassNameResolver.GetFullName(typeof(sbyte)), 1);

		/// <summary>
		///   2 byte
		/// </summary>
		public static readonly OdbType Short = new(true, ShortId, OdbClassNameResolver.GetFullName(typeof(short)), 2);

		/// <summary>
		///   2 byte
		/// </summary>
		public static readonly OdbType UShort = new(true, UShortId, OdbClassNameResolver.GetFullName(typeof(ushort)), 2);

		/// <summary>
		///   4 byte
		/// </summary>
		public static readonly OdbType Integer = new(true, IntegerId, OdbClassNameResolver.GetFullName(typeof(int)), 4);

		/// <summary>
		///   4 byte
		/// </summary>
		public static readonly OdbType UInteger = new(true, UIntegerId, OdbClassNameResolver.GetFullName(typeof(uint)), 4);

		/// <summary>
		///   16 byte
		/// </summary>
		public static readonly OdbType Decimal = new(true, DecimalId, OdbClassNameResolver.GetFullName(typeof(decimal)),
															 16);

		/// <summary>
		///   8 bytes
		/// </summary>
		public static readonly OdbType Long = new(true, LongId, OdbClassNameResolver.GetFullName(typeof(long)), 8);

		/// <summary>
		///   8 bytes
		/// </summary>
		public static readonly OdbType ULong = new(true, ULongId, OdbClassNameResolver.GetFullName(typeof(ulong)), 8);

		/// <summary>
		///   4 byte
		/// </summary>
		public static readonly OdbType Float = new(true, FloatId, OdbClassNameResolver.GetFullName(typeof(float)), 4);

		/// <summary>
		///   8 byte
		/// </summary>
		public static readonly OdbType Double = new(true, DoubleId, OdbClassNameResolver.GetFullName(typeof(double)), 8);

		/// <summary>
		///   2 byte
		/// </summary>
		public static readonly OdbType Character = new(true, CharacterId,
															   OdbClassNameResolver.GetFullName(typeof(char)), 2);

		/// <summary>
		///   1 byte
		/// </summary>
		public static readonly OdbType Boolean = new(true, BooleanId, OdbClassNameResolver.GetFullName(typeof(bool)), 1);

		/// <summary>
		///   8 byte
		/// </summary>
		public static readonly OdbType Date = new(false, DateId, OdbClassNameResolver.GetFullName(typeof(DateTime)), 8);

		public static readonly OdbType String = new(false, StringId, OdbClassNameResolver.GetFullName(typeof(string)),
															1);

		public static readonly OdbType Enum = new(false, EnumId, OdbClassNameResolver.GetFullName(typeof(Enum)), 1);

		public static readonly OdbType Array = new(false, ArrayId, "array", 0);

		public static readonly OdbType Oid = new(false, OidId, OdbClassNameResolver.GetFullName(typeof(OID)), 0);

		public static readonly OdbType ObjectOid = new(false, ObjectOidId,
															   OdbClassNameResolver.GetFullName(typeof(ObjectOID)), 0);

		public static readonly OdbType ClassOid = new(false, ClassOidId,
															  OdbClassNameResolver.GetFullName(typeof(ClassOID)), 0);

		public static readonly OdbType NonNative = new(false, NonNativeId, "non native", 0);

		#endregion Odb Types

		#region Type Sizes

		public static readonly int SizeOfInt = Integer.Size;

		public static readonly int SizeOfLong = Long.Size;

		public static readonly int SizeOfByte = Byte.Size;

		#endregion Type Sizes
	}