using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Silverfly;

namespace Furesoft.Core.Rules.DSL;

public class DslRule<T> : Rule<T>
    where T : class, new()
{
    private readonly List<string> _errors = [];
    private readonly Func<T, List<string>, bool> _evaluate;

    public DslRule(string source)
    {
        var tree = new Grammar().Parse(source);

        var visitor = new EvaluationVisitor<T>();

        var evaluationResult = visitor.ToLambda(tree.Tree.Accept(visitor));

        _evaluate = (Func<T, List<string>, bool>)evaluationResult.Compile();
    }

    public override IRuleResult Invoke()
    {
        var success = _evaluate(Model, _errors);

        if (success) return new RuleResult { Result = Model };

        return new RuleResult { Error = new Error(string.Join(Environment.NewLine, _errors)) };
    }
}