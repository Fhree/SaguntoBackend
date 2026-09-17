using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Reports
{
    public record SaguntinoDto(string Id, string Name, string Surname, string SaguntinoCode);

    public class GetSaguntinosQueryHandler
    {
        [WolverineGet("/api/reports/saguntinos")]
        [Tags("Reports")]
        [EndpointSummary("Get the list of Saguntinos")]
        public static async Task<IReadOnlyList<SaguntinoDto>> HandleAsync(ISaguntoDbContext db, CancellationToken ct)
        {
            var query = db.Users.AsNoTracking();

            return await query
                .OrderBy(s => s.Id)
                .Where(s => s.Name != "admin" && s.Name != "cam1" && s.Name != "cam2" && s.Name != "cam3" && s.Name != "cam4")
                .Select(s => new SaguntinoDto(s.FirebaseUid, s.Name, s.Surname, s.SaguntinoCode))
                .ToListAsync(ct);
        }
    }
}
