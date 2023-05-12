using System.Diagnostics;
using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

class ProductParallelUpdatePriceRuleAsync : RuleAsync<Product>
{
    public override Task InitializeAsync()
    {
        IsParallel = true;

        return Task.FromResult<object>(null);
    }

    public override async Task<IRuleResult> InvokeAsync()
    {
        await Task.Delay(5);
        Model.Price = 0.0m;
        Debug.WriteLine("ProductParallelUpdatePriceRuleAsync");

        return await Task.FromResult<IRuleResult>(null);
    }
}