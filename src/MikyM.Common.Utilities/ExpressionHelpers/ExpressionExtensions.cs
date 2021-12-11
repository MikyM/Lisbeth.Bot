using System.Linq.Expressions;

namespace MikyM.Common.Utilities.ExpressionHelpers;

public static class ExpressionExtensions
{
    /// <summary>
    /// Convert a lambda expression for a getter into a setter
    /// </summary>
    public static Action<TEntity, TValue> GetSetter<TEntity, TValue>(this Expression<Func<TEntity, TValue>> expression)
    {
        var parameter = Expression.Parameter(typeof(TValue), "value");
        var setterLambda = Expression.Lambda<Action<TEntity, TValue>>(Expression.Assign(expression.Body, parameter),
            expression.Parameters[0], parameter);

        return setterLambda.Compile();
    }
}
