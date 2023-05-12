using System.Linq;
using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;
using Xunit;

namespace RulesTest;

public class TestParallelRule
{
    [Fact]
    public void TestParallelRules()
    {
        var product = new Product();
        var engineExecutor = RuleEngine<Product>.GetInstance(product);
        var ruleEngineExecutor = engineExecutor;

        ruleEngineExecutor.AddRules(
            new ProductParallelUpdateNameRuleAsync(),
            new ProductParallelUpdateDescriptionRuleAsync(),
            new ProductParallelUpdatePriceRuleAsync());

        var ruleResults = ruleEngineExecutor.ExecuteAsync().Result;

        Assert.NotNull(ruleResults);
        Assert.Equal("Product", product.Name);
        Assert.Equal(0.0m, product.Price);
        Assert.Equal("Description", product.Description);
    }

    [Fact]
    public void TestNestedParallelRules()
    {
        var product = new Product();
        var engineExecutor = RuleEngine<Product>.GetInstance(product);
        var ruleEngineExecutor = engineExecutor;

        ruleEngineExecutor.AddRules(
            new ProductNestedParallelUpdateA(),
            new ProductNestedParallelUpdateB(),
            new ProductNestedParallelUpdateC());

        var ruleResults = ruleEngineExecutor.ExecuteAsync().Result;
        Assert.Equal(8, ruleResults.Count());
    }
}