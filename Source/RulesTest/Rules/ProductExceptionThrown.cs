using System;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using RulesTest.Models;

namespace RulesTest.Rules;

internal class ProductExceptionThrown : Rule<Product>
{
    public override IRuleResult Invoke()
    {
        throw new Exception();
    }
}