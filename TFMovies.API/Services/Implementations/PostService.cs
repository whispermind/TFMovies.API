using System.Data;
using System.Net;
using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Exceptions;
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

    public PostService(
        IUserRepository userRepository,
        IThemeRepository themeRepository,
        IPostRepository postRepository,
        ITagRepository tagRepository,
        IPostTagRepository postTagRepository)
    {
        _userRepository = userRepository;
        _themeRepository = themeRepository;
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _postTagRepository = postTagRepository;
    }

    public async Task<PostCreateResponse> CreateAsync(PostCreateRequest model, ClaimsPrincipal currentUserPrincipal)
    {
        var userId = currentUserPrincipal.FindFirstValue("sub");
    
        var currentUser = await GetUserByIdAsync(userId);       

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
            AuthorNickname = currentUser.Nickname,
            ThemeName = theme.Name,
            TagNames = existingTags.Select(t => t.Name).ToList()
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
        return tagNames?.Where(tn => !string.IsNullOrWhiteSpace(tn)).Distinct().ToList() ?? new List<string>();
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

        if (tagsToRemove.Any())
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

    private async Task<User> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.FindByIdAsync(id);

        if (user == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }
        return user;
    }
}
