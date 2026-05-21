using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record OrderDto(int Id, decimal Total);

    public class GetAllOrdersWithoutPayByUserQueryHandler
    {

        [WolverineGet("/api/orders/{customerId}/without-payment")]
        [Tags("Orders")]
        [EndpointSummary("Get all orders without payment")]
        public static async Task<List<OrderDto>> Handle(int customerId, ISaguntoDbContext dbContext)
        {
            return await dbContext.Orders.AsNoTracking()
                .Where(o => o.CustomerId == customerId && !o.IsPaid)
                .Select(o => new OrderDto(o.Id, o.Total)).ToListAsync();
        }
    }
}
