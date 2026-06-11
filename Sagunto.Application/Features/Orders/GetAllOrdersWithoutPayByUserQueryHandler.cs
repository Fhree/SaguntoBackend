using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record OrderDto(int Id, decimal Total, string Date, List<OrderLineDto> OrderLines);
    public record OrderLineDto(string Name, int Quantity, decimal Price);

    public class GetAllOrdersWithoutPayByUserQueryHandler
    {

        [WolverineGet("/api/orders/{customerId}/without-payment")]
        [Tags("Orders")]
        [EndpointSummary("Get all orders without payment")]
        public static async Task<List<OrderDto>> Handle(int customerId, ISaguntoDbContext dbContext)
        {
            return await dbContext.Orders.AsNoTracking()
                .Where(o => o.CustomerId == customerId && !o.IsPaid)
                .Select(o => new OrderDto(
                    o.Id,
                    o.Total,
                    o.CreatedAt.ToString("dd-MM-yyyy HH:mm"),
                    dbContext.OrderLines
                        .Where(ol => ol.OrderId == o.Id)
                        .Select(ol => new OrderLineDto(
                            ol.Product!.Name,
                            ol.Quantity,
                            ol.PriceSnapshot
                        )).ToList()
                ))
                .ToListAsync();
        }
    }
}
