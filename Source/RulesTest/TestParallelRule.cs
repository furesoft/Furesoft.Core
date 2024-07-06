using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;

namespace RulesTest;

public class TestParallelRule
{
    [Test]
    public async Task TestParallelRules()
    {
        var product = new Product();
        var engineExecutor = RuleEngine<Product>.GetInstance(product);
        var ruleEngineExecutor = engineExecutor;

        ruleEngineExecutor.AddRules(
            new ProductParallelUpdateNameRuleAsync(),
            new ProductParallelUpdateDescriptionRuleAsync(),
            new ProductParallelUpdatePriceRuleAsync());

        var ruleResults = await ruleEngineExecutor.ExecuteAsync();

        Assert.NotNull(ruleResults);
        Assert.AreEqual("Product", product.Name);
        Assert.AreEqual(0.0m, product.Price);
        Assert.AreEqual("Description", product.Description);
    }

    [Test]
    public async Task TestNestedParallelRules()
    {
        var product = new Product();
        var engineExecutor = RuleEngine<Product>.GetInstance(product);
        var ruleEngineExecutor = engineExecutor;

        ruleEngineExecutor.AddRules(
            new ProductNestedParallelUpdateA(),
            new ProductNestedParallelUpdateB(),
            new ProductNestedParallelUpdateC());

        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.AreEqual(8, ruleResults.Count());
    }
}