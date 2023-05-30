using System.Linq.Expressions;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Furesoft.PrattParser;

namespace Furesoft.Core.Rules.DSL;

public class DslRule<T> : Rule<T>
    where T : class, new()
{
    private Func<T, bool> _evaluate;

    public DslRule(string source)
    {
        var tree = Grammar.Parse<Grammar>(source);

        var visitor = new EvaluationVisitor<T>();

        var evaluationResult = visitor.ToLambda(tree.Tree.Accept(visitor));

        _evaluate = (Func<T, bool>) evaluationResult.Compile();
    }

    public override IRuleResult Invoke()
    {
        try
        {
            var success = _evaluate(Model);

            if (success)
            {
                return new RuleResult() {Result = Model};
            }
        }
        catch (Exception ex)
        {
            return new RuleResult() {Error = new Error(ex.Message)};
        }

        return null;
    }
}