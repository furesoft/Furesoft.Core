using System.Runtime.CompilerServices;
using Argon;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.DSL;
using Furesoft.PrattParser.Testing;
using RulesTest.Models;

namespace RulesTest;

[UsesVerify]
public class TestDsl
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.AddExtraSettings(_ =>
        {
            _.Converters.Add(new SymbolConverter());
            _.Converters.Add(new DocumentConverter());
            _.Converters.Add(new RangeConverter());
            _.TypeNameHandling = TypeNameHandling.All;
        });
    }

    [Fact]
    public Task Not_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("not 5", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task Percent_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("5%", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task Comparison_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("5 is equal to 5", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task If_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("if 5 is equal to 5 then error 'something went wrong'", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task Set_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("set x to 42.", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task Parse_Time_Seconds_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("set x to 12s.", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task Time_Seconds_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("12s.", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task Add_Time_Seconds_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("1m + 12s", "test.dsl");
        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var _evaluate = (Func<object, List<string>, TimeSpan>) evaluationResult.Compile();

        return Verify(_evaluate(new { }, new()));
    }

    [Fact]
    public Task Evaluate_Time_Seconds_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("12s", "test.dsl");
        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var _evaluate = (Func<object, List<string>, TimeSpan>) evaluationResult.Compile();

        return Verify(_evaluate(new { }, new()));
    }

    [Fact]
    public Task SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance(new() {Description = "hello world", Price = 999});

        engine.AddRule("if Description == 'hello world' then error 'wrong key'");

        var result = engine.Execute();

        return Verify(result);
    }
}