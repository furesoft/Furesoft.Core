using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

class ProductConstraintAsyncB : RuleAsync<Product>
{
    public override Task BeforeInvokeAsync()
    {
        Configuration.Constraint = product => Model.Description == "";
        return Task.FromResult<object>(null);
    }

    public override async Task<IRuleResult> InvokeAsync()
    {
        Model.Description = "Product Description";
        return await Task.FromResult(new RuleResult { Name = "ProductRule", Result = Model.Description });
    }
}