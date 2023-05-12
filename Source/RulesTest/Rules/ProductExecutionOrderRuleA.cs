using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductExecutionOrderRuleA : Rule<Product>
{
    public override void Initialize()
    {
        Configuration.ExecutionOrder = 2;
    }

    public override IRuleResult Invoke()
    {
        return new RuleResult();
    }
}