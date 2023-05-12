using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductConstraintA : Rule<Product>
{
    public override void Initialize()
    {
        Configuration.Constraint = product => Model.Description == "Description";
    }

    public override IRuleResult Invoke()
    {
        Model.Description = "Product Description";
        return new RuleResult { Name = "ProductRule", Result = Model.Description };
    }
}