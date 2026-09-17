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
        decimal PriceSnapshot,
        DateTime Date
    );

    public static class GetGeneralConsumptionEndpoint
    {
        [WolverineGet("/api/reports/general-consumption")]
        [Tags("Reports")]
        [EndpointSummary("Get the total consumption grouped by customer.")]
        public static async Task<List<CustomerConsumptionSummaryDto>> Handle(ISaguntoDbContext db, CancellationToken ct)
        {
            return await db.Orders.AsNoTracking()
                .GroupBy(x => new { x.CustomerId, x.Customer.Name, x.Customer.Surname })
                .OrderBy(g => g.Key.CustomerId)
                .Where(g => g.Key.CustomerId.HasValue && g.Key.CustomerId > 0)
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