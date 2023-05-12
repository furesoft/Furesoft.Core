using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductNestedErrorRule : Rule<Product>
{
    public ProductNestedErrorRule()
    {
        AddRules(new ProductChildErrorRule(), new ProductNestedRuleA());
    }
    public override IRuleResult Invoke()
    {
        return null;
    }
}