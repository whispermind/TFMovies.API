using Microsoft.AspNetCore.Identity;
using System.Net;
using TFMovies.API.Common.Constants;
using TFMovies.API.Common.Enum;
using TFMovies.API.Exceptions;

namespace TFMovies.API.Utils;

public class RoleInitializer
{
    public static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
    {
        foreach (UserRoleEnum role in Enum.GetValues(typeof(UserRoleEnum)))
        {
            await CreateRoleIfNotExists(roleManager, role.ToString());
        }

    }

    private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole()
                {
                    Name = roleName
                });

            if (!result.Succeeded)
            {
                throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
            }
        }
    }
}
