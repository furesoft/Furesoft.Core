using Furesoft.Core.ObjectDB.Api.Query;
using Furesoft.Core.ObjectDB.Meta;
using Furesoft.Core.ObjectDB.Tool.Wrappers;

namespace Furesoft.Core.ObjectDB.Core.Query.Criteria;

	internal interface IInternalConstraint : IConstraint
	{
		bool CanUseIndex();

		AttributeValuesMap GetValues();

		/// <summary>
		///   to be able to optimize query execution.
		/// </summary>
		/// <remarks>
		///   to be able to optimize query execution.
		///   Get only the field involved in the query instead of getting all the object
		/// </remarks>
		/// <returns> All involved fields in criteria, List of String </returns>
		IOdbList<string> GetAllInvolvedFields();

		/// <summary>
		///   To check if an object matches this criterion
		/// </summary>
		/// <returns> true if object matches the criteria </returns>
		bool Match(object @object);
	}