using System;
using System.Collections;
using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Api.Query;
using Furesoft.Core.ObjectDB.Exceptions;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query.Criteria.Evaluations;

	internal sealed class ContainsEvaluation : AEvaluation
	{
		/// <summary>
		///   For criteria query on objects, we use the oid of the object instead of the object itself.
		/// </summary>
		/// <remarks>
		///   For criteria query on objects, we use the oid of the object instead of the object itself.
		///   So comparison will be done with OID It is faster and avoid the need of the object
		///   (class) having to implement Serializable in client server mode
		/// </remarks>
		private readonly OID _oid;

		private readonly IInternalQuery _query;

		public ContainsEvaluation(object theObject, string attributeName, IQuery query)
			: base(theObject, attributeName)
		{
			_query = (IInternalQuery)query;

			if (IsNative())
				return;

			// For non native object, we just need the oid of it
			_oid = _query.GetQueryEngine().GetObjectId(TheObject, false);
		}

		public override bool Evaluate(object candidate)
		{
			candidate = AsAttributeValuesMapValue(candidate);

			if (candidate == null && TheObject == null && _oid == null)
				return true;

			if (candidate == null)
				return false;

			if (candidate is OID oid)
			{
				candidate = _query.GetQueryEngine().GetObjectFromOid(oid);
			}
			// The value in the map, just take the object with the attributeName

			if (candidate is IDictionary map)
			{
				candidate = map[AttributeName];

				// The value valueToMatch was redefined, so we need to re-make some
				// tests
				if (candidate == null && TheObject == null && _oid == null)
					return true;

				if (candidate == null)
					return false;
			}

			var clazz = candidate.GetType();

			if (clazz.IsArray)
				return CheckIfArrayContainsValue(candidate);

			if (candidate is ICollection collection)
				return CheckIfCollectionContainsValue(collection);

			if (candidate is string candidateAsString)
				return CheckIfStringContainsValue(candidateAsString);

			throw new OdbRuntimeException(
				NDatabaseError.QueryContainsCriterionTypeNotSupported.AddParameter(candidate.GetType().FullName));
		}

		private bool CheckIfStringContainsValue(string candidate)
		{
			return TheObject is string value && candidate.Contains(value);
		}

		private bool CheckIfCollectionContainsValue(ICollection collection)
		{
			var typeDefinition = GetTypeDefinition(collection);

			// If the object to compared is native
			if (IsNative())
				return CheckIfCollectionContainsNativeValue(collection, typeDefinition);

			return typeDefinition == typeof(AbstractObjectInfo)
					   ? CheckIfCollectionContainsAbstractObjectInfo(collection)
					   : CheckIfCollectionContainsItem(collection);
		}

		private bool CheckIfCollectionContainsItem(IEnumerable collection)
		{
			foreach (var item in collection)
			{
				if (item == null && TheObject == null && _oid == null)
					return true;

				if (Equals(item, TheObject))
					return true;

				if (_oid == null || item == null)
					continue;

				if (!OdbType.IsNative(item.GetType()))
					continue;

				if (Equals(item, _oid))
					return true;
			}
			return false;
		}

		private bool CheckIfCollectionContainsAbstractObjectInfo(IEnumerable collection)
		{
			foreach (AbstractObjectInfo abstractObjectInfo in collection)
			{
				if (abstractObjectInfo.IsNull() && TheObject == null && _oid == null)
					return true;

				if (_oid == null)
					continue;

				if (!abstractObjectInfo.IsNonNativeObject())
					continue;

				var nnoi1 = (NonNativeObjectInfo)abstractObjectInfo;
				var isEqual = nnoi1.GetOid() != null && _oid != null && nnoi1.GetOid().Equals(_oid);

				if (isEqual)
					return true;
			}

			return false;
		}

		private bool CheckIfCollectionContainsNativeValue(IEnumerable collection, Type typeDefinition)
		{
			if (typeDefinition == typeof(AbstractObjectInfo))
			{
				foreach (AbstractObjectInfo abstractObjectInfo in collection)
				{
					if (abstractObjectInfo == null && TheObject == null)
						return true;

					if (abstractObjectInfo != null && TheObject == null)
						return false;

					if (abstractObjectInfo != null && TheObject.Equals(abstractObjectInfo.GetObject()))
						return true;
				}
			}
			else
			{
				foreach (var item in collection)
				{
					if (item == null && TheObject == null)
						return true;

					if (item != null && TheObject == null)
						return false;

					if (TheObject.Equals(item))
						return true;
				}
			}

			return false;
		}

		private static Type GetTypeDefinition(ICollection collection)
		{
			var typeDefinition = typeof(object);

			if (collection.GetType().IsGenericType)
			{
				typeDefinition = collection.GetType().GetGenericArguments()[0];
			}
			else
			{
				if (collection.Count > 0)
				{
					var enumerator = collection.GetEnumerator();
					enumerator.MoveNext();
					if (enumerator.Current is AbstractObjectInfo abstractObjectInfo)
						typeDefinition = typeof(AbstractObjectInfo);
				}
			}
			return typeDefinition;
		}

		private bool CheckIfArrayContainsValue(object valueToMatch)
		{
			var arrayLength = ((Array)valueToMatch).GetLength(0);
			for (var i = 0; i < arrayLength; i++)
			{
				var element = ((Array)valueToMatch).GetValue(i);
				if (element == null && TheObject == null)
					return true;

				var abstractObjectInfo = (AbstractObjectInfo)element;
				if (abstractObjectInfo != null && abstractObjectInfo.GetObject() != null &&
					abstractObjectInfo.GetObject().Equals(TheObject))
				{
					return true;
				}
			}
			return false;
		}
	}