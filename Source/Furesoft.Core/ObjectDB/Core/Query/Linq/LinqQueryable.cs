using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Furesoft.Core.ObjectDB.Api.Query;

namespace Furesoft.Core.ObjectDB.Core.Query.Linq;

internal sealed class LinqQueryable<TElement> : ILinqQueryable<TElement>, IQueryProvider
{
    private readonly ILinqQuery<TElement> _query;

    private LinqQueryable(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException("expression");

        if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
            throw new ArgumentOutOfRangeException("expression");

        Expression = expression;
    }

    public LinqQueryable(ILinqQuery<TElement> query)
    {
        Expression = Expression.Constant(this);
        _query = query;
    }

    private static Expression TranslateQuery(Expression expression)
    {
        return LinqQueryTranslator.Translate(expression);
    }

    #region ILinqQueryable<TElement> Members

    public IEnumerator<TElement> GetEnumerator()
    {
        return Execute<IEnumerable<TElement>>(Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Expression Expression { get; }

    public Type ElementType => typeof(TElement);

    public IQueryProvider Provider => this;

    public ILinqQuery GetQuery()
    {
        return _query;
    }

    #endregion ILinqQueryable<TElement> Members

    #region IQueryProvider Members

    public IQueryable<T> CreateQuery<T>(Expression expression)
    {
        return new LinqQueryable<T>(expression);
    }

    public IQueryable CreateQuery(Expression expression)
    {
        var elementType = TypeSystem.GetElementType(expression.Type);

        try
        {
            return
                (IQueryable)
                Activator.CreateInstance(typeof(LinqQueryable<>).MakeGenericType(elementType), expression);
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException;
        }
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return Expression.Lambda<Func<TResult>>(TranslateQuery(expression)).Compile().Invoke();
    }

    public object Execute(Expression expression)
    {
        return Expression.Lambda(TranslateQuery(expression)).Compile().DynamicInvoke();
    }

    #endregion IQueryProvider Members
}