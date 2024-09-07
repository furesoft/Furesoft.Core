using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;

namespace RulesTest;

[TestFixture]
public class TestNestedRuleAsync
{
    [Test]
    public async void TestAsyncNestedRules()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());

        ruleEngineExecutor.AddRules(new ProductNestedRuleAsync());

        var ruleResults = await ruleEngineExecutor.ExecuteAsync();

        Assert.NotNull(ruleResults);
        Assert.Equal("ProductNestedRuleAsyncC", ruleResults.FindRuleResult<ProductNestedRuleAsyncC>().Name);
    }
}