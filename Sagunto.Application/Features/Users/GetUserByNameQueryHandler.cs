using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{

    public record UserByNameResponseDto(int Id, string Name, string Surname,string SaguntinoCode);

    public static class GetUserByNameQueryHandler
    {
        [WolverineGet("/api/users/name/{name}")]
        [Tags("Users")]
        [EndpointSummary("Get user by name")]
        public static async Task<List<UserByNameResponseDto>> Handle(string name, ISaguntoDbContext dbContext)
        {
            var user = await dbContext.Users.Include(u => u.Role).AsNoTracking().Where(u => u.Name == name).ToListAsync();

            if (user == null || user.Count == 0)
                return [];
            else
                return [.. user.Select(u => new UserByNameResponseDto(u.Id, u.Name, u.Surname, u.SaguntinoCode))];
        }
    }
}
