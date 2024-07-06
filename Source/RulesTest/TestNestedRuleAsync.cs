using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;

namespace RulesTest;

public class TestNestedRuleAsync
{
    [Test]
    public async Task TestAsyncNestedRules()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());

        ruleEngineExecutor.AddRules(new ProductNestedRuleAsync());

        var ruleResults = await ruleEngineExecutor.ExecuteAsync();

        Assert.NotNull(ruleResults);
        Assert.AreEqual("ProductNestedRuleAsyncC", ruleResults.FindRuleResult<ProductNestedRuleAsyncC>().Name);
    }
}