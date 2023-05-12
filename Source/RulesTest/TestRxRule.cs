using Furesoft.Core.Rules;
using RulesTest.AsyncRules;
using RulesTest.Models;
using RulesTest.Rules;
using Xunit;

namespace RulesTest;

public class TestRxRule
{
    [Fact]
    public void TestReactiveRules()
    {
        var product = new Product();
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
        ruleEngineExecutor.AddRules(new ProductRule(), new ProductReactiveRule());
        var rr = ruleEngineExecutor.Execute();
        Assert.True(rr.FindRuleResult<ProductReactiveRule>().Data["Ticks"].To<long>() >= rr.FindRuleResult<ProductRule>().Data["Ticks"].To<long>(),
            $"expected {rr.FindRuleResult<ProductReactiveRule>().Data["Ticks"]} actual {rr.FindRuleResult<ProductRule>().Data["Ticks"]}");
    }

    [Fact]
    public void TestProactiveRules()
    {
        var product = new Product();
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
        ruleEngineExecutor.AddRules(new ProductRule(), new ProductProactiveRule());
        var rr = ruleEngineExecutor.Execute();
        Assert.True(rr.FindRuleResult<ProductProactiveRule>().Data["Ticks"].To<long>() <= rr.FindRuleResult<ProductRule>().Data["Ticks"].To<long>(),
            $"expected {rr.FindRuleResult<ProductProactiveRule>().Data["Ticks"]} actual {rr.FindRuleResult<ProductRule>().Data["Ticks"]}");
    }

    [Fact]
    public void TestExceptionHandler()
    {
        var product = new Product();
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
        ruleEngineExecutor.AddRules(new ProductExceptionHandler(), new ProductExceptionThrown());
        var rr = ruleEngineExecutor.Execute();
        Assert.NotNull(rr.FindRuleResult<ProductExceptionHandler>().Error.Exception);
    }        

    [Fact]
    public void TestExceptionHandlerAsync()
    {
        var product = new Product();
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
        ruleEngineExecutor.AddRules(new ProductExceptionHandlerAsync(), new ProductExceptionThrownAsync());
        var rr = ruleEngineExecutor.ExecuteAsync().Result;
        Assert.NotNull(rr.FindRuleResult<ProductExceptionHandlerAsync>().Error.Exception);
    }

    [Fact]
    public void TestGlobalExceptionHandler()
    {
        var product = new Product();
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
        ruleEngineExecutor.AddRules(new ProductGlobalExceptionHandler(), new ProductExceptionThrown());
        var rr = ruleEngineExecutor.Execute();
        Assert.NotNull(rr.FindRuleResult<ProductGlobalExceptionHandler>().Error.Exception);
    }

    [Fact]
    public void TestGlobalExceptionHandlerAsync()
    {
        var product = new Product();
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
        ruleEngineExecutor.AddRules(new ProductGlobalExceptionHandlerAsync(), new ProductExceptionThrownAsync());
        var rr = ruleEngineExecutor.ExecuteAsync().Result;
        Assert.NotNull(rr.FindRuleResult<ProductGlobalExceptionHandlerAsync>().Error.Exception);
    }
}