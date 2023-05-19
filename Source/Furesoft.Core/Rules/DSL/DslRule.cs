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
        var lexer = new Lexer(source);
        lexer.Ignore(' ');
        lexer.Ignore('\t');

        var parser = new Grammar(lexer);
        var tree = parser.Parse();
        var visitor = new EvaluationVisitor<T>();
        
        var evaluationResult = (LambdaExpression)tree.Accept(visitor);

        _evaluate = (Func<T, RuleResult>)evaluationResult.Compile();
    }

    public override IRuleResult Invoke()
    {
        return _evaluate(Model);
    }
}