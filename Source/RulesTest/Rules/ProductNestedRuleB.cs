using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductNestedRuleB : Rule<Product>
{
    public override void Initialize()
    {
        Configuration.ExecutionOrder = 1;
    }
    public ProductNestedRuleB()
    {
        AddRules(new ProductNestedRuleC());
    }
    public override IRuleResult Invoke()
    {
        return null;
    }
}