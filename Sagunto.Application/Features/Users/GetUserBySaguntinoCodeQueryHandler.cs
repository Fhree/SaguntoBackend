using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record UserBySaguntinoCodeResponseDto(int Id, string Name, string SaguntinoCode, string Role);

    public static class GetUserBySaguntinoCodeQueryHandler
    {
        [WolverineGet("/api/users/{saguntinoCode}")]
        public static async Task<UserBySaguntinoCodeResponseDto?> Handle(string saguntinoCode, ISaguntoDbContext dbContext)
        {
            var user = await dbContext.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(u => u.SaguntinoCode == saguntinoCode);

            if (user == null)
                return null;
            else
                return new UserBySaguntinoCodeResponseDto(user.Id, user.Name, user.SaguntinoCode, user.Role?.Name ?? "No role");
        }
    }
}