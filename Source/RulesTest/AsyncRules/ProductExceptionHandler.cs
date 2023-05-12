using System;
using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

internal class ProductExceptionHandlerAsync : RuleAsync<Product>
{
    public override Task InitializeAsync()
    {
        IsExceptionHandler = true;
        ObserveRule<ProductExceptionThrownAsync>();

        return base.InitializeAsync();
    }

    public override Task<IRuleResult> InvokeAsync()
    {
        var ruleResult = new RuleResult();

        if (UnhandledException?.GetType() == typeof(Exception))
        {
            ruleResult.Error = new Error { Exception = UnhandledException };
        }

        return RuleResult.CreateAsync(ruleResult);
    }
}