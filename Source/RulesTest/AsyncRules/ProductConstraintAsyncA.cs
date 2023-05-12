using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

class ProductConstraintAsyncA : RuleAsync<Product>
{
    public override Task InitializeAsync()
    {
        Configuration.Constraint = product => product.Description == "Description";

        return Task.FromResult<object>(null);
    }

    public override async Task<IRuleResult> InvokeAsync()
    {
        Model.Description = "Product Description";
        return await RuleResult.CreateAsync(new RuleResult { Name = "ProductRule", Result = Model.Description });
    }        
}