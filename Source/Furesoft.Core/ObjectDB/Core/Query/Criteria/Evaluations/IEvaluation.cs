namespace Furesoft.Core.ObjectDB.Core.Query.Criteria.Evaluations
{
	internal interface IEvaluation
	{
		bool Evaluate(object candidate);
	}
}