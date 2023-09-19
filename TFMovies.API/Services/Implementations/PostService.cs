using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Entitiesl;
using TFMovies.API.Exceptions;
using TFMovies.API.Extensions;
using TFMovies.API.Filters;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Repositories.Interfaces;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Services.Implementations;

public class PostService : IPostService
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly IThemeRepository _themeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IPostTagRepository _postTagRepository;
    private readonly IPostCommentRepository _postCommentRepository;
    private readonly IPostLikeRepository _postLikeRepository;

    public PostService(
        IUserRepository userRepository,
        IThemeRepository themeRepository,
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IPostTagRepository postTagRepository,
        IPostCommentRepository postCommentRepository,
        IPostLikeRepository postLikeRepository)
    {
        _userRepository = userRepository;
        _themeRepository = themeRepository;
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _postTagRepository = postTagRepository;
        _postCommentRepository = postCommentRepository;
        _postLikeRepository = postLikeRepository;
    }

    public async Task<PostCreateResponse> CreateAsync(PostCreateRequest model, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        if (currentUser == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }

        var theme = await GetThemeByNameAsync(model.Theme);

        var post = new Post
        {
            UserId = currentUser.Id,
            ThemeId = theme.Id,
            Title = model.Title,
            HtmlContent = model.HtmlContent,
            CoverImageUrl = model.CoverImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };


        await _postRepository.CreateAsync(post);

        var existingTags = await GetOrCreateTagsAsync(model.Tags);

        if (existingTags != null)
        {
            await AddPostTagRelationAsync(post, existingTags);
        }

        var response = new PostCreateResponse
        {
            Id = post.Id,
            CoverImageUrl = post.CoverImageUrl,
            Title = post.Title,
            HtmlContent = post.HtmlContent,
            CreatedAt = post.CreatedAt,
            Author = currentUser.Nickname,
            Theme = theme.Name,
            Tags = existingTags?.Select(t => t.Name).ToList() ?? new List<string>()
        };

        return response;
    }

    public async Task<PostUpdateResponse> UpdateAsync(string id, PostUpdateRequest model)
    {
        var postDb = await GetPostByIdAsync(id);

        var user = await _userRepository.FindByIdAsync(postDb.UserId);

        var theme = await GetThemeByNameAsync(model.Theme);

        postDb.CoverImageUrl = model.CoverImageUrl;
        postDb.ThemeId = theme.Id;
        postDb.Title = model.Title;
        postDb.HtmlContent = model.HtmlContent;
        postDb.UpdatedAt = DateTime.UtcNow;

        var existingTags = await GetOrCreateTagsAsync(model.Tags);

        await UpdatePostTagRelationAsync(postDb, existingTags);

        await _postRepository.UpdateAsync(postDb);

        var response = new PostUpdateResponse
        {
            Id = postDb.Id,
            CoverImageUrl = postDb.CoverImageUrl,
            Title = postDb.Title,
            HtmlContent = postDb.HtmlContent,
            CreatedAt = postDb.CreatedAt,
            Author = user.Nickname,
            Theme = theme.Name,
            Tags = existingTags.Select(t => t.Name).ToList()
        };

        return response;
    }

    public async Task<PostGetAllResponse> GetAllAsync(int page, int limit, string? sort, string? theme, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        IQueryable<Post> query = _postRepository.Query()
            .Include(p => p.User)
            .Include(p => p.Theme)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostLikes);

        if (!string.IsNullOrEmpty(theme))
        {
            var themeDb = await GetThemeByNameAsync(theme);
            query = query.Where(post => post.ThemeId == themeDb.Id);
        }

        Expression<Func<Post, object>> sortSelector;
        switch (sort?.ToLower())
        {
            case "rated":
                sortSelector = p => p.PostLikes.Count;
                break;
            default:
                sortSelector = p => p.CreatedAt; // default
                break;
        }

        var pagination = new PaginationFilter
        {
            Limit = limit,
            Page = page
        };

        var pagedResult = await query.GetPagedDataAsync(pagination, sortSelector, "desc");

        var data = pagedResult.Data.Select(p => new PostShortInfoDto
        {
            Id = p.Id,
            CoverImageUrl = p.CoverImageUrl,
            Title = p.Title,
            CreatedAt = p.CreatedAt,
            Author = p.User.Nickname,
            IsLiked = (currentUser == null || p.PostLikes == null) ? false : p.PostLikes.Any(pl => pl.UserId == currentUser.Id),
            Tags = GetTagNamesFromPost(p)
        }).ToList();

        return new PostGetAllResponse
        {
            Page = pagedResult.Page,
            Limit = pagedResult.Limit,
            TotalPages = pagedResult.TotalPages,
            TotalRecords = pagedResult.TotalRecords,
            Theme = theme,
            Sort = sort,
            Data = data
        };
    }

    public async Task<PostGetByIdResponse> GetByIdAsync(string id, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        var post = await _postRepository.Query()
            .Include(p => p.User)
            .Include(p => p.Theme)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostComments)
                .ThenInclude(pc => pc.User)
            .Include(p => p.PostLikes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            throw new ServiceException(HttpStatusCode.NotFound, ErrorMessages.PostNotFound);
        }

        var otherPostsFromAuthor = await _postRepository.Query()
            .Where(p => p.UserId == post.UserId && p.Id != id)
            .Select(p => new AuthorOtherPosts
            {
                Id = p.Id,
                Title = p.Title,
                CreatedAt = p.CreatedAt,
                Tags = p.PostTags.Select(pt => pt.Tag.Name).ToList()
            })
            .OrderByDescending(p => p.CreatedAt)
            .Take(DefaultPaginationValues.AuthorOtherPostsLimit)
            .ToListAsync();

        var commentDetails = post.PostComments?.Select(pc => new CommentDetail
        {
            Author = pc.Author,
            Content = pc.Content,
            CreatedAt = pc.CreatedAt
        }).ToList();

        var response = new PostGetByIdResponse
        {
            Id = post.Id,
            CoverImageUrl = post.CoverImageUrl,
            Title = post.Title,
            Theme = post.Theme.Name,
            HtmlContent = post.HtmlContent,
            CreatedAt = post.CreatedAt,
            AuthorId = post.UserId,
            Author = post.User.Nickname,
            IsLiked = (currentUser == null || post.PostLikes == null) ? false : post.PostLikes.Any(pl => pl.UserId == currentUser.Id),
            LikesCount = post.PostLikes?.Count ?? 0,
            CommentsCount = post.PostComments?.Count ?? 0,
            Tags = GetTagNamesFromPost(post),
            Comments = commentDetails,
            AuthorOtherPosts = otherPostsFromAuthor
        };

        return response;
    }
    public async Task<PostAddCommentResponse> AddCommentAsync(PostAddCommentRequest model, ClaimsPrincipal currentUserPrincipal)
    {
        var postDb = await GetPostByIdAsync(model.PostId);

        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        if (currentUser == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }

        var newComment = new PostComment
        {
            PostId = model.PostId,
            UserId = currentUser.Id,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _postCommentRepository.CreateAsync(newComment);

        return new PostAddCommentResponse
        {
            PostId = newComment.PostId,
            Content = newComment.Content,
            CreatedAt = newComment.CreatedAt,
            Author = newComment.Author
        };
    }

    public async Task AddLikeAsync(string postId, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        if (currentUser == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }

        var newPostLike = new PostLike
        {
            PostId = postId,
            UserId = currentUser.Id
        };

        await _postLikeRepository.CreateAsync(newPostLike);
    }

    public async Task RemoveLikeAsync(string postId, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        if (currentUser == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }

        var postLikeDb = await _postLikeRepository.Query()
                             .Where(pl => pl.PostId == postId && pl.UserId == currentUser.Id)
                             .FirstOrDefaultAsync();

        if (postLikeDb != null)
        {
            await _postLikeRepository.DeleteAsync(postLikeDb);
        }
    }

    //helpers 
    private async Task<List<Tag>> GetOrCreateTagsAsync(List<string> tagNames)
    {
        CleanAndDistinctTags(tagNames);

        var existingTags = new List<Tag>();

        if (tagNames.Any())
        {
            existingTags = (await _tagRepository.FindByNamesAsync(tagNames)).ToList();

            var existingTagNames = existingTags.Select(t => t.Name).ToHashSet();

            var newTags = tagNames.Where(tn => !existingTagNames.Contains(tn))
                              .Select(tn => new Tag { Name = tn })
                              .ToList();

            if (newTags.Any())
            {
                await _tagRepository.CreateRangeAsync(newTags);
                existingTags.AddRange(newTags);
            }
        }

        return existingTags;
    }

    private static List<string> CleanAndDistinctTags(List<string> tagNames)
    {
        return tagNames.Where(tn => !string.IsNullOrWhiteSpace(tn)).Distinct().ToList();
    }

    private async Task AddPostTagRelationAsync(Post post, List<Tag> tags)
    {
        var newPostTags = tags.Select(t => new PostTag { Post = post, Tag = t }).ToList();

        await _postTagRepository.CreateRangeAsync(newPostTags);
    }

    private async Task UpdatePostTagRelationAsync(Post post, List<Tag> tags)
    {
        var existingPostTags = await _postTagRepository.FindByPostIdAsync(post.Id) ?? new List<PostTag>();

        if (!tags.Any())
        {
            await _postTagRepository.DeleteRangeAsync(existingPostTags);
            return;
        }

        var tagNames = tags.Select(t => t.Name).ToList();

        var tagsToRemove = existingPostTags?.Where(pt => pt.Tag != null && !tagNames.Contains(pt.Tag.Name)).ToList();

        var tagsToAdd = tags.Where(t => existingPostTags == null || !existingPostTags.Any(pt => pt.TagId == t.Id))
                                    .Select(t => new PostTag { Post = post, Tag = t })
                                    .ToList();

        if (tagsToRemove != null && tagsToRemove.Any())
        {
            await _postTagRepository.DeleteRangeAsync(tagsToRemove);
        }

        if (tagsToAdd.Any())
        {
            await _postTagRepository.CreateRangeAsync(tagsToAdd);
        }
    }

    private async Task<Theme> GetThemeByNameAsync(string themeName)
    {
        var theme = await _themeRepository.FindByNameAsync(themeName);
        if (theme == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.ThemeNotFound);
        }
        return theme;
    }

    private async Task<Post> GetPostByIdAsync(string id)
    {
        var post = await _postRepository.GetByIdAsync(id);

        if (post == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.PostNotFound);
        }
        return post;
    }

    private async Task<User?> GetUserByIdFromClaimAsync(ClaimsPrincipal currentUserPrincipal)
    {
        var userId = currentUserPrincipal.FindFirstValue("sub");

        var user = await _userRepository.FindByIdAsync(userId);

        return user;
    }

    private static IEnumerable<string> GetTagNamesFromPost(Post post)
    {
        return post.PostTags.Select(pt => pt.Tag.Name).ToList();
    }
}
