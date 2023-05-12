using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;
using Xunit;

namespace RulesTest;

public class TestNestedRuleAsync
{
    [Fact]
    public void TestAsyncNestedRules()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());

        ruleEngineExecutor.AddRules(new ProductNestedRuleAsync());

        var ruleResults = ruleEngineExecutor.ExecuteAsync().Result;

        Assert.NotNull(ruleResults);
        Assert.Equal("ProductNestedRuleAsyncC", ruleResults.FindRuleResult<ProductNestedRuleAsyncC>().Name);
    }
}