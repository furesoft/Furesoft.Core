using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;

namespace RulesTest;

public class TestRuleAsync
{
    [Fact]
    public async void TestInvokeAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductRuleAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.Equal("Product Description", ruleResults.First().Result);
    }

    [Fact]
    public async void TestBeforeInvokeAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductRuleAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();

        object value;
        ruleResults.First().Data.TryGetValue("Description", out value);
        Assert.Equal("Description", value);
    }

    [Fact]
    public async void TestAfterInvokeAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTerminateAsyncA(), new ProductTerminateAsyncB());
        var ruleResults =await ruleEngineExecutor.ExecuteAsync();
        Assert.Single(ruleResults);
    }

    [Fact]
    public async void TestSkipAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductSkipAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.False(ruleResults.Any());
    }

    [Fact]
    public async void TestTerminateAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTerminateAsyncA(), new ProductTerminateAsyncB());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.Single(ruleResults);
    }

    [Fact]
    public async void TestConstraintAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductConstraintAsyncA(), new ProductConstraintAsyncB());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.Single(ruleResults);
    }

    [Fact]
    public async void TestTryAddTryGetValueAsync()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTryAddAsync(), new ProductTryGetValueAsync());
        var ruleResults = await ruleEngineExecutor.ExecuteAsync();
        Assert.Equal("Product Description", ruleResults.First().Result);
    }

    [Fact]
    public async void TestExecutionOrder()
    {
        var ruleResults = await RuleEngine<Product>.GetInstance(new Product())
            .ApplyRules(new ProductAExecutionOrderRuleAsync(), new ProductBExecutionOrderRuleAsync())
            .ExecuteAsync();

        var productBExecutionOrderRuleAsync = ruleResults.FindRuleResult<ProductBExecutionOrderRuleAsync>();
        var productAExecutionOrderRuleAsync = ruleResults.FindRuleResult<ProductAExecutionOrderRuleAsync>();

        Assert.True(productBExecutionOrderRuleAsync.Result.To<long>() <= productAExecutionOrderRuleAsync.Result.To<long>());
    }
}