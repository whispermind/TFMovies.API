using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using TFMovies.API.Common.Constants;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Extensions;

public static class IQueryableExtensions
{
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

    public static IQueryable<T> SearchByTerms<T>(this IQueryable<T> query, IEnumerable<string> columns, IEnumerable<string> terms)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        Expression body = Expression.Constant(false);

        foreach (var term in terms)
        {
            foreach (var column in columns)
            {
                var property = Expression.Property(parameter, column);

                var containsExpression = ConstructContainsExpression(property, term);

                body = Expression.OrElse(body, containsExpression);
            }
        }

        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

        return query.Where(lambda);
    }


    public static async Task<PagedResult<T>> GetPagedDataAsync<T>(this IQueryable<T> query, PagingSortFilterDto<T> dto)
    {
        if (dto.FilterPredicate != null)
        {
            query = query.Where(dto.FilterPredicate);
        }

        var totalRecords = await query.CountAsync();

        if (totalRecords == 0)
        {
            return new PagedResult<T>
            {
                Page = 0,
                Limit = 0,
                TotalPages = 0,
                TotalRecords = 0,
                Data = new List<T>()
            };
        }

        int defaultPage = DefaultPaginationValues.PageNumber; 
        int defaultLimit = DefaultPaginationValues.MaxLimit; 

        int actualPage = dto.Page ?? defaultPage;
        int actualLimit = dto.Limit ?? defaultLimit;

        var totalPages = (int)Math.Ceiling(totalRecords / (double)actualLimit);

        if (actualPage > totalPages)
        {
            actualPage = totalPages;
        }

        if (dto.SortSelector != null)
        {
            query = dto.Order switch
            {
                "asc" => query.OrderBy(dto.SortSelector),
                _ => query.OrderByDescending(dto.SortSelector)
            };
        }

        query = query.Skip((actualPage - 1) * actualLimit)
                     .Take(actualLimit);

        var data = await query.ToListAsync();

        return new PagedResult<T>
        {
            Page = actualPage,
            Limit = actualLimit,
            TotalPages = totalPages,
            TotalRecords = totalRecords,
            Data = data
        };
    }

    private static Expression ConstructContainsExpression(Expression property, string term)
    {
        var termExpression = Expression.Constant(term, typeof(string));
        
        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
       
        var containsExpression = Expression.Call(property, ContainsMethod, termExpression);
       
        return Expression.AndAlso(nullCheck, containsExpression);        
    }
}
