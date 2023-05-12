using System;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

internal class ProductRule : Rule<Product>
{
    public override void BeforeInvoke()
    {
        TryAdd("Key", "Value");
    }

    public override IRuleResult Invoke()
    {
        Model.Description = "Product Description";
        TryAdd("Ticks", DateTime.Now.Ticks);
        return new RuleResult { Name = "ProductRule", Result = Model.Description, Data = { { "Key", TryGetValue("Key") }, { "Ticks", TryGetValue("Ticks") } } };
    }
}