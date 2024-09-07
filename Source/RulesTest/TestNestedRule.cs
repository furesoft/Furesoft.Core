using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;
using RulesTest.Rules;
using Assert = Xunit.Assert;

namespace RulesTest;

[TestFixture]
public class TestNestedRule
{
    [Test]
    public void TestNestedRules()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductNestedRule());
        var ruleResults = ruleEngineExecutor.Execute();
        var nestedRuleResult = ruleResults.FindRuleResult<ProductNestedRuleC>();

        Assert.NotNull(nestedRuleResult);
        Assert.AreEqual("ProductNestedRuleC", nestedRuleResult.Name);
    }

    [Test]
    public void TestNestedRuleError()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());
        ruleEngineExecutor.AddRules(new ProductNestedErrorRule());
        var ruleResults = ruleEngineExecutor.Execute();
        var errors = ruleResults.GetErrors();

        Assert.NotNull(errors);
        Assert.AreEqual("Error", errors.FindRuleResult<ProductChildErrorRule>().Error.Message);
    }

    [Test]
    public void TestNestedRuleInheritsConstraint()
    {
        var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product { Price = 49.99m });
        ruleEngineExecutor.AddRules(new ProductNestedRule
        {

            Configuration = new Configuration<Product>
            {
                Constraint = product => product.Price > 50,
                NestedRulesInheritConstraint = true
            }
        });

        var ruleResults = ruleEngineExecutor.Execute();

        Assert.IsEmpty(ruleResults);
    }
}