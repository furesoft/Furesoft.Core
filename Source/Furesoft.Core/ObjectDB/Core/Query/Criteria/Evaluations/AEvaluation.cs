using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query.Criteria.Evaluations;

	internal abstract class AEvaluation : IEvaluation
	{
		protected readonly string AttributeName;
		protected readonly object TheObject;

		protected AEvaluation(object theObject, string attributeName)
		{
			TheObject = theObject;
			AttributeName = attributeName;
		}

		#region IEvaluation Members

		public abstract bool Evaluate(object candidate);

		#endregion IEvaluation Members

		protected bool IsNative()
		{
			return TheObject == null || OdbType.IsNative(TheObject.GetType());
		}

		protected object AsAttributeValuesMapValue(object valueToMatch)
		{
			// If it is a AttributeValuesMap, then gets the real value from the map

			return valueToMatch is AttributeValuesMap attributeValues
					   ? attributeValues[AttributeName]
					   : valueToMatch;
		}
	}