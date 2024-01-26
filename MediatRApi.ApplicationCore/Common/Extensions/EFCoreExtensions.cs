using System.Linq.Expressions;
using MediatRApi.ApplicationCore.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MediatRApi.ApplicationCore.Common.Extensions;

public static class EFCoreExtensions
{
    public static async Task<PagedResult<TEntity>> GetPagedResultAsync<TEntity>(this IQueryable<TEntity> source, int pageSize, int currentPage)
        where TEntity : class
    {
        var rows = await source.CountAsync();
        var items = await source
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TEntity>
        {
            Results = items,
            RowsCount = rows,
            PageCount = (int)Math.Ceiling((double)rows / pageSize),
            PageSize = pageSize,
            CurrentPage = currentPage
        };
    }

    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByStrValues, string defaultOrderBy)
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
            //return source;
            Console.WriteLine($"defaultOrderBy: {defaultOrderBy}");
            //return source.OrderBy(x => x.GetType().GetProperty(defaultOrderBy)!.GetValue(x, null));
            property = type.GetProperty(defaultOrderBy)!;
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