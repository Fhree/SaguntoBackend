using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record CreateNewUserCommand(string Name, string Surname, int RoleId);

    public record CreateNewUserResponse(int UserId);

    public static class CreateNewUserCommandHandler
    {
        [WolverinePost("/api/users")]
        [Tags("Users")]
        [EndpointSummary("Create new user")]
        public static async Task<(CreationResponse, CreateNewUserResponse)> Handle(CreateNewUserCommand command, ISaguntoDbContext dbContext, CancellationToken cancellationToken)
        {
            string saguntinoCode;
            bool codeExists;

            do
            {
                saguntinoCode = GenerateSaguntinoCode();
                codeExists = await dbContext.Users.AnyAsync(u => u.SaguntinoCode == saguntinoCode, cancellationToken);

            } while (codeExists);

            var user = new User(command.Name, command.RoleId, saguntinoCode, command.Surname);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return (
                new CreationResponse($"/api/users/{user.Id}"),
                new CreateNewUserResponse(user.Id)
            );
        }

        private static string GenerateSaguntinoCode()
        {
            char letter = (char)Random.Shared.Next(65, 91);
            int number = Random.Shared.Next(0, 100);
            return $"{letter}{number:D2}";
        }
    }
}
