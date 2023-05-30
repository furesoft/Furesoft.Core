using System;
using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

internal class ProductGlobalExceptionHandler : Rule<Product>
{
    public override void Initialize()
    {
        IsGlobalExceptionHandler = true;
    }

    public override IRuleResult Invoke()
    {
        var ruleResult = new RuleResult();

        if (UnhandledException?.GetType() == typeof(Exception))
        {
            ruleResult.Error = new Error(UnhandledException);
        }

        return ruleResult;
    }
}

internal class ProductGlobalExceptionHandlerAsync : RuleAsync<Product>
{
    public override Task InitializeAsync()
    {
        IsGlobalExceptionHandler = true;
        return base.InitializeAsync();
    }

    public override Task<IRuleResult> InvokeAsync()
    {
        var ruleResult = new RuleResult();

        if (UnhandledException?.GetType() == typeof(Exception))
        {
            ruleResult.Error = new Error(UnhandledException);
        }

        return RuleResult.CreateAsync(ruleResult);
    }
}