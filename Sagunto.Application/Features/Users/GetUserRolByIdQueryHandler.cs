using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{

    public static class GetUserRolByIdQueryHandler
    {
        [WolverineGet("/api/users/{id}/role")]
        [Tags("Users")]
        [EndpointSummary("Get user role by Id")]
        public static async Task<int> Handle(int userId, ISaguntoDbContext dbContext)
        {
            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return -1;
            else
                return user.RoleId;
        }
    }
}
