using System;
using Furesoft.Core.ObjectDB.Exceptions;

namespace Furesoft.Core.ObjectDB.Core.Query.Criteria.Evaluations;

	internal sealed class EndsWithEvaluation : AEvaluation
	{
		private readonly bool _isCaseSensitive;

		public EndsWithEvaluation(object theObject, string attributeName, bool isCaseSensitive)
			: base(theObject, attributeName)
		{
			_isCaseSensitive = isCaseSensitive;
		}

		public override bool Evaluate(object candidate)
		{
			candidate = AsAttributeValuesMapValue(candidate);

			if (candidate == null && TheObject == null)
				return true;

			if (candidate == null)
				return false;

			return candidate is string candidateAsString && CheckIfStringEndsWithValue(candidateAsString);
		}

		private bool CheckIfStringEndsWithValue(string candidateAsString)
		{
			if (TheObject is string theObjectAsString)
			{
				return candidateAsString.EndsWith(theObjectAsString,
												  _isCaseSensitive
													  ? StringComparison.Ordinal
													  : StringComparison.OrdinalIgnoreCase);
			}

			throw new OdbRuntimeException(
				NDatabaseError.QueryEndsWithConstraintTypeNotSupported.AddParameter(TheObject.GetType().FullName));
		}
	}