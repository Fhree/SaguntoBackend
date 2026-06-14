using Microsoft.AspNetCore.Http;
using Sagunto.Application.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Wolverine.Http;
using Microsoft.EntityFrameworkCore;

namespace Sagunto.Application.Features.Orders
{
    public class PayAllUnpaidOrdersByUserCommandHandler
    {
        [WolverinePost("/api/orders/{customerId}/payall")]
        [Tags("Orders")]
        [EndpointSummary("Pay all unpaid orders for a user")]
        public static async Task<IResult> Handle(int customerId, ISaguntoDbContext dbContext)
        {
            var orders = await dbContext.Orders
                .Where(o => o.CustomerId == customerId && !o.IsPaid)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return Results.Conflict(new
                {
                    Message = "No hay consumiciones pendientes o ya han sido liquidadas por otro camarero."
                });
            }

            orders.ForEach(order => order.Pay());
            await dbContext.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = "Deuda liquidada correctamente"
            });
        }
    }
}