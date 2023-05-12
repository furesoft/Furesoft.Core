using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using Product = RulesTest.Models.Product;

namespace RulesTest.Rules;

class ProductTerminateB : Rule<Product>
{
    public override IRuleResult Invoke()
    {
        Model.Description = "Product Description";
        return new RuleResult { Name = "ProductRule", Result = Model.Description };
    }
}