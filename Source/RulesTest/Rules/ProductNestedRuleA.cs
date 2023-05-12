using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductNestedRuleA : Rule<Product>
{
    public override void Initialize()
    {
        Configuration.ExecutionOrder = 2;
    }

    public override IRuleResult Invoke()
    {
        Model.Description = "Product Description";

        return new RuleResult { Name = "ProductNestedRuleA", Result = Model.Description };
    }
}