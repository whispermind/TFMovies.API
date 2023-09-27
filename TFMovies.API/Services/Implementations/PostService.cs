using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Net;
using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Repositories.Interfaces;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

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

        CheckUserFoundOrThrow(currentUser);

        var theme = await GetThemeByIdAsync(model.ThemeId);

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
            Tags = existingTags?.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList() ?? new List<TagDto>()
        };

        return response;
    }

    public async Task<PostUpdateResponse> UpdateAsync(string id, PostUpdateRequest model)
    {
        var postDb = await GetPostByIdOrThrowAsync(id);

        var user = await _userRepository.FindByIdAsync(postDb.UserId);

        var theme = await GetThemeByIdAsync(model.ThemeId);

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

    public async Task<PostsPaginatedResponse> GetAllAsync(PagingSortFilterParams model, PostsQueryParams queryModel, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        //search is available just for authorized users
        if (currentUser == null &&
            (!string.IsNullOrEmpty(queryModel.Query) ||
             !string.IsNullOrEmpty(queryModel.TagQuery) ||
             !string.IsNullOrEmpty(queryModel.CommentQuery)))
        {
            throw new UnauthorizedAccessException();
        }

        var termsQuery = await ExtractTerms(queryModel.Query);

        var matchedTagIds = await ExtractTerms(queryModel.TagQuery, _tagRepository.GetMatchingIdsAsync);

        var matchedCommentIds = await ExtractTerms(queryModel.CommentQuery, _postCommentRepository.GetMatchingIdsAsync);

        var queryDto = new PostsQueryDto
        {
            Query = termsQuery,
            MatchingTagIdsQuery = matchedTagIds,
            MatchingCommentIdsQuery = matchedCommentIds
        };

        var pagedPosts = await _postRepository.GetAllPagingAsync(model, queryDto);

        var response = MapToPostsPaginatedResponse(pagedPosts, currentUser);

        return response;
    }

    public async Task<PostGetByIdResponse> GetByIdAsync(string id, ClaimsPrincipal currentUserPrincipal, int limit)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        CheckUserFoundOrThrow(currentUser);

        var post = await _postRepository.GetFullByIdAsync(id);

        if (post == null)
        {
            throw new ServiceException(HttpStatusCode.NotFound, ErrorMessages.PostNotFound);
        }

        //Author's other posts

        var otherPostsByAuthor = await _postRepository.GetOthersAsync(id, currentUser.Id, limit);

        var otherPostsDtos = otherPostsByAuthor?.Select(p => new PostByAuthorDto
        {
            Id = p.Id,
            Title = p.Title,
            CreatedAt = p.CreatedAt,
            Tags = p.PostTags.Select(pt => new TagDto { Id = pt.Tag.Id, Name = pt.Tag.Name }).ToList()
        });

        //comments
        var comments = await _postCommentRepository.GetAllByPostIdAsync(id);

        var commentDetails = comments?.Select(pc => new CommentDetailDto
        {
            Author = pc.Author,
            Content = pc.Content,
            CreatedAt = pc.CreatedAt
        }).ToList();

        var themeDto = new ThemeDto { Id = post.ThemeId, Name = post.Theme.Name };

        var response = new PostGetByIdResponse
        {
            Id = post.Id,
            CoverImageUrl = post.CoverImageUrl,
            Title = post.Title,
            HtmlContent = post.HtmlContent,
            CreatedAt = post.CreatedAt,
            AuthorId = post.UserId,
            Author = post.User.Nickname,
            IsLiked = (currentUser == null || post.PostLikes == null) ? false : post.PostLikes.Any(pl => pl.UserId == currentUser.Id),
            LikesCount = post.LikeCount,
            CommentsCount = post.PostComments?.Count ?? 0,
            Theme = themeDto,
            Tags = GetTagNamesFromPost(post),
            Comments = commentDetails,
            PostsByAuthor = otherPostsDtos
        };

        return response;
    }

    public async Task<PostAddCommentResponse> AddCommentAsync(string id, PostAddCommentRequest model, ClaimsPrincipal currentUserPrincipal)
    {
        var postDb = await GetPostByIdOrThrowAsync(id);

        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        CheckUserFoundOrThrow(currentUser);

        var newComment = new PostComment
        {
            PostId = id,
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

    public async Task AddLikeAsync(string id, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        CheckUserFoundOrThrow(currentUser);

        var post = await GetPostByIdOrThrowAsync(id);

        var newPostLike = new PostLike
        {
            PostId = id,
            UserId = currentUser.Id
        };

        await _postLikeRepository.CreateAsync(newPostLike);

        post.LikeCount += 1;

        await _postRepository.SaveChangesAsync();
    }

    public async Task RemoveLikeAsync(string id, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        CheckUserFoundOrThrow(currentUser);

        var post = await GetPostByIdOrThrowAsync(id);

        var postLikeDb = await _postLikeRepository.GetPostLikeAsync(id, currentUser.Id);

        if (postLikeDb != null)
        {
            await _postLikeRepository.DeleteAsync(postLikeDb);

            post.LikeCount -= 1;

            if (post.LikeCount < 0)
            {
                post.LikeCount = 0;
            }

            await _postRepository.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<TagDto>> GetTagsAsync(PagingSortFilterParams model)
    {
        var sortOption = (model.Sort ?? string.Empty).ToLower();

        var tagsDb = await _tagRepository.GetTagsAsync(model.Limit, model.Sort, model.Order);

        return tagsDb?.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name
        }) ?? Enumerable.Empty<TagDto>();
    }

    public async Task<PostsPaginatedResponse> GetUserFavoritePostAsync(PagingSortFilterParams model, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await GetUserByIdFromClaimAsync(currentUserPrincipal);

        CheckUserFoundOrThrow(currentUser);

        var likedPostIds = await _postLikeRepository.GetLikedPostIdsByUserIdAsync(currentUser.Id);

        var pagedPosts = await _postRepository.GetByIdsPagingAsync(likedPostIds, model);

        var response = MapToPostsPaginatedResponse(pagedPosts, currentUser);

        return response;
    }


    //helpers
    private async Task<IEnumerable<string>> ExtractTerms(string? input, Func<IEnumerable<string>, Task<IEnumerable<string>>>? repositoryFunc = null)
    {
        if (string.IsNullOrEmpty(input)) return Enumerable.Empty<string>();

        var terms = StringParsingHelper.ParseDelimitedValues(input);

        return repositoryFunc != null ? await repositoryFunc(terms) : terms;
    }

    private PostsPaginatedResponse MapToPostsPaginatedResponse(PagedResult<Post> pagedPosts, User? currentUser)
    {
        var data = pagedPosts.Data.Select(p => new PostShortInfoDto
        {
            Id = p.Id,
            CoverImageUrl = p.CoverImageUrl,
            Title = p.Title,
            CreatedAt = p.CreatedAt,
            AuthorId = p.User.Id,
            Author = p.User.Nickname,
            IsLiked = (currentUser == null || p.PostLikes == null) ? false : p.PostLikes.Any(pl => pl.UserId == currentUser.Id),
            LikesCount = p.LikeCount,
            Tags = GetTagNamesFromPost(p)
        }).ToList();

        return new PostsPaginatedResponse
        {
            Page = pagedPosts.Page,
            Limit = pagedPosts.Limit,
            TotalPages = pagedPosts.TotalPages,
            TotalRecords = pagedPosts.TotalRecords,
            Data = data
        };
    }

    private static void CheckUserFoundOrThrow(User? user)
    {
        if (user == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }
    }
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

    private async Task<Theme> GetThemeByIdAsync(string themeId)
    {
        var theme = await _themeRepository.GetByIdAsync(themeId);

        if (theme == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.ThemeNotFound);
        }

        return theme;
    }

    private async Task<Post> GetPostByIdOrThrowAsync(string id)
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

    private static IEnumerable<TagDto> GetTagNamesFromPost(Post post)
    {
        return post.PostTags.Select(pt => new TagDto
        {
            Id = pt.Tag.Id,
            Name = pt.Tag.Name
        }).ToList() ?? Enumerable.Empty<TagDto>();
    }
    private async Task<bool> IsUserInRoleAsync(User user, string role)
    {
        return await _userRepository.IsInRoleAsync(user, role);
    }
}
