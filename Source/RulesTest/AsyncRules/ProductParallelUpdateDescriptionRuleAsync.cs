using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

class ProductParallelUpdateDescriptionRuleAsync : RuleAsync<Product>
{
    public override Task InitializeAsync()
    {
        IsParallel = true;

        return Task.FromResult<object>(null);
    }

    public override async Task<IRuleResult> InvokeAsync()
    {
        await Task.Delay(10);
        Model.Description = "Description";

        return await Task.FromResult<IRuleResult>(null);
    }
}