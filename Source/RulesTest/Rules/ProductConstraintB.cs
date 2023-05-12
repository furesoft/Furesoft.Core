using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductConstraintB : Rule<Product>
{
    public override void BeforeInvoke()
    {
        Configuration.Constraint = product => Model.Description == "";
    }

    public override IRuleResult Invoke()
    {
        Model.Description = "Product Description";
        return new RuleResult { Name = "ProductRule", Result = Model.Description };
    }
}