using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

class ProductNestedRuleAsync : RuleAsync<Product>
{
    public ProductNestedRuleAsync()
    {
        AddRules(new ProductNestedRuleAsyncA(), new ProductNestedRuleAsyncB());
    }
    public override async Task<IRuleResult> InvokeAsync()
    {
        return await Task.FromResult<IRuleResult>(null);
    }
}