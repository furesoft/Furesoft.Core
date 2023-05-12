using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

internal class ProductExecutionOrderRuleB : Rule<Product>
{
    public override void Initialize()
    {
        Configuration.ExecutionOrder = 1;
    }

    public override IRuleResult Invoke()
    {
        return new RuleResult();
    }
}