using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;

namespace RulesTest;

public class TestParallelRule
{
    [Fact]
    public async void TestParallelRules()
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
        Assert.Equal("Product", product.Name);
        Assert.Equal(0.0m, product.Price);
        Assert.Equal("Description", product.Description);
    }

    [Fact]
    public async void TestNestedParallelRules()
    {
        var product = new Product();
        var engineExecutor = RuleEngine<Product>.GetInstance(product);
        var ruleEngineExecutor = engineExecutor;

        ruleEngineExecutor.AddRules(
            new ProductNestedParallelUpdateA(),
            new ProductNestedParallelUpdateB(),
            new ProductNestedParallelUpdateC());

        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.Equal(8, ruleResults.Count());
    }
}