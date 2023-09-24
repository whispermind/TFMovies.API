using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TFMovies.API.Filters;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Extensions;

public static class IQueryableExtensions
{
    public static async Task<PagedResult<T>> GetPagedDataAsync<T>(this IQueryable<T> query, PaginationSortDto<T> dto)       
    {
        var totalRecords = await query.CountAsync();

        if (totalRecords == 0)
        {
            return new PagedResult<T>
            {
                Page = dto.Page,
                Limit = dto.Limit,
                TotalPages = 0,
                TotalRecords = 0,
                Data = new List<T>()
            };
        }

        var totalPages = (int)Math.Ceiling(totalRecords / (double)dto.Limit);

        if (dto.Page > totalPages)
        {
            dto.Page = totalPages;
        }

        if (dto.SortSelector != null)
        {
            query = dto.Order switch
            {
                "asc" => query.OrderBy(dto.SortSelector),
                _ => query.OrderByDescending(dto.SortSelector)
            };
        }        

        query = query.Skip((dto.Page - 1) * dto.Limit)
                     .Take(dto.Limit);

        var data = await query.ToListAsync();

        return new PagedResult<T>
        {
            Page = dto.Page,
            Limit = dto.Limit,
            TotalPages = totalPages,
            TotalRecords = totalRecords,
            Data = data
        };
    }
}
