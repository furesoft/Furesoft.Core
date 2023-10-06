using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;

namespace RulesTest;

public class TestNestedRuleAsync
{
    [Fact]
    public async void TestAsyncNestedRules()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());

        ruleEngineExecutor.AddRules(new ProductNestedRuleAsync());

        var ruleResults = await ruleEngineExecutor.ExecuteAsync();

        Assert.NotNull(ruleResults);
        Assert.Equal("ProductNestedRuleAsyncC", ruleResults.FindRuleResult<ProductNestedRuleAsyncC>().Name);
    }
}