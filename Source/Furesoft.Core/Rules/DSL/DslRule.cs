using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;

namespace Furesoft.Core.Rules.DSL;

public class DslRule<T> : Rule<T>
    where T : class, new()
{
    private readonly Func<T, List<string>, bool> _evaluate;
    private readonly List<string> _errors = new();

    public DslRule(string source)
    {
        var tree = Grammar.Parse<Grammar>(source);

        var visitor = new EvaluationVisitor<T>();

        var evaluationResult = visitor.ToLambda(tree.Tree.Accept(visitor));

        _evaluate = (Func<T, List<string>, bool>) evaluationResult.Compile();
    }

    public override IRuleResult Invoke()
    {
        var success = _evaluate(Model, _errors);

        if (success)
        {
            return new RuleResult() {Result = Model};
        }

        return new RuleResult(){ Error = new Error(string.Join(Environment.NewLine, _errors))};
    }
}