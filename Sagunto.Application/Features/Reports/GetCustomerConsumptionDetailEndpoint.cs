using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Backend.Features.Reports
{

    public static class GetCustomerConsumptionDetailEndpoint
    {
        [WolverineGet("/api/reports/customers/{customerId}/detail")]
        [Tags("Reports")]
        [EndpointSummary("Get customer consumption detail.")]
        public static async Task<List<CustomerOrderDetailDto>> Handle(int customerId, ISaguntoDbContext db, CancellationToken ct)
        {
            return await db.Orders
                .Where(o => o.CustomerId == customerId)
                .Join(
                    db.OrderLines,
                    o => o.Id,
                    ol => ol.OrderId,
                    (o, ol) => new { o, ol }
                )
                .Join(
                    db.Products,
                    combined => combined.ol.ProductId,
                    p => p.Id,
                    (combined, p) => new CustomerOrderDetailDto(
                        combined.o.Id,
                        combined.o.Total,
                        combined.o.IsPaid,
                        p.Name,
                        combined.ol.Quantity,
                        combined.ol.PriceSnapshot
                    )
                )
                .ToListAsync(ct);
        }
    }
}