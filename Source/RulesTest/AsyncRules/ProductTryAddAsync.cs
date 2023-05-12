using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

class ProductTryAddAsync : RuleAsync<Product>
{        
    public override async Task<IRuleResult> InvokeAsync()
    {
        await TryAddAsync("Description", Task.FromResult<object>("Product Description"));
        return await Task.FromResult<IRuleResult>(null);
    }
}