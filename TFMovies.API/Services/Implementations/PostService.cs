using System.Data;
using System.Net;
using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Exceptions;
using TFMovies.API.Mappers;
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
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        await GetThemeByIdOrThrowAsync(model.ThemeId);

        var newPost = PostMapper.ToCreateEntity(model, currentUser);

        await _postRepository.CreateAsync(newPost);

        var existingTags = await GetOrCreateTagsAsync(model.Tags);

        if (existingTags != null)
        {
            await AddPostTagRelationAsync(newPost, existingTags);
        }

        var response = new PostCreateResponse 
        {
            Id = newPost.Id
        };  

        return response;
    }

    public async Task UpdateAsync(string id, PostUpdateRequest model)
    {
        var postDb = await GetPostByIdOrThrowAsync(id);

        var user = await _userRepository.FindByIdAsync(postDb.UserId);

        var theme = await GetThemeByIdOrThrowAsync(model.ThemeId);

        PostMapper.ToUpdateEntity(postDb, model, theme);

        var existingTags = await GetOrCreateTagsAsync(model.Tags);

        await UpdatePostTagRelationAsync(postDb, existingTags);

        await _postRepository.UpdateAsync(postDb);       
    }

    public async Task<PagedResult<PostShortInfoDto>> GetAllAsync(PagingSortParams pagingSortModel, PostsFilterParams filterModel, PostsQueryParams queryModel, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        //search is available just for authorized users
        if (currentUser == null &&
            (!string.IsNullOrEmpty(queryModel.Articles) ||
             !string.IsNullOrEmpty(queryModel.Tags) ||
             !string.IsNullOrEmpty(queryModel.Comments)))
        {
            throw new UnauthorizedAccessException();
        }

        var termsQuery = await ExtractTerms(queryModel.Articles);

        var matchedTagIds = await ExtractTerms(queryModel.Tags, _tagRepository.GetMatchingIdsAsync);

        var matchedCommentIds = await ExtractTerms(queryModel.Comments, _postCommentRepository.GetMatchingIdsAsync);

        if ((!string.IsNullOrEmpty(queryModel.Tags) && (matchedTagIds == null || !matchedTagIds.Any())) ||
            (!string.IsNullOrEmpty(queryModel.Comments) && (matchedCommentIds == null || !matchedCommentIds.Any())))
        {
            var emptyResult = GenericMapper.GetEmptyPagedResult<PostShortInfoDto>();
            return emptyResult;
        }

        var queryDto = PostMapper.ToPostsQueryDto(termsQuery, matchedTagIds, matchedCommentIds);

        var pagedPosts = await _postRepository.GetAllPagingAsync(pagingSortModel, filterModel, queryDto);

        var response = GenericMapper.ToPaginatedResponse(pagedPosts, post => PostMapper.ToPostShortInfoDto(post, currentUser));        

        return response;
    }

    public async Task<PostGetByIdResponse> GetByIdAsync(string id, ClaimsPrincipal currentUserPrincipal, int limit)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        var post = await _postRepository.GetFullByIdAsync(id);

        if (post == null)
        {
            throw new ServiceException(HttpStatusCode.NotFound, ErrorMessages.PostNotFound);
        }

        //Author's other posts

        var otherPostsByAuthor = await _postRepository.GetOthersAsync(id, post.UserId, limit);

        var otherPostsDtos = otherPostsByAuthor?.Select(p => PostMapper.ToPostByAuthorDto(p)).ToList();

        //comments
        var comments = await _postCommentRepository.GetAllByPostIdAsync(id);

        var commentDetails = comments?.Select(c => PostCommentMapper.ToCommentDetailDto(c)).ToList();

        //theme
        var themeDto = ThemeMapper.ToThemeDto(post);

        //map the response
        var response = PostMapper.ToPostGetByIdResponse(post, currentUser, themeDto, otherPostsDtos, commentDetails);

        return response;
    }

    public async Task<PostAddCommentResponse> AddCommentAsync(string id, PostAddCommentRequest model, ClaimsPrincipal currentUserPrincipal)
    {
        await GetPostByIdOrThrowAsync(id);

        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        var newComment = PostCommentMapper.ToCreateEntity(id, model, currentUser);

        await _postCommentRepository.CreateAsync(newComment);

        var response = PostCommentMapper.ToPostAddCommentResponse(newComment);

        return response;
    }

    public async Task AddLikeAsync(string id, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        var isExist = await _postLikeRepository.IsExistAsync(currentUser.Id, id);

        if (isExist)
        {
            return;
        }

        var post = await GetPostByIdOrThrowAsync(id);

        var newPostLike = PostLikeMapper.ToCreateEntity(id, currentUser);

        await _postLikeRepository.CreateAsync(newPostLike);

        post.LikeCount += 1;

        await _postRepository.SaveChangesAsync();
    }

    public async Task RemoveLikeAsync(string id, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

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

    public async Task<IEnumerable<TagDto>> GetTagsAsync(PagingSortParams model)
    {       
        var tagsDb = await _tagRepository.GetTagsAsync(model);        

        var response = tagsDb?.Select(t => TagMapper.ToTagDto(t)).ToList() ?? Enumerable.Empty<TagDto>();        

        return response;
    }

    public async Task<PagedResult<PostShortInfoDto>> GetUserFavoritePostAsync(PagingSortParams model, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        var likedPostIds = await _postLikeRepository.GetLikedPostIdsByUserIdAsync(currentUser.Id);

        var pagedPosts = await _postRepository.GetByIdsPagingAsync(likedPostIds, model);
        
        var response = GenericMapper.ToPaginatedResponse(pagedPosts, post => PostMapper.ToPostShortInfoDto(post, currentUser));

        return response;
    }

    public async Task DeleteAsync(string id, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        var post = await _postRepository.GetByIdAsync(id);

        if (post == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.PostNotFound);
        }

        var isAdmin = currentUserPrincipal.IsInRole(RoleNames.Admin);

        if (!isAdmin)
        {
            if (currentUser.Id != post.UserId)
            {
                throw new ServiceException(HttpStatusCode.Forbidden, ErrorMessages.CommentDeleteForbidden);
            }
        }
        //var commentToDelete = await _commentRepository.GetByIdAsync(id);

        await _postRepository.DeleteAsync(post);
    }

    //helpers
    private async Task<IEnumerable<string>> ExtractTerms(string? input, Func<IEnumerable<string>, Task<IEnumerable<string>>>? repositoryFunc = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Enumerable.Empty<string>();
        }

        var terms = StringParsingHelper.ParseDelimitedValues(input);

        return repositoryFunc != null ? await repositoryFunc(terms) : terms;
    }   
    
    private async Task<List<Tag>> GetOrCreateTagsAsync(List<string> tagNames)
    {
        tagNames = CleanAndDistinctTags(tagNames);

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
        return tagNames
        .Where(tn => !string.IsNullOrWhiteSpace(tn))
        .Select(tn => tn.ToLower())
        .Distinct()
        .ToList();
    }

    private async Task AddPostTagRelationAsync(Post post, List<Tag> tags)
    {
        var newPostTags = tags.Select(t => new PostTag { Post = post, Tag = t }).ToList();

        await _postTagRepository.CreateRangeAsync(newPostTags);
    }

    private async Task UpdatePostTagRelationAsync(Post post, List<Tag> newTags)
    {
        var existingPostTags = await _postTagRepository.FindByPostIdAsync(post.Id) ?? new List<PostTag>();

        if (!newTags.Any())
        {
            await _postTagRepository.DeleteRangeAsync(existingPostTags);
            return;
        }

        var tagIds = newTags.Select(t => t.Id).ToList();

        var tagsToRemove = existingPostTags?.Where(pt => !tagIds.Contains(pt.TagId)).ToList();

        var tagsToAdd = newTags.Where(t => existingPostTags == null || !existingPostTags.Any(pt => pt.TagId == t.Id))
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

    private async Task<Theme> GetThemeByIdOrThrowAsync(string themeId)
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
}
