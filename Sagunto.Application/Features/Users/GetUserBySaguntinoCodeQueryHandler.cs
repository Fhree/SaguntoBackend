using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record UserBySaguntinoCodeResponseDto(int Id, string Name, string SaguntinoCode);

    public static class GetUserBySaguntinoCodeQueryHandler
    {
        [WolverineGet("/api/users/saguntino_code/{saguntinoCode}")]
        [Tags("Users")]
        [EndpointSummary("Get user by saguntino code")]
        public static async Task<UserBySaguntinoCodeResponseDto?> Handle(string saguntinoCode, ISaguntoDbContext dbContext)
        {
            var user = await dbContext.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(u => u.SaguntinoCode == saguntinoCode);

            if (user == null)
                return null;
            else
                return new UserBySaguntinoCodeResponseDto(user.Id, user.Name, user.SaguntinoCode);
        }
    }
}