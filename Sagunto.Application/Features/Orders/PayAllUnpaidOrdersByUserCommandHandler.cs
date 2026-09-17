using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record PayAllUnpaidOrdersByUserRequest(List<Guid> OrderIds);

    public class PayAllUnpaidOrdersByUserCommandHandler
    {
        [WolverinePost("/api/orders/{customerId}/payAll")]
        [Tags("Orders")]
        [EndpointSummary("Pay specific unpaid orders for a customer")]
        public static async Task<IResult> Handle(int customerId, PayAllUnpaidOrdersByUserRequest request, ISaguntoDbContext dbContext)
        {
            if (request.OrderIds == null || !request.OrderIds.Any())
                return Results.BadRequest(new { Message = "No se indicaron pedidos para liquidar." });

            var orders = await dbContext.Orders
                .Where(o => o.CustomerId == customerId
                         && request.OrderIds.Contains(o.Id)
                         && !o.IsPaid)
                .ToListAsync();

            if (!orders.Any())
                return Results.Ok(new { Message = "No hay consumiciones pendientes de las solicitadas." });

            orders.ForEach(o => o.Pay());
            await dbContext.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = $"{orders.Count} consumición(es) liquidada(s) correctamente."
            });
        }
    }
}