using System;
using Furesoft.Core.ObjectDB.Exceptions;

namespace Furesoft.Core.ObjectDB.Core.Query.Criteria.Evaluations;

	internal sealed class StartsWithEvaluation : AEvaluation
	{
		private readonly bool _isCaseSensitive;

		public StartsWithEvaluation(object theObject, string attributeName, bool isCaseSensitive)
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

			return candidate is string candidateAsString && CheckIfStringStartsWithValue(candidateAsString);
		}

		private bool CheckIfStringStartsWithValue(string candidateAsString)
		{
			if (TheObject is string theObjectAsString)
			{
				return candidateAsString.StartsWith(theObjectAsString,
													_isCaseSensitive
														? StringComparison.Ordinal
														: StringComparison.OrdinalIgnoreCase);
			}

			throw new OdbRuntimeException(
				NDatabaseError.QueryStartsWithConstraintTypeNotSupported.AddParameter(TheObject.GetType().FullName));
		}
	}