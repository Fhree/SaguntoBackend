using Microsoft.AspNetCore.Http;
using Sagunto.Application.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Wolverine.Http;
using Microsoft.EntityFrameworkCore;

namespace Sagunto.Application.Features.Orders
{
    public record PayOrdersRequest(List<Guid> OrderIds);

    public class PayOrdersByIdsCommandHandler
    {
        [WolverinePost("/api/orders/{customerId}/payAll")]
        [Tags("Orders")]
        [EndpointSummary("Pay specific unpaid orders for a customer")]
        public static async Task<IResult> Handle(
            int customerId,
            PayOrdersRequest request,
            ISaguntoDbContext dbContext)
        {
            if (request.OrderIds == null || !request.OrderIds.Any())
            {
                return Results.BadRequest(new
                {
                    Message = "Debe proporcionar al menos un identificador de pedido para liquidar."
                });
            }

            // Filtramos solo los pedidos que pertenezcan al cliente, estén en la lista enviada y sigan impagados
            var orders = await dbContext.Orders
                .Where(o => o.CustomerId == customerId
                         && request.OrderIds.Contains(o.Id)
                         && !o.IsPaid)
                .ToListAsync();

            if (!orders.Any())
            {
                // Retornamos 200/Ok para garantizar idempotencia si un worker reintenta tras un timeout
                return Results.Ok(new
                {
                    Message = "Los pedidos seleccionados ya han sido liquidados previamente o no existen."
                });
            }

            orders.ForEach(order => order.Pay());
            await dbContext.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = $"{orders.Count} consumición(es) liquidada(s) correctamente.",
                SettledOrderIds = orders.Select(o => o.Id).ToList()
            });
        }
    }
}