using System;
using System.Threading.Tasks;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.AsyncRules;

internal class ProductExceptionThrownAsync : RuleAsync<Product>
{
    public override Task<IRuleResult> InvokeAsync()
    {
        throw new Exception();
    }
}