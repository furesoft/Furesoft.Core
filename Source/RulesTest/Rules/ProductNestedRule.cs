using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductNestedRule : Rule<Product>
{
    public override void Initialize()
    {
        AddRules(new ProductNestedRuleA(), new ProductNestedRuleB());
    }

    public override IRuleResult Invoke()
    {
        return null;
    }
}