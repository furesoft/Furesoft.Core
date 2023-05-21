using System.Linq.Expressions;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Furesoft.PrattParser;

namespace Furesoft.Core.Rules.DSL;

public class DslRule<T> : Rule<T>
    where T : class, new()
{
    private Func<T, IRuleResult> _evaluate;
    public DslRule(string source)
    {
        var tree = Grammar.Parse<Grammar>(source);
        
        var visitor = new EvaluationVisitor<T>();
        
        var evaluationResult = (LambdaExpression)tree.Tree.Accept(visitor);

        _evaluate = (Func<T, RuleResult>)evaluationResult.Compile();
    }

    public override IRuleResult Invoke()
    {
        return _evaluate(Model);
    }
}