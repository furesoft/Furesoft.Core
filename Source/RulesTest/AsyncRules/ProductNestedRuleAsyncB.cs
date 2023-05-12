using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

class ProductNestedRuleAsyncB : RuleAsync<Product>
{
    public ProductNestedRuleAsyncB()
    {
        AddRules(new ProductNestedRuleAsyncC());
    }
    public override async Task<IRuleResult> InvokeAsync()
    {
        return await Task.FromResult<IRuleResult>(null);
    }
}