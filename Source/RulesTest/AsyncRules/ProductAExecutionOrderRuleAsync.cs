using System;
using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

public class ProductAExecutionOrderRuleAsync : RuleAsync<Product>
{
    public override Task InitializeAsync()
    {
        Configuration.ExecutionOrder = 2;

        return Task.FromResult<object>(null);
    }

    public override async Task<IRuleResult> InvokeAsync()
    {
        return await RuleResult.CreateAsync(new RuleResult { Result = DateTime.Now.Ticks });
    }
}