using System;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

internal class ProductReactiveRule : Rule<Product>
{
    public override void Initialize()
    {
        IsReactive = true;
        ObserveRule<ProductRule>();
    }

    public override IRuleResult Invoke()
    {
        TryAdd("Ticks", DateTime.Now.Ticks);
        return new RuleResult { Result = Model.Description, Data = { { "Ticks", TryGetValue("Ticks") } } };
    }
}