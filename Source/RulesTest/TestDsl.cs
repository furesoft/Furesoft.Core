using Furesoft.Core.Rules;
using Furesoft.Core.Rules.DSL;
using RulesTest.Models;

namespace RulesTest;

[UsesVerify]
public class TestDsl
{
    [Fact]
    public Task Not_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("not 5", "test.dsl");
        
        return Verify(node);
    }
    
    [Fact]
    public Task Comparison_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("5 is equal to 5", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task If_Should_Pass() {
        var node = Grammar.Parse<Grammar>("if 5 is equal to 5 then error 'something went wrong'", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance(new Product() { Description = "hello world", Price = 999});
        
        engine.AddRule("if Description == 'hello world' then error 'wrong key'");

        var result = engine.Execute();
        
        return Verify(result);
    }
}