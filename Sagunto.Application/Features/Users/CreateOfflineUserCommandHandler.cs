using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    
    public record CreateOfflineUserCommand(string Name, string Surname);
    public record CreateOfflineUserResponse(int Id, string Name, string Surname, string SaguntinoCode, int RoleId);

    public static class CreateOfflineUserCommandHandler
    {
        [Authorize]
        [WolverinePost("/api/admin/users")]
        [Tags("Admin")]
        [EndpointSummary("Create offline user")]
        public static async Task<IResult> Handle(CreateOfflineUserCommand command, ISaguntoDbContext dbContext, ClaimsPrincipal userPrincipal,CancellationToken cancellationToken)
        {
            
            var callerFirebaseUid = userPrincipal.FindFirstValue("user_id") ?? userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(callerFirebaseUid))
            {
                return TypedResults.Unauthorized();
            }

            var caller = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.FirebaseUid == callerFirebaseUid, cancellationToken);

            if (caller == null || caller.RoleId != 1)
                return TypedResults.StatusCode(StatusCodes.Status403Forbidden);

            string saguntinoCode;
            bool codeExists;

            do
            {
                saguntinoCode = GenerateSaguntinoCode();
                codeExists = await dbContext.Users.AnyAsync(u => u.SaguntinoCode == saguntinoCode, cancellationToken);

            } while (codeExists);

            var newUser = new User(command.Name.Trim(), 2, saguntinoCode.Trim(), command.Surname.Trim());

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync(cancellationToken);

            return TypedResults.Created($"/api/admin/users/{newUser.Id}",
                new CreateOfflineUserResponse(newUser.Id, newUser.Name, newUser.Surname, newUser.SaguntinoCode, newUser.RoleId));
        }

        private static string GenerateSaguntinoCode()
        {
            char letter = (char)Random.Shared.Next(65, 91);
            int number = Random.Shared.Next(0, 100);
            return $"{letter}{number:D2}";
        }
    }
}