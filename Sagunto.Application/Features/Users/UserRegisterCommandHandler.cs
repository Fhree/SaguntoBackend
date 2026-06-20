using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record UserRegisterCommand(string Name, string Surname);

    public record UserRegisterResponse(int Id, string Name, string Surname, string SaguntinoCode, int RoleId);

    public static class UserRegisterCommandHandler
    {
        [Authorize]
        [WolverinePost("/api/users/register")]
        [Tags("Users")]
        [EndpointSummary("Register user")]
        public static async Task<IResult> Handle(UserRegisterCommand command, ISaguntoDbContext dbContext, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            var firebaseUid = userPrincipal.FindFirstValue("user_id") ?? userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = userPrincipal.FindFirstValue("email") ?? userPrincipal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(firebaseUid) || string.IsNullOrEmpty(email))
                return TypedResults.Unauthorized();

            var userExists = await dbContext.Users.AnyAsync(u => u.FirebaseUid == firebaseUid, cancellationToken);
            if (userExists)
                return TypedResults.Conflict("El usuario ya está registrado en el sistema.");

            string saguntinoCode;
            bool codeExists;

            do
            {
                saguntinoCode = GenerateSaguntinoCode();
                codeExists = await dbContext.Users.AnyAsync(u => u.SaguntinoCode == saguntinoCode, cancellationToken);

            } while (codeExists);

            var user = new User(firebaseUid, email, command.Name.Trim(), 2, saguntinoCode.Trim(), command.Surname.Trim());

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return TypedResults.Created($"/api/users/register", new UserRegisterResponse(user.Id, user.Name, user.Surname, user.SaguntinoCode, user.RoleId));
        }

        private static string GenerateSaguntinoCode()
        {
            char letter = (char)Random.Shared.Next(65, 91);
            int number = Random.Shared.Next(0, 100);
            return $"{letter}{number:D2}";
        }
    }
}