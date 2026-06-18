using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using System.Security.Claims;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record UserProfileResponse(string Name, string SaguntinoCode, int RoleId);

    public static class GetUserProfile
    {
        [Authorize]
        [WolverineGet("/api/users/profile")]
        [Tags("Users")]
        [EndpointSummary("Get the profile of the authenticated user via Firebase")]
        public static async Task<IResult> Handle(ClaimsPrincipal userPrincipal, ISaguntoDbContext dbContext, CancellationToken cancellationToken)
        {
            var firebaseUid = userPrincipal.FindFirstValue("user_id") ?? userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(firebaseUid))
                return TypedResults.Unauthorized();

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, cancellationToken);

            if (user == null)
                return TypedResults.NotFound("Usuario no encontrado en la base de datos. Requiere registro previo.");

            return TypedResults.Ok(new UserProfileResponse(user.Name, user.SaguntinoCode, user.RoleId));
        }
    }
}