using Furesoft.Core.Rules;
using Furesoft.Core.Rules.DSL;
using Furesoft.PrattParser;
using RulesTest.Models;
using Xunit;

namespace RulesTest;

public class TestDsl
{
    [Fact]
    public void Not_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("not 5", "test.dsl");
    }
    
    [Fact]
    public void Comparison_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("5 is equal to 5", "test.dsl");
    }

    [Fact]
    public void SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance();
        
        engine.AddRule("1 + 1");

        var result = engine.Execute();
    }
}