using System.Linq.Expressions;

namespace MediatRApi.ApplicationCore.Common.Extensions;

public static class EFCoreExtensions
{
    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByStrValues)
        where TEntity : class
    {
        var qryExpression = source.Expression;
        var command = orderByStrValues.ToUpper().EndsWith("DESC") ? "OrderByDescending" : "OrderBy";
        var propertyName = orderByStrValues.Split(' ')[0].Trim();

        var type = typeof(TEntity);
        var property = type.GetProperties()
            .Where(item => item.Name.ToLower() == propertyName.ToLower())
            .FirstOrDefault();

        if (property is null)
        {
            return source;
        }

        //p
        var parameter = Expression.Parameter(type, "p");

        //p.Price
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);

        //p => p.Price
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        //Ejemplo Final: source.OrderBy(p => p.Price) or source.OrderByDescending(p => p.Price)
        qryExpression = Expression.Call(
            type: typeof(Queryable),
            methodName: command,
            typeArguments: new Type[] { type, property.PropertyType },
            qryExpression,
            Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<TEntity>(qryExpression);
    }
}