using System;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

internal class ProductExceptionHandler : Rule<Product>
{
    public override void Initialize()
    {
        IsExceptionHandler = true;
        ObserveRule<ProductExceptionThrown>();
    }

    public override IRuleResult Invoke()
    {
        var ruleResult = new RuleResult();

        if (UnhandledException?.GetType() == typeof(Exception))
        {
            ruleResult.Error = new Error { Exception = UnhandledException };
        }

        return ruleResult;
    }
}