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
        var currentUser = await _userRepository.FindByIdAsync(userId);

        if (currentUser == null)
        {
            throw new ServiceException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
        }

        var theme = await _themeRepository.FindByNameAsync(model.Theme);

        if (theme == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.ThemeNotFound);
        }

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

        var tagNames = model.Tags
                        .Where(tn => !string.IsNullOrWhiteSpace(tn))
                        .Distinct()
                        .ToList();

        var existingTags = (await _tagRepository.FindByNamesAsync(tagNames)).ToList();
        var existingTagNames = existingTags.Select(t => t.Name).ToHashSet();
        
        var newTags = tagNames
            .Where(tn => !existingTagNames.Contains(tn))
            .Select(tn => new Tag { Name = tn })
            .ToList();

        if (newTags.Any())
        {
            await _tagRepository.CreateRangeAsync(newTags);
            existingTags.AddRange(newTags);
        }


        var newPostTags = existingTags.Select(t => new PostTag { Post = post, Tag = t }).ToList();
        await _postTagRepository.CreateRangeAsync(newPostTags);

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
}
