using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductChildErrorRule : Rule<Product>
{
    public override IRuleResult Invoke()
    {
        Model.Description = "Product Description";

        return new RuleResult { Result = Model.Description, Error = new Error("Error") };
    }
}