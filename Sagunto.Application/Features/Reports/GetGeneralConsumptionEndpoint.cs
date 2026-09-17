using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Backend.Features.Reports
{
    public record CustomerConsumptionSummaryDto(
        int CustomerId,
        string Name,
        string Surname,
        decimal TotalConsumed
    );

    public record CustomerOrderDetailDto(
        Guid OrderId,
        decimal OrderTotal,
        bool IsPaid,
        string ProductName,
        int Quantity,
        decimal PriceSnapshot
    );

    public static class GetGeneralConsumptionEndpoint
    {
        [WolverineGet("/api/reports/general-consumption")]
        [Tags("Reports")]
        [EndpointSummary("Get the total consumption grouped by customer.")]
        public static async Task<List<CustomerConsumptionSummaryDto>> Handle(ISaguntoDbContext db, CancellationToken ct)
        {
            return await db.Orders.AsNoTracking()
                .GroupBy(x => new { x.CustomerId, x.User.Name, x.User.Surname })
                .OrderBy(g => g.Key.CustomerId)
                .Select(g => new CustomerConsumptionSummaryDto(
                    g.Key.CustomerId.Value,
                    g.Key.Name,
                    g.Key.Surname,
                    g.Sum(x => x.Total)
                ))
                .ToListAsync(ct);
        }
    }
}