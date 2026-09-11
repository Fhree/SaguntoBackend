using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record SaguntinoOfflineDto(
        int Id,
        string Name,
        string Surname,
        string SaguntinoCode,
        string NormalizedSearch
    );

    public static class GetOfflineSaguntinosEndpoint
    {
        [WolverineGet("/api/users/saguntinos")]
        [Tags("Users")]
        [EndpointSummary("Get all saguntinos for offline sync")]
        public static async Task<List<SaguntinoOfflineDto>> Handle(ISaguntoDbContext dbContext)
        {
            int saguntinoRoleId = 2;

            return await dbContext.Users
                .AsNoTracking()
                .Where(u => u.RoleId == saguntinoRoleId)
                .Select(u => new SaguntinoOfflineDto(
                    u.Id,
                    u.Name,
                    u.Surname,
                    u.SaguntinoCode,
                    u.NormalizedSearch
                ))
                .ToListAsync();
        }
    }
}