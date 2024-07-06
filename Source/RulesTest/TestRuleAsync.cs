using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;

namespace RulesTest;

public class TestRuleAsync
{
    [Test]
    public async Task TestInvokeAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductRuleAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.AreEqual("Product Description", ruleResults.First().Result);
    }

    [Test]
    public async Task TestBeforeInvokeAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductRuleAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();

        object value;
        ruleResults.First().Data.TryGetValue("Description", out value);
        Assert.AreEqual("Description", value);
    }

    [Test]
    public async Task TestAfterInvokeAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTerminateAsyncA(), new ProductTerminateAsyncB());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.IsTrue(ruleResults.Any());
    }

    [Test]
    public async Task TestSkipAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductSkipAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.False(ruleResults.Any());
    }

    [Test]
    public async Task TestTerminateAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTerminateAsyncA(), new ProductTerminateAsyncB());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.IsTrue(ruleResults.Any());
    }

    [Test]
    public async Task TestConstraintAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductConstraintAsyncA(), new ProductConstraintAsyncB());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.IsTrue(ruleResults.Any());
    }

    [Test]
    public async Task TestTryAddTryGetValueAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTryAddAsync(), new ProductTryGetValueAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.AreEqual("Product Description", ruleResults.First().Result);
    }

    [Test]
    public async Task TestExecutionOrder()
    {
        var ruleResults = await RuleEngine<Product>.GetInstance(new Product())
            .ApplyRules(new ProductAExecutionOrderRuleAsync(), new ProductBExecutionOrderRuleAsync())
            .ExecuteAsync();

        var productBExecutionOrderRuleAsync = ruleResults.FindRuleResult<ProductBExecutionOrderRuleAsync>();
        var productAExecutionOrderRuleAsync = ruleResults.FindRuleResult<ProductAExecutionOrderRuleAsync>();

        Assert.True(productBExecutionOrderRuleAsync.Result.To<long>() <= productAExecutionOrderRuleAsync.Result.To<long>());
    }
}