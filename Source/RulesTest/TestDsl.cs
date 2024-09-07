using System.Runtime.CompilerServices;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.DSL;
using Silverfly;
using RulesTest.Models;
using Silverfly.Testing;

namespace RulesTest;

[TestFixture]
public class TestDsl : Silverfly.Testing.SnapshotParserTestBase<Grammar>
{
    [ModuleInitializer]
    public static void Initalize()
    {
        Init(new TestOptions("test.dsl"));
    }

    [Test]
    public Task Not_Should_Pass()
    {
        return Test("not 5");
    }

    [Test]
    public Task Percent_Should_Pass()
    {
        return Test("5 %");
    }

    [Test]
    public Task Comparison_Should_Pass()
    {
        return Test("5 is equal to 5");
    }

    [Test]
    public Task Divisible_Should_Pass()
    {
        var node = Parse("5 is divisible by 5");

        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var _evaluate = (Func<object, List<string>, bool>)evaluationResult.Compile();

        return Verify(new
        {
            tree = node,
            result = _evaluate(new { }, [])
        }, Settings);
    }

    [Test]
    public Task If_Should_Pass()
    {
        return Test("if 5 is equal to 5 then error 'something went wrong'");
    }

    [Test]
    public Task Set_Should_Pass()
    {
        return Test("set x to 42");
    }

    [Test]
    public Task Parse_Time_Seconds_Should_Pass()
    {
        return Test("set x to 12s");
    }

    [Test]
    public Task Time_Seconds_Should_Pass()
    {
        return Test("12.");
    }

    [Test]
    public Task Block_Multiple_Should_Pass()
    {
        return Test("12s.13min");
    }

    [Test]
    public Task TimeLiteral_Sequence_Should_Pass()
    {
        return Test("12min 13s");
    }

    [Test]
    public Task TimeLiteral_Evaluation_Sequence_Should_Pass()
    {
        var node = Parse("12min 13s.");
        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var _evaluate = (Func<object, List<string>, TimeSpan>)evaluationResult.Compile();

        return Verify(new
        {
            tree = node,
            result = _evaluate(new { }, [])
        }, Settings);
    }

    [Test]
    public Task Evaluate_Time_Seconds_Should_Pass()
    {
        var node = Parse("12s");
        var visitor = new EvaluationVisitor<object>();

        var evaluationResult = visitor.ToLambda(node.Tree.Accept(visitor));

        var evaluate = (Func<object, List<string>, TimeSpan>)evaluationResult.Compile();

        return Verify(new
        {
            tree = node,
            result = evaluate(new { }, [])
        }, Settings);
    }

    [Test]
    public Task SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance(new() { Description = "hello world", Price = 999 });

        engine.AddRule("if Description == 'hello world' then error 'wrong key'");

        var result = engine.Execute();

        return Verify(result, Settings);
    }
}