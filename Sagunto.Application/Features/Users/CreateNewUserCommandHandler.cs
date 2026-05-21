using Microsoft.AspNetCore.Http;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record CreateNewUserCommand(string Name, string Surname, string SaguntinoCode, int RoleId);

    public record CreateNewUserResponse(int UserId);

    public static class CreateNewUserCommandHandler
    {
        [WolverinePost("/api/users")]
        [Tags("Users")]
        [EndpointSummary("Create new user")]
        public static async Task<(CreationResponse, CreateNewUserResponse)> Handle(CreateNewUserCommand command, ISaguntoDbContext dbContext)
        {
            var user = new User(command.Name, command.RoleId, command.SaguntinoCode, command.Surname);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return (
                new CreationResponse($"/api/users/{user.Id}"),
                new CreateNewUserResponse(user.Id)
            );
        }
    }
}
