using System.Runtime.CompilerServices;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.DSL;
using Silverfly;
using RulesTest.Models;

namespace RulesTest;

public class TestDsl : Silverfly.Testing.SnapshotParserTestBase
{
    [ModuleInitializer]
    public static void Initalize()
    {
        Init();
    }

    [Fact]
    public Task Not_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("not 5", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task Percent_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("5 %", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task Comparison_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("5 is equal to 5", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task Divisible_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("5 is divisible by 5", "test.dsl");

        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var _evaluate = (Func<object, List<string>, bool>)evaluationResult.Compile();

        return Verify(new
        {
            tree = node,
            result = _evaluate(new { }, [])
        }, settings);
    }

    [Fact]
    public Task If_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("if 5 is equal to 5 then error 'something went wrong'", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task Set_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("set x to 42.", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task Parse_Time_Seconds_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("set x to 12s.", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task Time_Seconds_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("12s.", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task Block_Multiple_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("12s.13min.", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task TimeLiteral_Sequence_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("12min 13s.", "test.dsl");

        return Verify(node, settings);
    }

    [Fact]
    public Task TimeLiteral_Evaluation_Sequence_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("12min 13s.", "test.dsl");
        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var _evaluate = (Func<object, List<string>, TimeSpan>)evaluationResult.Compile();

        return Verify(new
        {
            tree = node,
            result = _evaluate(new { }, [])
        }, settings);
    }

    [Fact]
    public Task Evaluate_Time_Seconds_Should_Pass()
    {
        var node = Parser.Parse<Grammar>("12s", "test.dsl");
        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var _evaluate = (Func<object, List<string>, TimeSpan>)evaluationResult.Compile();

        return Verify(new
        {
            tree = node,
            result = _evaluate(new { }, [])
        }, settings);
    }

    [Fact]
    public Task SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance(new() { Description = "hello world", Price = 999 });

        engine.AddRule("if Description == 'hello world' then error 'wrong key'");

        var result = engine.Execute();

        return Verify(result, settings);
    }
}