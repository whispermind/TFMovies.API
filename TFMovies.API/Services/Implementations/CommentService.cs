using System.Net;
using System.Security.Claims;
using TFMovies.API.Common.Constants;
using TFMovies.API.Exceptions;
using TFMovies.API.Repositories.Interfaces;
using TFMovies.API.Services.Interfaces;
using TFMovies.API.Utils;

namespace TFMovies.API.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly IPostCommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(IPostCommentRepository commentRepository, IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }
    public async Task DeleteAsync(string id, ClaimsPrincipal currentUserPrincipal)
    {
        var currentUser = await UserUtils.GetUserByIdFromClaimAsync(_userRepository, currentUserPrincipal);

        UserUtils.CheckCurrentUserFoundOrThrow(currentUser);

        var comment = await _commentRepository.GetCommentUserPostInfoByIdAsync(id);

        if (comment == null)
        {
            throw new ServiceException(HttpStatusCode.BadRequest, ErrorMessages.CommentNotFound);
        }

        var isAdmin = currentUserPrincipal.IsInRole(RoleNames.Admin);
        
        if (!isAdmin)
        {            
            if (currentUser.Id != comment.UserId && currentUser.Id != comment.PostUserId)
            {    
                throw new ServiceException(HttpStatusCode.Forbidden, ErrorMessages.CommentDeleteForbidden);
            }
        }

        var commentToDelete = await _commentRepository.GetByIdAsync(id);

        await _commentRepository.DeleteAsync(commentToDelete);        
    }
}
