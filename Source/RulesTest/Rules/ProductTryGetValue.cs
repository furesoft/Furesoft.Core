using System.Collections.Generic;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Product = RulesTest.Models.Product;

namespace RulesTest.Rules;

class ProductTryGetValue : Rule<Product>
{
    public override IRuleResult Invoke()
    {
        var descriptionList = new List<string>
        {
            TryGetValue("Description1").To<string>(),
            TryGetValue("Description2").To<string>(),
            TryGetValue("Description3").To<string>(),
            TryGetValue("Description4").To<string>()
        };

        return new RuleResult { Name = "ProductRule", Result = descriptionList };
    }
}