using Furesoft.Core.Rules;
using Furesoft.Core.Rules.Interfaces;
using Product = RulesTest.Models.Product;

namespace RulesTest.Rules;

class ProductTryAdd : Rule<Product>
{
    public override void Initialize()
    {
        TryAdd("Description1", "Product Description1");
    }

    public override void BeforeInvoke()
    {
        TryAdd("Description2", "Product Description2");
    }

    public override IRuleResult Invoke()
    {
        TryAdd("Description3", "Product Description3");
        return null;
    }

    public override void AfterInvoke()
    {
        TryAdd("Description4", "Product Description4");
    }
}