namespace Furesoft.Core.ExpressionEvaluator;

public interface IBindable
{
    CodeObject Bind(ExpressionParser ep, Binder binder);
}
