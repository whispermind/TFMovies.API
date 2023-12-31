﻿using TFMovies.API.Data.Entities;
using TFMovies.API.Data;
using TFMovies.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using TFMovies.API.Models.Dto;


namespace TFMovies.API.Repositories.Implementations;

public class PostLikeRepository : BaseRepository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(DataContext context) : base(context)
    { }

    public async Task<List<string>> GetLikedPostIdsByUserIdAsync(string userId)
    {
        return await _entities
            .Where(e => e.UserId == userId)
            .Select(e => e.PostId)
            .ToListAsync();
    }

    public async Task<PostLike?> GetPostLikeAsync(string postId, string userId)
    {
        var result = await _entities
            .Where(pl => pl.PostId == postId && pl.UserId == userId)
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<IEnumerable<UserPostLikeCountsDto>> GetUserIdsByPostLikeCountsAsync(int? limit, string? order)
    {
        var actualLimit = limit ?? 10;

        IQueryable<UserPostLikeCountsDto> query = _entities
        .Include(pl => pl.Post)
        .Where(pl => pl.Post != null)
        .GroupBy(pl => pl.Post.UserId)
        .Select(g => new UserPostLikeCountsDto
        {
            AuthorId = g.Key,
            LikeCount = g.Count()
        });

        if (order == "asc")
        {
            query = query
                .OrderBy(g => g.LikeCount);
        }
        else
        {
            query = query
                .OrderByDescending(g => g.LikeCount);
        }

        var result = await query
            .Take(actualLimit)
            .ToListAsync();

        return result;
    }

    public async Task<bool> IsExistAsync(string userId, string postId)
    {
        return await _entities
            .AnyAsync(pl => pl.UserId == userId && pl.PostId == postId);
    }
}
