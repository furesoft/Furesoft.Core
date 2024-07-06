using System;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.Rules;
using RulesTest.Models;
using RulesTest.Rules;

namespace RulesTest;

[TestFixture]
public class TestRule
{
    [Test]
    public void TestInvoke()
    {
        var ruleResults = RuleEngine<Product>.GetInstance(new Product())
            .ApplyRules(new ProductRule())
            .Execute();

        Assert.AreEqual("Product Description", ruleResults.FindRuleResult<ProductRule>().Result);
    }

    [Test]
    public void TestBeforeInvoke()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        var ruleResults = ruleEngineExecutor.ApplyRules(new ProductRule())
            .Execute();

        object value;
        ruleResults.FindRuleResult("ProductRule").Data.TryGetValue("Key", out value);
        Assert.AreEqual("Value", value);
    }

    [Test]
    public void TestAfterInvoke()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTerminateA(), new ProductTerminateB());
        var ruleResults = ruleEngineExecutor.Execute();
        var ruleResult = ruleResults.FindRuleResult("ProductRule");
        Assert.NotNull(ruleResult);
        Assert.IsTrue(ruleResults.Any());
    }

    [Test]
    public void TestSkip()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductSkip());
        var ruleResults = ruleEngineExecutor.Execute();
        Assert.False(ruleResults.Any());
    }

    [Test]
    public void TestTerminate()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTerminateA(), new ProductTerminateB());
        var ruleResults = ruleEngineExecutor.Execute();
        Assert.IsTrue(ruleResults.Any());
    }

    [Test]
    public void TestConstraint()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductConstraintA(), new ProductConstraintB());
        var ruleResults = ruleEngineExecutor.Execute();
        Assert.IsTrue(ruleResults.Any());
    }

    [Test]
    public void TestTryAddTryGetValue()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductTryAdd(), new ProductTryGetValue());
        var ruleResults = ruleEngineExecutor.Execute().FindRuleResult("ProductRule").Result.To<List<string>>();

        Assert.AreEqual("Product Description1", ruleResults[0]);
        Assert.AreEqual("Product Description2", ruleResults[1]);
        Assert.AreEqual("Product Description3", ruleResults[2]);
        Assert.AreEqual("Product Description4", ruleResults[3]);
    }

    [Test]
    public void TestExecutionOrder()
    {
        var ruleResults = RuleEngine<Product>.GetInstance(new Product())
            .ApplyRules(new ProductExecutionOrderRuleA(), new ProductExecutionOrderRuleB())
            .Execute();

        Assert.AreEqual("ProductExecutionOrderRuleB", ruleResults.First().Name);
        Assert.AreEqual("ProductExecutionOrderRuleA", ruleResults.Skip(1).First().Name);
    }

    [Test]
    public void TestErrorResult()
    {
        var errors = RuleEngine<Product>.GetInstance(new Product())
            .ApplyRules(new ProductRule(), new ProductRuleError())
            .Execute()
            .GetErrors();

        Assert.AreEqual("Error", errors.FindRuleResult<ProductRuleError>().Error.Message);
        Assert.AreEqual(typeof(Exception), errors.FindRuleResult<ProductRuleError>().Error.Exception.GetType());
    }
}