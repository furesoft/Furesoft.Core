using System;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Furesoft.Core.Rules.Models;
using RulesTest.Models;

namespace RulesTest.Rules;

class ProductRuleError : Rule<Product>
{
    public override IRuleResult Invoke()
    {
        Model.Description = "Product Description";

        return new RuleResult { Error = new Error { Message = "Error", Exception = new Exception() } };
    }
}